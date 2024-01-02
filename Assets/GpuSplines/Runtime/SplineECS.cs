namespace PeDev.GpuSplines {
	public struct SplineEntity {
		/// <summary>
		/// The index of the component in the ECS array.
		/// </summary>
		internal int id;
	};

	public struct SplineComponent {
		/// <summary>
		/// The index of the entity in the ECS array.
		/// </summary>
		internal int indexEntity;
		
		/// <summary>
		/// The index of the SplineBatch list.
		/// </summary>
		internal int indexBatch;
		
		/// <summary>
		/// The index of the spline list inside the SplineBatch.
		/// </summary>
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