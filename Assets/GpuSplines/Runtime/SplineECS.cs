namespace PeDev.GpuSplines {
	public struct SplineEntity {
		/// <summary>
		/// The index of the component in the ECS array.
		/// </summary>
		public readonly int id;

		internal SplineEntity(int id) {
			this.id = id;
		}
	};

	public struct SplineComponent {
		/// <summary>
		/// The index of the entity in the ECS array.
		/// </summary>
		public int indexEntity { get; internal set; }

		/// <summary>
		/// The index of the SplineBatch list.
		/// </summary>
		public int indexBatch { get; internal set; }
		
		/// <summary>
		/// The index of the spline list inside the SplineBatch.
		/// </summary>
		public int indexInBatchSplines { get; internal set; }
		
		
		public int startIndexControlPoint { get; internal set; }
		public int endIndexControlPoint => startIndexControlPoint + numControlPoints;
		public int numControlPoints { get; internal set; }

		public int startIndexVertices { get; internal set; }
		public int numVerticesPerSegment { get; internal set; }
		public int numVertices => GetNumVertices(this.numControlPoints, this.numVerticesPerSegment);

		public static int GetNumVertices(int numControlPoints, int numVerticesPerSegment) {
			return (numControlPoints - 3) * numVerticesPerSegment;
		}
	}
}