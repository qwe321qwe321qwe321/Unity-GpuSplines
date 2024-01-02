using System;
using UnityEngine;

namespace PeDev.GpuSplines {
	public class GpuSplineManager : MonoBehaviour {
		#region Singleton
		private static GpuSplineManager s_Instance;

		public static GpuSplineManager Instance {
			get {
				if (s_Instance){
					return s_Instance;
				}

				s_Instance = FindObjectOfType<GpuSplineManager>();
				if (s_Instance) {
					return s_Instance;
				}
				
				s_Instance = new GameObject("Gpu Spline Manager").AddComponent<GpuSplineManager>();
				return s_Instance;

			}
		}
		#endregion

		public GpuSplineContext.DrawMode renderMode;
		public bool optimizeLinearVertices;
		public bool drawControlPointsInGizmos = false;
		public bool drawBoundsInGizmos = false;
		public GpuSplineContext Context { get; } = new GpuSplineContext();

		private void Update() {
			Context.SetDrawMode(renderMode)
				.SetOptimizeLinearVertices(optimizeLinearVertices)
				.Update();
		}

		private void OnDestroy() {
			Context.Dispose();
		}


		private void OnDrawGizmosSelected() {
			if (drawControlPointsInGizmos) {
				foreach (var batch in Context.GetSplineBatches()) {
					Gizmos.color = Color.yellow;
					for (int i = 0; i < batch.numControlPoints; i++) {
						Gizmos.DrawSphere(batch.controlPoints[i], 0.1f);
					}
				}
			}
			
			if (drawBoundsInGizmos) {
				foreach (var batch in Context.GetSplineBatches()) {
					Gizmos.color = Color.green;
					Bounds bounds = batch.meshBounds;
					Gizmos.DrawWireCube(bounds.center, bounds.size);
				}
			}
		}
	}
}