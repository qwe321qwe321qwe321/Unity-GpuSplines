using Stella3D;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace PeDev.GpuSplines {
	class SplineBatch {
		public const int MAX_NUM_VERTICES = 65000;
		public const int MAX_NUM_CONTROL_POINTS = 1000;

		public bool IsEmpty() {
			return numControlPoints == 0;
		}

		internal int indexBatch; 

		public SplineBatchKey batchProperties;

		private SharedArray<Vector4, float4> m_SharedControlPoints =
			new SharedArray<Vector4, float4>(MAX_NUM_CONTROL_POINTS);
		public Vector4[] controlPoints => m_SharedControlPoints;
		public NativeArray<float4> controlPointsNativeArray => m_SharedControlPoints;
		
		private SharedArray<SplineEntity> m_SharedSplineEntities = new SharedArray<SplineEntity>(4);
		public SplineEntity[] splineEntities => m_SharedSplineEntities;
		public NativeArray<SplineEntity> splineEntitiesNativeArray => m_SharedSplineEntities;
		public int splineCount { get; private set; }

		public int numControlPoints = 0;
		public int numVertices = 0;
		
		public bool dirtyMesh = false;
		public Mesh mesh;
		public ComputeBuffer computeBuffer;

		public MaterialPropertyBlock materialProperty = new MaterialPropertyBlock();
		public bool dirtyControlPoints = false;
		
		public bool dirtyWidthColor = false;

		public Material sharedMaterial;
		public bool dirtyMaterial = false;

		public Bounds controlPointBounds;
		public Bounds meshBounds;
		public bool dirtyBounds;
		
		internal SplineBatch(SplineBatchKey key) {
			batchProperties = key;
		}

		internal void AddToSplineList(SplineEntity entity) {
			if (splineCount + 1 >= m_SharedSplineEntities.Length) {
				// Expand double.
				m_SharedSplineEntities.Resize(m_SharedSplineEntities.Length * 2);
			}

			splineEntities[splineCount] = entity;
			splineCount += 1;
		}

		internal void RemoveAtSplineList(int index) {
			if (index < 0 || index >= splineCount) { return; }
			
			// Remove the element.
			for (int i = index; i < splineCount - 1; i++) {
				splineEntities[i] = splineEntities[i + 1];
			}
			splineCount -= 1;
		}
		
		internal void SetAllDirty() {
			dirtyMesh = true;
			dirtyControlPoints = true;
			dirtyWidthColor = true;
			dirtyMaterial = true;
			dirtyBounds = true;
		}
		
		internal void CheckDirtyControlPoints() {
			if (!dirtyControlPoints) {
				return;
			}
			dirtyControlPoints = false;
			
			// Update vector array.
			Profiler.BeginSample("UpdateVectorArray");
			materialProperty.SetVectorArray(ShaderIDs._ControlPoints, controlPoints);
			Profiler.EndSample();
			
			// Need to update bounds.
			dirtyBounds = true;
		}
		
		internal void UpdateWidthColor() {
			dirtyWidthColor = false;
			
			Vector4 value = batchProperties.color;
			value.w = batchProperties.width;
			materialProperty.SetVector(ShaderIDs._ColorAndWidth, value);

			// Need to update bounds if width is dirty.
			UpdateMeshBounds();
		}

		internal void UpdateMaterial(bool drawProcedural) {
			dirtyMaterial = false;
			sharedMaterial = SharedSplineMaterial.Get(batchProperties.splineType, drawProcedural);
			
			// Need to update bounds if spline is changed.
			UpdateMeshBounds();
		}

		

		internal void UpdateMeshBounds() {
			meshBounds = controlPointBounds;
			if (batchProperties.splineType == SplineType.CatmullRom) {
				// for curve. 
				meshBounds.extents *= 1.25f;
			}
			meshBounds.Expand(batchProperties.width * 2);
		}

		internal void Dispose() {
			if (mesh) {
				UnityEngine.Object.Destroy(mesh);
			}
			computeBuffer?.Release();
			if (m_SharedControlPoints != null) {
				m_SharedControlPoints.Dispose();
				m_SharedControlPoints = null;
			}

			if (m_SharedSplineEntities != null) {
				m_SharedSplineEntities.Dispose();
				m_SharedSplineEntities = null;
				splineCount = 0;
			}
		}
	}
	
	struct SplineBatchKey : IEquatable<SplineBatchKey> {
		public float width;
		public Color color;
		public SplineType splineType;

		public bool Equals(SplineBatchKey other) => width.Equals(other.width) && color.Equals(other.color) && splineType == other.splineType;

		public override bool Equals(object obj) => obj is SplineBatchKey other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = width.GetHashCode();
				hashCode = (hashCode * 397) ^ color.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)splineType;
				return hashCode;
			}
		}
	}
	
	public enum SplineType {
		Linear,
		CatmullRom
	}

	public static class SharedSplineMaterial {
		private static readonly Dictionary<Key, Material> mapping = new Dictionary<Key, Material>();

		private static Shader s_SplineShader;
		private static Shader s_SplineShaderProcedural;

		public static Material Get(SplineType type, bool drawProcedural) {
			Key key = new Key(type, drawProcedural);
			if (mapping.TryGetValue(key, out Material value)) {
				if (value != null) {
					return value;
				}
				mapping.Remove(key);
			}

			
			Material newMaterial = new Material(GetShader(drawProcedural));
			newMaterial.EnableKeyword(ShaderIDs.SplineTypeKeywords[(int)type]);
			mapping.Add(key, newMaterial);
			return newMaterial;
		}

		private static Shader GetShader(bool drawProcedural) {
			if (drawProcedural) {
				if (s_SplineShaderProcedural == null) {
					s_SplineShaderProcedural = Shader.Find("Unlit/GpuSplineProcedural");
				}

				return s_SplineShaderProcedural;
			}
			if (s_SplineShader == null) {
				s_SplineShader = Shader.Find("Unlit/GpuSpline");
			}

			return s_SplineShader;
		}

		private struct Key : IEquatable<Key> {
			public SplineType splineType;
			public bool drawProcedural;
			
			public Key(SplineType splineType, bool drawProcedural) {
				this.splineType = splineType;
				this.drawProcedural = drawProcedural;
			}

			public bool Equals(Key other) => splineType == other.splineType && drawProcedural == other.drawProcedural;

			public override bool Equals(object obj) => obj is Key other && Equals(other);

			public override int GetHashCode()
			{
				unchecked
				{
					return ((int)splineType * 397) ^ drawProcedural.GetHashCode();
				}
			}
		}
	}
}