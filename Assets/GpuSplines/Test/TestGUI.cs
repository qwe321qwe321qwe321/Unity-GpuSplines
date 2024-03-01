using PeDev.GpuSplines;
using UnityEngine;

namespace GpuSplines.Test {
	public class TestGUI : MonoBehaviour {
		private void OnGUI() {
			var context = GpuSplineManager.Instance.Context;
			if (context != null) {
#if GPU_SPLINES_ACCUMULATE_STATISTICS
				GUI.Label(new Rect(10, 10, 500, 20), $"Control Points: {context.TotalControlPoints}");
#endif
				GUI.Label(new Rect(10, 30, 500, 20), $"Splines: {context.TotalSplines}");
				GUI.Label(new Rect(10, 50, 500, 20), $"Batches: {context.TotalBatches}");
			}
		}
	}
}