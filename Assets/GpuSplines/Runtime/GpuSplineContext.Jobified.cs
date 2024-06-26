﻿using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace PeDev.GpuSplines {
	// Jobified methods.
	public partial class GpuSplineContext {
		public struct JobifiedContext {
			[ReadOnly]
			public NativeArray<SplineEntity> entityArray;
			
			[NativeDisableParallelForRestriction]
			public NativeArray<SplineComponent> componentArray;
			
			[NativeDisableParallelForRestriction]
			public NativeArray<JobifiedSplineBatch> tempSplineBatches;

			[NativeDisableParallelForRestriction]
			public NativeArray<bool> tempSplineBatchDirtyControlPoints;
			
			[BurstCompile]
			public void ModifyPoint(SplineEntity entity, int index, in float3 point, bool insertFirstLastPoints) {
				ModifyPoint(entity, index, point.x, point.y, point.z, insertFirstLastPoints);
			}
			
			[BurstCompile]
			public void ModifyPoint(SplineEntity entity, int index, in Vector3 point, bool insertFirstLastPoints) {
				ModifyPoint(entity, index, point.x, point.y, point.z, insertFirstLastPoints);
			}
			
			[BurstCompile]
			private unsafe void ModifyPoint(SplineEntity entity, int index, float pointX, float pointY, float pointZ, bool insertFirstLastPoints) {
				SplineComponent comp = componentArray[entity.id];
				int indexRange = insertFirstLastPoints ? 
					comp.numControlPoints - 2 :
					comp.numControlPoints;
			
#if UNITY_ASSERTIONS
				// out of range check.
				if (index < 0 || index >= indexRange) {
					throw new IndexOutOfRangeException($"Index {index} is out of range {indexRange}.");
				}
#endif
			
				var batch = tempSplineBatches[comp.indexBatch];
				int startIndexInBatch = insertFirstLastPoints ? comp.startIndexControlPoint + 1 : comp.startIndexControlPoint;

				var batchCPs = batch.UnsafeControlPoints;
				batchCPs[startIndexInBatch + index] = new float4(
					pointX, pointY, pointZ, 
					batchCPs[startIndexInBatch + index].w
				);

				// Modify the first or the last if necessary.
				if (insertFirstLastPoints) {
					if (index == 0 || index == 1) {
						int firstIndex = comp.startIndexControlPoint;
						batchCPs[firstIndex] =
							batchCPs[firstIndex + 1] * 2 - batchCPs[firstIndex + 2];
					} else if (index == (indexRange - 1) || index == (indexRange - 2)) {
						int lastIndex = comp.endIndexControlPoint - 1;
						batchCPs[lastIndex] =
							batchCPs[lastIndex - 1] * 2 - batchCPs[lastIndex - 2];
					}
				}

				// Write only so there is no race condition.
				tempSplineBatchDirtyControlPoints[comp.indexBatch] = true;
			}

			[BurstCompile]
			public void ModifyPoints(SplineEntity entity, NativeArray<float3> controlPoints, int startReadIndex, int numControlPoints,
				bool insertFirstLastPoints) {
				unsafe {
					ModifyPoints(entity, (float3*)controlPoints.GetUnsafeReadOnlyPtr(), startReadIndex, numControlPoints, insertFirstLastPoints);
				}
			}

			[BurstCompile]
			public unsafe void ModifyPoints(SplineEntity entity, float3* controlPoints, int startReadIndex, int numControlPoints,
				bool insertFirstLastPoints) {
				
				int rawNumControlPoints = numControlPoints;
				// Add 2 points at the first and the last.
				if (insertFirstLastPoints) {
					numControlPoints += 2;
				}
				
				SplineComponent comp = componentArray[entity.id];
			    
#if UNITY_ASSERTIONS
				if (comp.numControlPoints != numControlPoints) {
					throw new Exception($"{comp.numControlPoints}!= {numControlPoints}");
				}
#endif

				var batch = tempSplineBatches[comp.indexBatch];
				int startModifyIndex = insertFirstLastPoints ? comp.startIndexControlPoint + 1 : comp.startIndexControlPoint;
			    
				for (int i = 0; i < rawNumControlPoints; i++) {
					int modifyIndex = startModifyIndex + i;
					int readIndex = startReadIndex + i;
					unsafe {
						batch.UnsafeControlPoints[modifyIndex] = new float4(
							controlPoints[readIndex], 
							batch.UnsafeControlPoints[modifyIndex].w);
					}
				}
				
				if (insertFirstLastPoints) {
					AddFirstAndLastPointToSplineInBatch(batch, comp.startIndexControlPoint, comp.endIndexControlPoint - 1);
				}

				// Write only so there is no race condition.
				tempSplineBatchDirtyControlPoints[comp.indexBatch] = true;
			}
			
			[BurstCompile]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void AddFirstAndLastPointToSplineInBatch(JobifiedSplineBatch batch, int firstIndex, int lastIndex) {
				// Extend the first and last points.
				// cp[0] = cp[1] + (cp[1] - cp[2])
				// cp[N] = cp[N - 1] + (cp[N - 1] - cp[N - 2])
				unsafe {
					batch.UnsafeControlPoints[firstIndex] =
						batch.UnsafeControlPoints[firstIndex + 1] * 2 - batch.UnsafeControlPoints[firstIndex + 2];
					batch.UnsafeControlPoints[lastIndex] =
						batch.UnsafeControlPoints[lastIndex - 1] * 2 - batch.UnsafeControlPoints[lastIndex - 2];
				}
			}

			public void Dispose(GpuSplineContext context) {
				if (tempSplineBatches.IsCreated) {
					context.EndJobifiedContext(this);
				}
			}
		}

		public struct JobifiedSplineBatch {
			public IntPtr controlPointArrayPtr;
			public unsafe float4* UnsafeControlPoints => ((float4*)controlPointArrayPtr.ToPointer());
			
			[NativeDisableParallelForRestriction]
			public IntPtr dirtyControlPointsPtr;
			public unsafe bool* UnsafeDirtyControlPoints =>  ((bool*)dirtyControlPointsPtr.ToPointer());
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public float4 GetControlPoint(int index) {
				unsafe {
					return UnsafeControlPoints[index];
				}
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void SetControlPoint(int index, float4 value) {
				unsafe {
					UnsafeControlPoints[index] = value;
				}
			}
		}

		/// <summary>
		/// Create a jobified context for Unity Job and Burst.
		/// It will create a temporary buffers internally for caching the SplineBatch values.
		/// So you must call EndJobifiedContext() when your job is done.  
		/// </summary>
		/// <param name="allocator"></param>
		/// <returns></returns>
		public JobifiedContext BeginJobifiedContext(Allocator allocator) {
			NativeArray<JobifiedSplineBatch> tempSplineBatches = new NativeArray<JobifiedSplineBatch>(
				m_ActiveBatchCount, allocator);
			NativeArray<bool> tempSplineBatchDirtyControlPoints = new NativeArray<bool>(
				m_ActiveBatchCount, allocator, NativeArrayOptions.ClearMemory);
			unsafe {
				for (int i = 0; i < m_ActiveBatchCount; i++) {
					tempSplineBatches[i] = new JobifiedSplineBatch() {
						controlPointArrayPtr = (IntPtr)(m_SplineBatches[i].controlPointsNativeArray.GetUnsafePtr())
					};
				}
			}
			
			return new JobifiedContext() {
				entityArray = m_SharedEntities,
				componentArray =  m_SharedComponents,
				tempSplineBatches = tempSplineBatches,
				tempSplineBatchDirtyControlPoints = tempSplineBatchDirtyControlPoints,
			};
		}

		public void EndJobifiedContext(JobifiedContext context) {
			// Apply back dirty flags.
			for (int i = 0; i < m_ActiveBatchCount; i++) {
				m_SplineBatches[i].dirtyControlPoints |= context.tempSplineBatchDirtyControlPoints[i];
			}

			context.tempSplineBatchDirtyControlPoints.Dispose();
			context.tempSplineBatches.Dispose();
		}
	}
	
	[BurstCompile]
	public struct CopyTransformPositionJob : IJobParallelForTransform {
		public NativeArray<float3> destination;

		public void Execute(int index, TransformAccess transform) {
			destination[index] = transform.position;
		}
	}
	
	[BurstCompile]
	public struct CopyTransformPositionWithIndicesJob : IJobParallelForTransform {
		[ReadOnly]
		public NativeArray<int> indices;
		
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> destination;

		public void Execute(int index, TransformAccess transform) {
			destination[indices[index]] = transform.position;
		}
	}

	public struct BatchSplineInput {
		public SplineEntity entity;
		public int startIndex;
		public int numControlPoints;
	}
	    
	/// <summary>
	/// Modify the control points of the spline with the input array.
	/// </summary>
	[BurstCompile]
	public struct ModifySplineControlPointsJob : IJobParallelFor {
		[ReadOnly]
		public NativeArray<BatchSplineInput> inputEntities;
		[ReadOnly]
		public NativeArray<float3> inputControlPoints;
		    
		public bool insertFirstLastPoints;
		    
		public GpuSplineContext.JobifiedContext splineContext;

		    
		public void Execute(int index) {
			BatchSplineInput input = inputEntities[index];
			SplineEntity entity = input.entity;
			splineContext.ModifyPoints(entity, inputControlPoints, input.startIndex, input.numControlPoints, insertFirstLastPoints);
		}
	}
	
	public struct BatchSplineInputWithArray {
		public SplineEntity entity;
		public int numControlPoints;

		// Pointer hack: to have nested native arrays.
		// WARNING: You have to STORE the NativeArray in the other place.
		// When you are gonna dispose it, just dispose the one you allocated instead of this.
		public IntPtr inputControlPointsPtr;
		public NativeArray<float3> inputControlPoints {
			get {
				unsafe {
					// This line cannot be Burst compiled.
					// You should use inputControlPointsPtr directly to leverage Burst best.
					NativeArray<float3> nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float3>
						(inputControlPointsPtr.ToPointer(), numControlPoints, Allocator.None);
#if UNITY_EDITOR
					NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
#endif
					return nativeArray;
				}
			}
			set {
				unsafe {
					inputControlPointsPtr = (IntPtr)value.GetUnsafePtr(); 
				}
			}
		}
	}
	
	[BurstCompile]
	public struct CopyTransformPositionToBatchSplineInputWithArrayJob : IJobParallelForTransform {
		[ReadOnly]
		public NativeArray<int> batchIndices;
		[ReadOnly]
		public NativeArray<int> controlPointIndices;
		
		[NativeDisableParallelForRestriction]
		public NativeArray<BatchSplineInputWithArray> destination;

		public void Execute(int index, TransformAccess transform) {
			unsafe {
				((float3*)destination[batchIndices[index]].inputControlPointsPtr.ToPointer())[controlPointIndices[index]] = transform.position;
			}
		}
	}
	    
	/// <summary>
	/// Modify the control points of the spline with the nested array as the input.
	/// </summary>
	[BurstCompile]
	public struct ModifySplineControlPointsWithNestedArraysJob : IJobParallelFor {
		[ReadOnly]
		public NativeArray<BatchSplineInputWithArray> inputs;
		    
		public bool insertFirstLastPoints;
		    
		public GpuSplineContext.JobifiedContext splineContext;

		    
		public unsafe void Execute(int index) {
			BatchSplineInputWithArray input = inputs[index];
			SplineEntity entity = input.entity;
			splineContext.ModifyPoints(entity, (float3*)input.inputControlPointsPtr.ToPointer(), 0, input.numControlPoints, insertFirstLastPoints);
		}
	}
}