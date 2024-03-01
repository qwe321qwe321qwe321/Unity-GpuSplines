using UnityEngine;

namespace PeDev.GpuSplines {
	/// <summary>
	/// The manager of the GPU spline. It is a singleton class.
	/// You don't have to directly use this manager to use the GPU spline, it is just an example of how to use the GPU spline.
	/// </summary>
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

		[Tooltip("The render method of the spline. DrawProcedural is faster than DrawMesh, but it requires compute buffer support. If the platform doesn't support compute shaders, it will automatically switch to DrawMesh.")]
		public GpuSplineContext.DrawMode renderMode;
		
		[Tooltip("If true, the spline will optimize linear vertices. This will minimize the number of vertices in the linear spline.")]
		public bool optimizeLinearVertices;
		
#if UNITY_EDITOR
		public bool drawControlPointsInGizmos = false;
		public bool drawBoundsInGizmos = false;
#endif
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


#if UNITY_EDITOR
		private void OnDrawGizmos() {
			if (drawControlPointsInGizmos) {
				GpuSplineGizmos.DrawAllControlPoints(Context, Color.yellow, 0.1f);
			}
			
			if (drawBoundsInGizmos) {
				GpuSplineGizmos.DrawAllBatchBounds(Context, Color.green);
			}
		}
#endif
	}
}