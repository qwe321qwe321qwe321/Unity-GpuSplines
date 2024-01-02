﻿using Stella3D;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace PeDev.GpuSplines {
	public class GpuSplineContext {
		private const int MinimumVerticesPerSegment = 2;
		private const int MinCapacity = 16;

		public enum DrawMode {
			DrawMesh,
			DrawProcedural
		}

		private DrawMode m_DrawMode = DrawMode.DrawMesh;
		private bool m_OptimizeLinearVertices = false;
		

		private int m_Count = 0;
		private int m_Capacity = 0;

		private SharedArray<SplineEntity> m_SharedEntities = new SharedArray<SplineEntity>(0);
		private SplineEntity[] m_Entities => m_SharedEntities;
		
		private SharedArray<SplineComponent> m_SharedComponents = new SharedArray<SplineComponent>(0);
		private SplineComponent[] m_Components => m_SharedComponents;
		private readonly List<SplineBatch> m_SplineBatches = new List<SplineBatch>();
		private int m_ActiveSplineCount = 0;
		
		
		internal IReadOnlyList<SplineBatch> GetSplineBatches() => m_SplineBatches.AsReadOnly();
		
		/// <summary>
		/// Set the mode to draw.
		/// </summary>
		/// <param name="drawMode"></param>
		public GpuSplineContext SetDrawMode(DrawMode drawMode) {
			if (m_DrawMode == drawMode) {
				return this;
			}

			if (drawMode == DrawMode.DrawProcedural && 
			    !SystemInfo.supportsComputeShaders) {
				// DrawProcedural needs to support compute buffer. 
				drawMode = DrawMode.DrawMesh;
				return SetDrawMode(drawMode);
			}
			
			m_DrawMode = drawMode;
			foreach (var batch in m_SplineBatches) {
				batch.dirtyMesh = true;
				batch.dirtyMaterial = true;
			}

			return this;
		}

		/// <summary>
		/// If true, the splines with SplineType.Linear type will be optimized to 2 vertices per segment when it added.
		/// The existed splines won't change.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public GpuSplineContext SetOptimizeLinearVertices(bool value) {
			m_OptimizeLinearVertices = value;
			return this;
		}
		
		public SplineComponent GetComponent(SplineEntity entity) {
			return m_Components[entity.id];
		}
		
		/// <summary>
		/// Get the batch that entity belongs to. 
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		internal SplineBatch GetBatch(SplineEntity entity) {
			return m_SplineBatches[m_Components[entity.id].indexBatch];
		}

		internal void Dispose() {
			if (m_SharedEntities != null) {
				m_SharedEntities.Dispose();
				m_SharedEntities = null;
			}
			if (m_SharedComponents != null) {
				m_SharedComponents.Dispose();
				m_SharedComponents = null;
			}
			
			if (m_SplineBatches != null) {
				for (int i = 0; i < m_SplineBatches.Count; i++) {
					m_SplineBatches[i].Dispose();
				}
				m_SplineBatches.Clear();
			}
			m_ActiveSplineCount = 0;
		}

		#region Add/Remove Splines
		public SplineEntity AddSpline(Vector3[] controlPoints, int numControlPoints, bool insertFirstLastPoints, 
			int numVerticesPerSegment, float width, Color color, SplineType splineType) {
			// Clamp length.
			numControlPoints = Mathf.Min(controlPoints.Length, numControlPoints);
			numVerticesPerSegment = Mathf.Max(numVerticesPerSegment, MinimumVerticesPerSegment);

			// Optimize the linear vertices.
			if (m_OptimizeLinearVertices && splineType == SplineType.Linear) {
				numVerticesPerSegment = MinimumVerticesPerSegment;
			}

			SplineEntity entity = AddSplineEntityComponent(numVerticesPerSegment);
			// Set up the batch.
			int actualNumControlPoints = insertFirstLastPoints ? numControlPoints + 2 : numControlPoints;
			SplineBatch batch = GetBatchOrCreateOne(
				new SplineBatchKey() { width = width, color = color, splineType = splineType },
				actualNumControlPoints,
				SplineComponent.GetNumVertices(actualNumControlPoints, numVerticesPerSegment)
			);
			AddSplineInBatch(batch, entity, controlPoints, numControlPoints, insertFirstLastPoints);

			return entity;
		}

		public SplineEntity AddSpline(List<Vector3> controlPoints, bool insertFirstLastPoints,
			int numVerticesPerSegment, float width, Color color, SplineType splineType) {
			int numControlPoints = controlPoints.Count;
			numVerticesPerSegment = Mathf.Max(numVerticesPerSegment, MinimumVerticesPerSegment);
			
			// Optimize the linear vertices.
			if (m_OptimizeLinearVertices && splineType == SplineType.Linear) {
				numVerticesPerSegment = MinimumVerticesPerSegment;
			}

			SplineEntity entity = AddSplineEntityComponent(numVerticesPerSegment);
			// Set up the batch.
			int actualNumControlPoints = insertFirstLastPoints ? numControlPoints + 2 : numControlPoints;
			SplineBatch batch = GetBatchOrCreateOne(
				new SplineBatchKey() { width = width, color = color, splineType = splineType },
				actualNumControlPoints,
				SplineComponent.GetNumVertices(actualNumControlPoints, numVerticesPerSegment)
			);
			AddSplineInBatch(batch, entity, controlPoints, insertFirstLastPoints);

			return entity;
		}

		/// <summary>
		/// Add a new entity and component into the ECS arrays internally.
		/// </summary>
		private SplineEntity AddSplineEntityComponent(int numVerticesPerSegment) {
			if (m_Count >= m_Capacity) {
				SetCapacity(m_Capacity * 2);
			}

			// Take a new entity.
			SplineEntity entity = m_Entities[m_Count];
			m_Count++;

			// Clear the old data.
			m_Components[entity.id] = new SplineComponent() { numVerticesPerSegment = numVerticesPerSegment, };

			return entity;
		}

		/// <summary>
		///  Remove the spline from ECS.
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public bool RemoveSpline(SplineEntity entity) {
			if (!RemoveSplineInBatch(GetBatch(entity), entity)) {
				return false;
			}

			
			// Remove swap back in m_Entities list.
			// We don't move m_Component list to make the entity link to their location.
			int removeIndex = m_Components[entity.id].indexEntity;
			int lastIndex = m_Count - 1;
			SplineEntity lastEntity = m_Entities[lastIndex];
			// Swap the last entity to the removeIndex location.
			m_Entities[removeIndex] = lastEntity;
			m_Entities[lastIndex] = entity;
			// Remember to update its indexEntity.
			m_Components[lastEntity.id].indexEntity = removeIndex;
			m_Components[entity.id].indexEntity = lastIndex;
			
			m_Count -= 1;
			return true;
		}

		#endregion

		#region Modification
		/// <summary>
		/// Modify a single control point.
		/// </summary>
		public void ModifyPoint(SplineEntity entity, int index, Vector3 point, bool insertFirstLastPoints) {
			SplineComponent comp = m_Components[entity.id];
			int indexRange = insertFirstLastPoints ? 
				comp.numControlPoints - 2 :
				comp.numControlPoints;
			
			// out of range check.
			if (index < 0 || index >= indexRange) {
				throw new IndexOutOfRangeException($"Index {index} is out of range {indexRange}.");
			}
			
			SplineBatch batch = m_SplineBatches[comp.indexBatch];
			int startIndexInBatch = insertFirstLastPoints ? comp.startIndexControlPoint + 1 : comp.startIndexControlPoint;
			batch.controlPoints[startIndexInBatch + index] = new Vector4(point.x, point.y, point.z, 0f);

			// Modify the first or the last if necessary.
			if (insertFirstLastPoints) {
				if (index == 0 || index == 1) {
					int firstIndex = comp.startIndexControlPoint;
					batch.controlPoints[firstIndex] =
						batch.controlPoints[firstIndex + 1] * 2 - batch.controlPoints[firstIndex + 2];
				} else if (index == (indexRange - 1) || index == (indexRange - 2)) {
					int lastIndex = comp.endIndexControlPoint - 1;
					batch.controlPoints[lastIndex] =
						batch.controlPoints[lastIndex - 1] * 2 - batch.controlPoints[lastIndex - 2];
				}
			}

			batch.dirtyControlPoints = true;
		}

		public void ModifyPoints(SplineEntity entity, Vector3[] controlPoints, int numControlPoints, bool insertFirstLastPoints) {
			int rawNumControlPoints = numControlPoints;
			// Add 2 points at the first and the last.
			if (insertFirstLastPoints) {
				numControlPoints += 2;
			}

			SplineComponent comp = m_Components[entity.id];
			SplineBatch batch = m_SplineBatches[comp.indexBatch];
			if (numControlPoints != comp.numControlPoints) {
				// slow path.
				Profiler.BeginSample("ModifyPoints.SlowPath");
				ModifyControlPointsInBatches(entity, controlPoints, rawNumControlPoints, insertFirstLastPoints);
				Profiler.EndSample();
				return;
			}
			
			// fast path.
			Profiler.BeginSample("ModifyPoints.FastPath");
			int appendIndex = insertFirstLastPoints ? comp.startIndexControlPoint + 1 : comp.startIndexControlPoint;

			for (int i = 0; i < rawNumControlPoints; i++) {
				batch.controlPoints[appendIndex + i] =
					new Vector4(controlPoints[i].x, controlPoints[i].y, controlPoints[i].z, 0f);
			}
				
			if (insertFirstLastPoints) {
				AddFirstAndLastPointToSplineInBatch(batch, comp.startIndexControlPoint, comp.endIndexControlPoint - 1);
			}
			Profiler.EndSample();

			batch.dirtyControlPoints = true;
		}

		public void ModifyPoints(SplineEntity entity, List<Vector3> controlPoints, bool insertFirstLastPoints) {
			int numControlPoints = controlPoints.Count;
			
			int rawNumControlPoints = numControlPoints;
			// Add 2 points at the first and the last.
			if (insertFirstLastPoints) {
				numControlPoints += 2;
			}

			SplineComponent comp = m_Components[entity.id];
			SplineBatch batch = m_SplineBatches[comp.indexBatch];
			if (numControlPoints != comp.numControlPoints) {
				// slow way.
				ModifyControlPointsInBatches(entity, controlPoints, insertFirstLastPoints);
				return;
			}

			// fast way.
			int appendIndex = insertFirstLastPoints ? comp.startIndexControlPoint + 1 : comp.startIndexControlPoint;
			
			for (int i = 0; i < rawNumControlPoints; i++) {
				batch.controlPoints[appendIndex] =
					new Vector4(controlPoints[i].x, controlPoints[i].y, controlPoints[i].z, 0f);
				appendIndex += 1;
			}

			if (insertFirstLastPoints) {
				AddFirstAndLastPointToSplineInBatch(batch, comp.startIndexControlPoint, appendIndex);
			}

			batch.dirtyControlPoints = true;
		}

		/// <summary>
		/// Modify the number of vertices per segment in the specific spline.
		/// </summary>
		public void ModifyVerticesPerSegment(SplineEntity entity, int value) {
			value = Mathf.Max(value, MinimumVerticesPerSegment);
			
			// Optimize linear vertices.
			if (m_OptimizeLinearVertices && 
			    GetBatch(entity).batchProperties.splineType == SplineType.Linear) {
				value = MinimumVerticesPerSegment;
			}

			ModifyVerticesPerSegmentInternal(entity, value);
		}
		
		private void ModifyVerticesPerSegmentInternal(SplineEntity entity, int value) {
			ref SplineComponent comp = ref m_Components[entity.id];
			if (comp.numVerticesPerSegment == value) {
				return;
			}
			SplineBatch belongBatch = m_SplineBatches[comp.indexBatch];
			
			int numVerticesDiff = SplineComponent.GetNumVertices(comp.numControlPoints, value) - comp.numVertices;
			
			// Check the number of vertices is out of range.
			if ((belongBatch.numVertices + numVerticesDiff) > SplineBatch.MAX_NUM_VERTICES) {
				// Slow path: Move to the new batch.
				SplineBatch newBatch = GetBatchOrCreateOne(belongBatch.batchProperties, 
					belongBatch.numControlPoints, 
					belongBatch.numVertices + numVerticesDiff
					);
				MoveSplineToBatch(newBatch, entity);
				//Debug.LogWarning($"Move from {belongBatch.indexBatch} to {newBatch.indexBatch}");
				
				// To ensure the consistency of numVertices during moving.
				// I must assign the numVerticesPerSegment after moving.
				comp.numVerticesPerSegment = value;
				// The spline in the new batch has to be the last one, so we don't need to update other splines. 
				newBatch.numVertices += numVerticesDiff;
				return;
			}
			
			// Fast path.
			// update the spline.
			comp.numVerticesPerSegment = value;
			
			// update the following splines in the list. 
			for (int i = comp.indexInBatchSplines + 1; i < belongBatch.splineCount; i++) {
				m_Components[belongBatch.splineEntities[i].id].startIndexVertices += numVerticesDiff;
			}
			belongBatch.numVertices += numVerticesDiff;
			belongBatch.dirtyMesh = true;
		}
		
		
		public void ModifyWidth(SplineEntity entity, float width) {
			SplineBatchKey prop = GetBatch(entity).batchProperties;
			prop.width = width;
			ModifyProperties(entity, prop);
		}
		
		public void ModifyColor(SplineEntity entity, Color color) {
			SplineBatchKey prop = GetBatch(entity).batchProperties;
			prop.color = color;
			ModifyProperties(entity, prop);
		}
		
		public void ModifySplineType(SplineEntity entity, SplineType splineType) {
			SplineBatchKey prop = GetBatch(entity).batchProperties;
			prop.splineType = splineType;
			ModifyProperties(entity, prop);
			
			// Optimize linear vertices.
			if (m_OptimizeLinearVertices && splineType == SplineType.Linear) {
				// After prop changes, do segment changes.
				if (m_Components[entity.id].numVerticesPerSegment != MinimumVerticesPerSegment) {
					ModifyVerticesPerSegmentInternal(entity, MinimumVerticesPerSegment);
				}
			}
		}
		
		public void ModifySplineType(SplineEntity entity, SplineType splineType, int numVerticesPerSegment) {
			SplineBatchKey prop = GetBatch(entity).batchProperties;
			prop.splineType = splineType;
			ModifyProperties(entity, prop, numVerticesPerSegment);
		}
		
		public void ModifyProperties(SplineEntity entity, float width, Color color, SplineType splineType) {
			SplineBatchKey prop = new SplineBatchKey() { width = width, color = color, splineType = splineType };
			ModifyProperties(entity, prop);
			
			// Optimize linear vertices.
			if (m_OptimizeLinearVertices && splineType == SplineType.Linear) {
				// After prop changes, do segment changes.
				if (m_Components[entity.id].numVerticesPerSegment != MinimumVerticesPerSegment) {
					ModifyVerticesPerSegmentInternal(entity, MinimumVerticesPerSegment);
				}
			}
		}
		public void ModifyProperties(SplineEntity entity, float width, Color color, SplineType splineType, int numVerticesPerSegment) {
			SplineBatchKey prop = new SplineBatchKey() { width = width, color = color, splineType = splineType };
			ModifyProperties(entity, prop, numVerticesPerSegment);
		}

		private void ModifyProperties(SplineEntity entity, SplineBatchKey prop, int numVerticesPerSegment) {
			ModifyProperties(entity, prop);
			
			// Optimize linear vertices.
			if (m_OptimizeLinearVertices && prop.splineType == SplineType.Linear) {
				numVerticesPerSegment = MinimumVerticesPerSegment;
			}
			
			// After prop changes, do segment changes.
			if (m_Components[entity.id].numVerticesPerSegment != numVerticesPerSegment) {
				ModifyVerticesPerSegmentInternal(entity, numVerticesPerSegment);
			}
		}

		private void ModifyProperties(SplineEntity entity, SplineBatchKey prop) {
			SplineBatch batch = m_SplineBatches[m_Components[entity.id].indexBatch];
			if (prop.Equals(batch.batchProperties)) {
				// no need to change.
				return;
			}

			
			if (batch.splineCount == 1) {
				// Fast path: just modify the belong batch.
				batch.dirtyWidthColor = true;
				if (batch.batchProperties.splineType != prop.splineType) {
					batch.dirtyMaterial = true;
				}
				batch.batchProperties = prop;
			} else {
				// Slow path:
				// Move to another batch.
                SplineBatch newBatch = GetBatchOrCreateOne(prop,
                	m_Components[entity.id].numControlPoints,
                	m_Components[entity.id].numVertices);
                MoveSplineToBatch(newBatch, entity);
			}
		}

		#endregion

		#region Render

		public void Update() {
			for (int i = 0; i < m_ActiveSplineCount; i++) {
				SplineBatch batch = m_SplineBatches[i];
				if (batch.IsEmpty()) {
					Debug.LogWarning($"Weird behaviour: The batch should not be empty if it is active. {i}/{m_ActiveSplineCount}");
					continue;
				}
				
				if (batch.dirtyMesh) {
					Profiler.BeginSample("GenerateMesh");
					GenerateMesh(batch);
					Profiler.EndSample();
				}

				if (batch.dirtyMaterial) {
					Profiler.BeginSample("UpdateSplineType");
					batch.UpdateMaterial(m_DrawMode == DrawMode.DrawProcedural);
					Profiler.EndSample();
				}
				
				if (batch.dirtyControlPoints) {
					Profiler.BeginSample("CheckDirtyControlPoints");
					batch.CheckDirtyControlPoints();
					Profiler.EndSample();
				}
				
				if (batch.dirtyWidthColor) {
					Profiler.BeginSample("UpdateWidthColor");
					batch.UpdateWidthColor();
					Profiler.EndSample();
				}

				if (batch.dirtyBounds) {
					UpdateBatchBounds(batch);
				}
				
				switch (m_DrawMode) {
					case DrawMode.DrawMesh: {
						Profiler.BeginSample("Graphics.DrawMesh");
						batch.mesh.bounds = batch.meshBounds;
						Graphics.DrawMesh(batch.mesh, Matrix4x4.identity,
							batch.sharedMaterial, 0, null, 0, batch.materialProperty,
							false, false, false);
						Profiler.EndSample();
					}
						break;
					case DrawMode.DrawProcedural: {
						Profiler.BeginSample("Graphics.DrawProcedural");
						Bounds bounds = batch.meshBounds;
						// number of indices array = splineCount * (vertices of a spline - 1) * (3 for triangle) * (2 for quad). 
						int numIndices = (batch.numVertices - batch.splineCount) * 6;
						Graphics.DrawProcedural(batch.sharedMaterial, bounds,
							MeshTopology.Triangles, numIndices, 1, null, batch.materialProperty,
							ShadowCastingMode.Off, false, 0);

						Profiler.EndSample();
					}
						break;
				}
			}
		}
		
		private void GenerateMesh(SplineBatch batch) {
			batch.dirtyMesh = false;
			if (m_DrawMode == DrawMode.DrawMesh) {
				if (batch.mesh == null) {
					batch.mesh = new Mesh();
					batch.mesh.name = "Spline Batch";
					batch.mesh.hideFlags = HideFlags.DontSave;
				}
				Profiler.BeginSample("Jobified Generate Mesh");
				int verticesCount = batch.numVertices * 2;
				int indicesCount = (batch.numVertices - batch.splineCount) * 6;
				using (NativeArray<float3> verticesBuffer = new NativeArray<float3>(verticesCount, Allocator.TempJob,
					       NativeArrayOptions.UninitializedMemory))
				using (NativeArray<int> triangleBuffer = new NativeArray<int>(indicesCount, Allocator.TempJob,
					       NativeArrayOptions.UninitializedMemory))
				using (NativeArray<Color32> colorBuffer = new NativeArray<Color32>(verticesCount, Allocator.TempJob,
					       NativeArrayOptions.UninitializedMemory)) {


					Profiler.BeginSample("CreateSplineMeshJob");
					// Job.
					var meshJob = new CreateSplineMeshJob() {
						splineIndices = batch.splineEntitiesNativeArray,
						splineComponents = m_SharedComponents,
						vertices = verticesBuffer,
						triangles = triangleBuffer,
						colors = colorBuffer,
					};
					if (batch.splineCount > 4) {
						meshJob.Schedule(batch.splineCount, 4)
							.Complete();
					} else {
						// 數量少的時候沒什麼必要 multithreading ，直接在 main thread 做完
						meshJob.Run(batch.splineCount);
					}
					Profiler.EndSample();


					Profiler.BeginSample("Apply to mesh");
					batch.mesh.Clear();
					batch.mesh.SetVertices(verticesBuffer);
					batch.mesh.SetColors(colorBuffer);
					batch.mesh.SetIndices(triangleBuffer, MeshTopology.Triangles, 0, false);
					Profiler.EndSample();
				}

				Profiler.EndSample();
			} else if (m_DrawMode == DrawMode.DrawProcedural) {
				if (batch.computeBuffer == null || batch.computeBuffer.count < batch.numVertices) {
					if (batch.computeBuffer != null) {
						batch.computeBuffer.Release();
					}
					batch.computeBuffer = new ComputeBuffer(batch.numVertices, Marshal.SizeOf(typeof(ProceduralSegment)), ComputeBufferType.Default);
				}

				Profiler.BeginSample("Jobified Generate Mesh");
				using (var segmentBuffer = new NativeArray<ProceduralSegment>(batch.numVertices, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)) { 
					
					Profiler.BeginSample("CreateSplineProceduralDataJob");
					var job = new CreateSplineProceduralDataJob() {
						splineIndices = batch.splineEntitiesNativeArray,
						splineComponents = m_SharedComponents,
						segmentBuffer = segmentBuffer
					};
					
					if (batch.splineCount > 4) {
						job.Schedule(batch.splineCount, 4)
							.Complete();
					} else {
						// 數量少的時候沒什麼必要 multithreading ，直接在 main thread 做完
						job.Run(batch.splineCount);
					}
					Profiler.EndSample();

					Profiler.BeginSample("Apply to buffer");
					batch.computeBuffer.SetData(segmentBuffer);
					Profiler.EndSample();
					Profiler.BeginSample("materialProperty.SetBuffer");
					batch.materialProperty.SetBuffer(ShaderIDs._SegmentBuffer, batch.computeBuffer);
					Profiler.EndSample();
				}
				Profiler.EndSample();
			}
		}
		
		private void UpdateBatchBounds(SplineBatch batch) {
			batch.dirtyBounds = false;
			using (NativeArray<Vector3> nativeResult =
			       new NativeArray<Vector3>(2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
			{
				new CalculateBoundJob()
				{
					splineIndices =  batch.splineEntitiesNativeArray,
					splineComponents = m_SharedComponents,
					controlPoints = batch.controlPointsNativeArray,
					splineCount = batch.splineCount,
					result = nativeResult,
				}.Run();
				Vector3 minPos = nativeResult[0];
				Vector3 maxPos = nativeResult[1];
				// assign bounds.
				batch.controlPointBounds.SetMinMax(minPos, maxPos);
			}
			
			batch.UpdateMeshBounds();
		}
		#endregion
		
		#region Batches
		private SplineBatch GetBatchOrCreateOne(SplineBatchKey batchKey, int numControlPoints, int numVertices) {
			for (int i = 0; i < m_ActiveSplineCount; i++) {
				if (m_SplineBatches[i].batchProperties.Equals(batchKey) &&
				    (m_SplineBatches[i].numVertices + numVertices) <= SplineBatch.MAX_NUM_VERTICES &&
				    (m_SplineBatches[i].numControlPoints + numControlPoints) <= SplineBatch.MAX_NUM_CONTROL_POINTS) {
					return m_SplineBatches[i];
				}
			}

			if (m_ActiveSplineCount < m_SplineBatches.Count) {
				// Take from inactive batches.
				SplineBatch activeBatch = m_SplineBatches[m_ActiveSplineCount];
				activeBatch.batchProperties = batchKey;
				activeBatch.SetAllDirty();
				m_ActiveSplineCount += 1;
				Debug.Log($"Take from inactive batches. Now active batches: {m_ActiveSplineCount}");
				return activeBatch;
			}

			// Create new one.
			SplineBatch newBatch = new SplineBatch(batchKey);
			newBatch.indexBatch = m_SplineBatches.Count;
			newBatch.SetAllDirty();
			m_SplineBatches.Add(newBatch);
			m_ActiveSplineCount += 1;
			Debug.Log($"Created new batch. Now there are ({m_ActiveSplineCount}/{m_SplineBatches.Count}) active batches, {m_Count} splines.");
			return newBatch;
		}

		private void ModifyControlPointsInBatches(SplineEntity entity, Vector3[] controlPoints, int numControlPoints, bool insertFirstLastPoints) {
			SplineBatch removeFromBatch = GetBatch(entity);
			SplineBatchKey batchKey = removeFromBatch.batchProperties;
			RemoveSplineInBatch(removeFromBatch, entity);
			
			int actualNumControlPoints = insertFirstLastPoints ? numControlPoints + 2 : numControlPoints;
			SplineBatch batch = GetBatchOrCreateOne(
				batchKey,
				actualNumControlPoints,
				SplineComponent.GetNumVertices(actualNumControlPoints, m_Components[entity.id].numVerticesPerSegment)
			);
			AddSplineInBatch(batch, entity, controlPoints, numControlPoints, insertFirstLastPoints);
		}

		private void ModifyControlPointsInBatches(SplineEntity entity, List<Vector3> controlPoints, bool insertFirstLastPoints) {
			SplineBatch removeFromBatch = GetBatch(entity);
			SplineBatchKey batchKey = removeFromBatch.batchProperties;
			RemoveSplineInBatch(removeFromBatch, entity);

			int numControlPoints = controlPoints.Count;
			int actualNumControlPoints = insertFirstLastPoints ? numControlPoints + 2 : numControlPoints;
			SplineBatch batch = GetBatchOrCreateOne(
				batchKey,
				actualNumControlPoints,
				SplineComponent.GetNumVertices(actualNumControlPoints, m_Components[entity.id].numVerticesPerSegment)
			);
			AddSplineInBatch(batch, entity, controlPoints, insertFirstLastPoints);
		}

		private void AddSplineInBatch(SplineBatch batch, SplineEntity entity, Vector3[] controlPoints, int numControlPoints, bool insertFirstLastPoints) {
			int startIndexControlPoint = batch.numControlPoints;
			int rawNumControlPoints = numControlPoints;
			// Add 2 points at the first and the last.
			if (insertFirstLastPoints) {
				numControlPoints += 2;
			}
			
			m_Components[entity.id].indexBatch = batch.indexBatch;
			m_Components[entity.id].indexInBatchSplines = batch.splineCount;
			m_Components[entity.id].numControlPoints = numControlPoints;
			m_Components[entity.id].startIndexControlPoint = startIndexControlPoint;
			m_Components[entity.id].startIndexVertices = batch.numVertices;
			batch.AddToSplineList(entity);

			int appendIndex = insertFirstLastPoints ? startIndexControlPoint + 1 : startIndexControlPoint;
			for (int i = 0; i < rawNumControlPoints; i++) {
				batch.controlPoints[appendIndex] =
					new Vector4(controlPoints[i].x, controlPoints[i].y, controlPoints[i].z, 0f);
				appendIndex += 1;
			}

			if (insertFirstLastPoints) {
				AddFirstAndLastPointToSplineInBatch(batch, startIndexControlPoint, appendIndex);
			}

			batch.numControlPoints += numControlPoints;
			batch.numVertices += m_Components[entity.id].numVertices;
			batch.dirtyMesh = true;
			batch.dirtyControlPoints = true;
		}

		private void AddSplineInBatch(SplineBatch batch, SplineEntity entity, List<Vector3> controlPoints, bool addFirstLastPoints) {
			int numControlPoints = controlPoints.Count;
			int startIndexControlPoint = batch.numControlPoints;
			int rawNumControlPoints = numControlPoints;
			// Add 2 points at the first and the last.
			if (addFirstLastPoints) {
				numControlPoints += 2;
			}
			
			m_Components[entity.id].indexBatch = batch.indexBatch;
			m_Components[entity.id].indexInBatchSplines = batch.splineCount;
			m_Components[entity.id].numControlPoints = numControlPoints;
			m_Components[entity.id].startIndexControlPoint = startIndexControlPoint;
			m_Components[entity.id].startIndexVertices = batch.numVertices;
			batch.AddToSplineList(entity);

			int appendIndex = addFirstLastPoints ? startIndexControlPoint + 1 : startIndexControlPoint;
			for (int i = 0; i < rawNumControlPoints; i++) {
				batch.controlPoints[appendIndex] =
					new Vector4(controlPoints[i].x, controlPoints[i].y, controlPoints[i].z, 0f);
				appendIndex += 1;
			}

			if (addFirstLastPoints) {
				AddFirstAndLastPointToSplineInBatch(batch, startIndexControlPoint, appendIndex);
			}

			batch.numControlPoints += numControlPoints;
			batch.numVertices += m_Components[entity.id].numVertices;
			batch.dirtyMesh = true;
			batch.dirtyControlPoints = true;
		}

		private void AddFirstAndLastPointToSplineInBatch(SplineBatch batch, int firstIndex, int lastIndex) {
			// Extend the first and last points.
			// cp[0] = cp[1] + (cp[1] - cp[2])
			// cp[N] = cp[N - 1] + (cp[N - 1] - cp[N - 2])
			batch.controlPoints[firstIndex] =
				batch.controlPoints[firstIndex + 1] * 2 - batch.controlPoints[firstIndex + 2];
			batch.controlPoints[lastIndex] =
				batch.controlPoints[lastIndex - 1] * 2 - batch.controlPoints[lastIndex - 2];
		}

		private bool RemoveSplineInBatch(SplineBatch batch, SplineEntity entity) {
			SplineComponent removeComponent = m_Components[entity.id];
			int removeNumControlPoints = removeComponent.numControlPoints;
			int removeNumVertices = removeComponent.numVertices;

			int count = batch.splineCount;
			int removeIndex = removeComponent.indexInBatchSplines;
			
			// Remove Range.
			Array.Copy(batch.controlPoints, removeComponent.endIndexControlPoint,
				batch.controlPoints, removeComponent.startIndexControlPoint,
				(batch.numControlPoints - removeComponent.endIndexControlPoint));
			batch.numControlPoints -= removeNumControlPoints;
			batch.numVertices -= removeNumVertices;
			
			// Update the indices in components.
			SplineEntity[] batchSplines = batch.splineEntities;
			for (int i = removeIndex + 1; i < count; i++) {
				var id = batchSplines[i].id;
				m_Components[id].startIndexControlPoint -= removeNumControlPoints;
				m_Components[id].startIndexVertices -= removeNumVertices;
				m_Components[id].indexInBatchSplines -= 1;
			}

			// Remove in batch.
			batch.RemoveAtSplineList(removeIndex);

			batch.dirtyMesh = true;
			batch.dirtyControlPoints = true;

			if (batch.IsEmpty()) {
				if (m_ActiveSplineCount > 0) {
					// Move the batch to the last active pos.
					int swapIndex = batch.indexBatch;
					int lastIndex = m_ActiveSplineCount - 1;
					// Move the last one to the position.
					SplineBatch lastActiveBatch = m_SplineBatches[lastIndex];
					m_SplineBatches[swapIndex] = lastActiveBatch;
					lastActiveBatch.indexBatch = swapIndex;
					// Update all splines' index.
					for (int i = 0; i < lastActiveBatch.splineCount; i++) {
						m_Components[lastActiveBatch.splineEntities[i].id].indexBatch = swapIndex;
					}
					// Move the empty one to the last index.
					m_SplineBatches[lastIndex] = batch;
					batch.indexBatch = lastIndex;
					m_ActiveSplineCount -= 1;
				}
				Debug.Log($"Removed all spline in this batch. Active Batch Count: {m_ActiveSplineCount}");
			}
			return true;
		}

		private void MoveSplineToBatch(SplineBatch toBatch, SplineEntity entity) {
			var srcBatch = GetBatch(entity);
			if (srcBatch == toBatch) {
				return;
			}

			// Copy to the new batch.
			Array.Copy(srcBatch.controlPoints, m_Components[entity.id].startIndexControlPoint,
				toBatch.controlPoints, toBatch.numControlPoints,
				m_Components[entity.id].numControlPoints);

			// Remove in old batch.
			RemoveSplineInBatch(srcBatch, entity);
			
			// set up the values for new batch.
			int addIndex = toBatch.splineCount;
			m_Components[entity.id].indexBatch = toBatch.indexBatch;
			m_Components[entity.id].indexInBatchSplines = addIndex;
			m_Components[entity.id].startIndexControlPoint = toBatch.numControlPoints;
			m_Components[entity.id].startIndexVertices = toBatch.numVertices;
			
			toBatch.numControlPoints += m_Components[entity.id].numControlPoints;
			toBatch.numVertices += m_Components[entity.id].numVertices;
			toBatch.AddToSplineList(entity);

			toBatch.dirtyMesh = true;
			toBatch.dirtyControlPoints = true;
		}

		#endregion

		private void SetCapacity(int newCapacity) {
			newCapacity = Mathf.Max(newCapacity, MinCapacity);
			if (newCapacity <= m_Capacity) {
				return;
			}
			
			m_SharedEntities.Resize(newCapacity);
			m_SharedComponents.Resize(newCapacity);

			// Allocate new entity ids.
			for (int i = m_Capacity; i < newCapacity; i++) {
				m_Entities[i] = new SplineEntity() { id = i };
			}
			m_Capacity = newCapacity;

		}
	}

	static class ShaderIDs {
		public static readonly int _ControlPoints = Shader.PropertyToID("_ControlPoints");
		public static readonly int _ColorAndWidth = Shader.PropertyToID("_ColorAndWidth");
		public static readonly int _SegmentBuffer = Shader.PropertyToID("_SegmentBuffer");
		public static readonly string[] SplineTypeKeywords = new[] { "LINEAR", "CATMULLROM" };
	}
}