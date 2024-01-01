namespace PeDev.GpuSplines {
	public struct SplineEntity {
		internal int id;
	};

	public struct SplineComponent {
		internal int indexBatch;
		
		internal int indexInBatchSplines;
		
		
		internal int startIndexControlPoint;
		internal int endIndexControlPoint => startIndexControlPoint + numControlPoints;
		internal int numControlPoints;

		internal int startIndexVertices;
		internal int numVerticesPerSegment;
		internal int numVertices => GetNumVertices(this.numControlPoints, this.numVerticesPerSegment);

		public static int GetNumVertices(int numControlPoints, int numVerticesPerSegment) {
			return (numControlPoints - 3) * numVerticesPerSegment;
		}
	}
}