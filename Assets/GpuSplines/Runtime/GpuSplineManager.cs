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

		private void Awake() {
			if (renderMode == GpuSplineContext.DrawMode.DrawProcedural && !SystemInfo.supportsComputeShaders) {
				// DrawProcedural needs to support compute buffer. 
				renderMode = GpuSplineContext.DrawMode.DrawMesh;
				Debug.LogWarning("This platform doesn't support Compute Shaders, so DrawMode.DrawProcedural cannot perform");
			}
			Context.SetDrawMode(renderMode)
				.SetOptimizeLinearVertices(optimizeLinearVertices);
		}

		private void Update() {
			Context.SetDrawMode(renderMode)
				.SetOptimizeLinearVertices(optimizeLinearVertices)
				.Update();
		}

		private void OnDestroy() {
			Context.Dispose();
		}


		private void OnDrawGizmos() {
			if (drawControlPointsInGizmos) {
				GpuSplineGizmos.DrawAllControlPoints(Context, Color.yellow, 0.1f);
			}
			
			if (drawBoundsInGizmos) {
				GpuSplineGizmos.DrawAllBatchBounds(Context, Color.green);
			}
		}
	}
}