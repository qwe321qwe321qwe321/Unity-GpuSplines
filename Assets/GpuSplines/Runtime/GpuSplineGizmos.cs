using UnityEngine;

namespace PeDev.GpuSplines {
	public static class GpuSplineGizmos {
		public static void DrawAllControlPoints(GpuSplineContext context, Color color, float radius) {
			if (context == null) {
				return;
			}
			Gizmos.color = color;
			var batches = context.GetSplineBatches();
			for (int i = 0; i < context.ActiveSplineCount; i++) {
				for (int j = 0; j < batches[i].numControlPoints; j++) {
					Gizmos.DrawSphere(batches[i].controlPoints[j], radius);
				}
			}
		}
		
		public static void DrawAllBatchBounds(GpuSplineContext context, Color color) {
			if (context == null) {
				return;
			}
			Gizmos.color = color;
			var batches = context.GetSplineBatches();
			for (int i = 0; i < context.ActiveSplineCount; i++) {
				Bounds bounds = batches[i].meshBounds;
				Gizmos.DrawWireCube(bounds.center, bounds.size);
			}
		}
	}
}