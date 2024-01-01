using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace PeDev.GpuSplines {
	public class GpuLinesTestComponent : MonoBehaviour {
		public Vector3[] points = new [] {
			new Vector3(-5f, 0f, 0f),
			new Vector3(-2.5f, 5f, 0f),
			new Vector3(0f, 0f, 0f),
			new Vector3(2.5f, -5f, 0f),
			new Vector3(5f, 0f, 0f)
		};

		public float width;
		public Color color;
		public SplineType splineType;
		public int numVerticesPerSegment = 4;

		public bool createSplines = false;
		public int createNum = 10;
		public bool randomDestroySplines = false;
		public int destroyNum = 10;
		public bool randomModifySinglePoint = false;
		public bool randomModifyPoints = false;
		public bool randomModifyVerticesPerSegment = false;
		
		public bool randomModifyColors = false;
		public bool randomModifyWidth = false;
		public bool randomModifySplineType = false;
		public int modifyNum = 10;


		private GpuSplineContext m_Context => GpuSplineManager.Instance.Context;
		
		private List<SplineEntity> entities = new List<SplineEntity>();


		private void Start() {
			for (int i = 0; i < createNum; i++) {
				entities.Add(m_Context.AddSpline(
					points, points.Length, true, 
					numVerticesPerSegment, width, color, splineType));
			}
		}

		private void Update() {
			if (createSplines) {
				for (int i = 0; i < createNum; i++) {
					Profiler.BeginSample("SplineManager.AddSpline");
					var newOne = m_Context.AddSpline(
						points, points.Length, true, 
						numVerticesPerSegment, width, color, splineType);
					Profiler.EndSample();
					entities.Add(newOne);
				}
			}

			if (randomDestroySplines) {
				for (int i = 0; i < destroyNum; i++) {
					if (entities.Count == 0) {
						break;
					}

					int randIndex = Random.Range(0, entities.Count);
					SplineEntity entity = entities[randIndex];
					entities.RemoveAt(randIndex);
					
					Profiler.BeginSample("SplineManager.RemoveSpline");
					m_Context.RemoveSpline(entity);
					Profiler.EndSample();
				}
			}

			if (randomModifySinglePoint) {
				for (int i = 0; i < entities.Count; i++) {
					int randIndex = Random.Range(0, points.Length);
					points[randIndex] += Random.insideUnitSphere * 2 * Time.deltaTime;
					
					Profiler.BeginSample("SplineManager.ModifyPoint");
					m_Context.ModifyPoint(entities[i], randIndex, points[randIndex], true);
					Profiler.EndSample();
				}
			}

			if (randomModifyPoints) {
				for (int i = 0; i < entities.Count; i++) {
					SplineEntity entity = entities[i];
					int randIndex = Random.Range(0, points.Length);
					points[randIndex] += Random.insideUnitSphere * 2 * Time.deltaTime;
					
					Profiler.BeginSample("SplineManager.ModifyPoints");
					m_Context.ModifyPoints(entity, points, points.Length, true);
					Profiler.EndSample();
				}
			}

			if (randomModifyVerticesPerSegment) {
				for (int i = 0; i < entities.Count; i++) {
					SplineEntity entity = entities[i];
					int rand = Random.Range(2, numVerticesPerSegment);
					
					Profiler.BeginSample("SplineManager.ModifyVerticesPerSegment");
					m_Context.ModifyVerticesPerSegment(entity, rand);
					Profiler.EndSample();
				}
			}

			if (randomModifySplineType || randomModifyWidth || randomModifyColors) {
				for (int i = 0; i < modifyNum; i++) {
					if (entities.Count == 0) {
						break;
					}
					int randIndex = Random.Range(0, entities.Count);
					SplineEntity entity = entities[randIndex];

					if (randomModifySplineType) {
						int lineTypeNum = Random.Range(0, 2);
						SplineType splineType = (SplineType)lineTypeNum;
						Profiler.BeginSample("SplineManager.ModifySplineType");
						m_Context.ModifySplineType(entity, splineType);
						Profiler.EndSample();
					}

					if (randomModifyWidth) {
						float width = Random.Range(0.1f, 2f);
						Profiler.BeginSample("SplineManager.ModifyColor");
						m_Context.ModifyWidth(entity, width);
						Profiler.EndSample();
					}

					if (randomModifyColors) {
						Color color = Color.HSVToRGB(Random.Range(0, 256) / 255f, 1f, 1f);
						Profiler.BeginSample("SplineManager.ModifyColor");
						m_Context.ModifyColor(entity, color);
						Profiler.EndSample();
					}
				}
			}
		}
	}
}