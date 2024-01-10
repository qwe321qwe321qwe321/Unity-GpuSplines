using System;

namespace PeDev.GpuSplines {
	public readonly struct SplineEntity : IEquatable<SplineEntity> {
		/// <summary>
		/// The index of the component in the ECS array.
		/// </summary>
		public readonly int id;

		internal SplineEntity(int id) {
			this.id = id;
		}

		public static SplineEntity Invalid => new SplineEntity(-1);

		public bool IsValid() {
			return id >= 0;
		}

		public bool Equals(SplineEntity other) => id == other.id;

		public override bool Equals(object obj) => obj is SplineEntity other && Equals(other);

		public override int GetHashCode() => id;
	}

	public struct SplineComponent {
		public bool IsDummy() {
			return indexBatch < 0;
		}

		public static SplineComponent Empty = new SplineComponent() {
			indexEntity = -1,
			numVerticesPerSegment = 0,
			
			indexBatch = -1,
			indexInBatchSplines = -1,
			numControlPoints = 0,
			startIndexControlPoint = -1,
			startIndexVertices = -1,
		};

		public static SplineComponent Create(int indexEntity, int numVerticesPerSegment) {
			SplineComponent value= Empty;
			value.indexEntity = indexEntity;
			value.numVerticesPerSegment = numVerticesPerSegment;
			return value;
		}

		internal void SetDataForBatch(int indexBatch, int indexInBatchSplines, int numControlPoints, int startIndexControlPoint, int startIndexVertices) {
			this.indexBatch = indexBatch;
			this.indexInBatchSplines = indexInBatchSplines;
			this.numControlPoints = numControlPoints;
			this.startIndexControlPoint = startIndexControlPoint;
			this.startIndexVertices = startIndexVertices;
		}
		
		internal void ClearDataForBatch() {
			SplineComponent empty = Empty;
			this.indexBatch = empty.indexBatch;
			this.indexInBatchSplines = empty.indexInBatchSplines;
			this.numControlPoints = empty.numControlPoints;
			this.startIndexControlPoint = empty.startIndexControlPoint;
			this.startIndexVertices = empty.startIndexVertices;
		}
		
		/// <summary>
		/// The index of the entity in the ECS array.
		/// </summary>
		public int indexEntity { get; internal set; }

		public int numVerticesPerSegment { get; internal set; }

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
		public int numControlPoints { get; private set; }
		public int startIndexVertices { get; internal set; }
		public int numVertices => GetNumVertices(this.numControlPoints, this.numVerticesPerSegment);

		public static int GetNumVertices(int numControlPoints, int numVerticesPerSegment) {
			return Math.Max(numControlPoints - 3, 0) * numVerticesPerSegment;
		}
	}
}