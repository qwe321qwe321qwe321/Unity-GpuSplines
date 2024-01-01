using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace PeDev {
	public static class Drawing2DUtil {
		/// <summary>
		/// Get the point array of a circle with the specific parameters.
		/// </summary>
		/// <param name="center">The position of circle center</param>
		/// <param name="radius">The radius of circle</param>
		/// <param name="segments">The segments/points count which also means how precise the points are</param>
		/// <returns></returns>
		public static Vector2[] GetCirclePoints(Vector2 center, float radius, int segments = 20, float rotation = 0f) {
			if (radius <= 0) {
				throw new InvalidOperationException("radius in a circle should never be less than zero");
			}
			if (segments < 3) {
				throw new InvalidOperationException("segments in a circle should never be less than 3");
			}

			Vector2[] points = new Vector2[segments];
			GetCirclePoints(ref points, center, radius, segments, rotation);
			return points;
		}

		/// <summary>
		/// Get the point array of a circle with the specific parameters.
		/// </summary>
		/// <param name="points">The output array</param>
		/// <param name="center">The position of circle center</param>
		/// <param name="radius">The radius of circle</param>
		/// <param name="segments">The segments/points count which also means how precise the points are</param>
		/// <param name="startIndex">The start index of points to set values</param>
		/// <returns></returns>
		public static void GetCirclePoints(ref Vector2[] points, Vector2 center, float radius, int segments = 20, float rotation = 0f, int startIndex = 0) {
			if (radius <= 0) {
				throw new InvalidOperationException("radius in a circle should never be less than zero");
			}
			if (segments < 3) {
				throw new InvalidOperationException("segments in a circle should never be less than 3");
			}
			if (points == null || points.Length < segments + startIndex) {
				points = new Vector2[segments + startIndex];
			}
			float radianInterval = Mathf.PI * 2 / segments;
			float currentRadian = 0f;
			for (int i = 0; i < segments; i++, currentRadian += radianInterval) {
				points[i + startIndex] = center + new Vector2(Mathf.Cos(currentRadian), Mathf.Sin(currentRadian)) * radius;
			}

			// Rotation.
			if (rotation != 0f) {
				for (int i = 0; i < points.Length; i++) {
					points[i] = points[i].RotateAround(rotation, center);
				}
			}
		}

		public static Vector2[] GetArcPointsWithoutCenterPoint(Vector2 center, float radius, float centerAngle, float angleRange, int segments = 20) {
			Vector2[] points = new Vector2[segments];
			GetArcPointsWithoutCenterPoint(ref points, center, radius, centerAngle, angleRange, segments);
			return points;
		}

		public static void GetArcPointsWithoutCenterPoint(ref Vector2[] points, in Vector2 center, float radius, float centerAngle, float angleRange, int segments = 20, int startIndex = 0) {
			if (radius <= 0) {
				throw new InvalidOperationException("radius in an arc should never be less than zero");
			}
			if (segments < 2) {
				throw new InvalidOperationException("segments in an arc should never be less than 2");
			}
			if (angleRange <= 0) {
				throw new InvalidOperationException("angleRange in an arc should never be less than zero");
			}
			if (points == null || points.Length < segments + startIndex) {
				points = new Vector2[segments + startIndex];
			}
			float angleInterval = angleRange / (segments - 1);
			float currentAngle = centerAngle - angleRange / 2;
			for (int i = 0; i < segments; i++, currentAngle += angleInterval) {
				float radian = currentAngle * Mathf.Deg2Rad;
				points[i + startIndex] = center + new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)) * radius;
			}
		}

		public static Vector2[] GetSwirlPoints(Vector2 center, float startRadius, float expandRadiusPerCircle, float angleAmount, int segmentsPerCircle = 20, float rotation = 0f) {
			if (startRadius < 0) {
				throw new InvalidOperationException("radius in a circle should never be less than zero");
			}
			if (segmentsPerCircle < 3) {
				throw new InvalidOperationException("segments in a circle should never be less than 3");
			}
			int totalSegements = Mathf.CeilToInt(Mathf.Abs(angleAmount) / 360f * segmentsPerCircle);

			Vector2[] points = new Vector2[totalSegements];
			GetSwirlPoints(ref points, center, startRadius, expandRadiusPerCircle, angleAmount, segmentsPerCircle, rotation, 0);
			return points;
		}

		public static void GetSwirlPoints(ref Vector2[] points, Vector2 center, float startRadius, float expandRadiusPerCircle, float angleAmount, int segmentsPerCircle = 20, float rotation = 0f, int startIndex = 0) {
			if (startRadius < 0) {
				throw new InvalidOperationException("radius in a circle should never be less than zero");
			}
			if (segmentsPerCircle < 3) {
				throw new InvalidOperationException("segments in a circle should never be less than 3");
			}
			int totalSegements = Mathf.CeilToInt(Mathf.Abs(angleAmount) / 360f * segmentsPerCircle);

			if (points == null || points.Length < totalSegements + startIndex) {
				points = new Vector2[totalSegements + startIndex];
			}

			float radianInterval = angleAmount * Mathf.Deg2Rad / totalSegements;
			float expandRadiusPerRadian = expandRadiusPerCircle / (Mathf.PI * 2);
			float expandRadiusInterval = expandRadiusPerRadian * Mathf.Abs(radianInterval);
			float currentRadian = 0f;
			for (int i = 0; i < totalSegements; i++, currentRadian += radianInterval) {
				float segementRadius = startRadius + (i * expandRadiusInterval);
				points[i + startIndex] = center + new Vector2(Mathf.Cos(currentRadian), Mathf.Sin(currentRadian)) * segementRadius;
			}

			// Rotation.
			if (rotation != 0f) {
				for (int i = 0; i < points.Length; i++) {
					points[i] = points[i].RotateAround(rotation, center);
				}
			}
		}


		public static Vector2[] GetCapsulePoints(in Vector2 center, in Vector2 size, CapsuleDirection2D direction, float rotation, int segments = 20) {
			Vector2[] points = new Vector2[segments];
			GetCapsulePoints(ref points, center, size, direction, rotation, segments);
			return points;
		}

		public static void GetCapsulePoints(ref Vector2[] points, in Vector2 center, in Vector2 size, CapsuleDirection2D direction, float rotation, int segments = 20, int startIndex = 0) {
			if (Mathf.Abs(size.x * size.y) <= 0) {
				throw new InvalidOperationException("size in a capsule should never be zero");
			}
			if (segments < 2) {
				throw new InvalidOperationException("segements in a capsule should never be less than 2");
			}
			if (points == null || points.Length < segments + startIndex) {
				points = new Vector2[segments + startIndex];
			}

			bool isCircle = false;
			switch (direction) {
				case CapsuleDirection2D.Vertical: {
					if (size.x >= size.y) {
						isCircle = true;
						break;
					}
					int topSemiCircleSegments = segments / 2;
					int bottomSemiCircleSegments = segments - topSemiCircleSegments;
					float radius = size.x / 2;
					float semiCircleCenterOffset = size.y / 2 - radius;
					GetArcPointsWithoutCenterPoint(ref points, center + new Vector2(0f, semiCircleCenterOffset), radius, 90f, 180f, topSemiCircleSegments, 0);
					GetArcPointsWithoutCenterPoint(ref points, center - new Vector2(0f, semiCircleCenterOffset), radius, -90f, 180f, bottomSemiCircleSegments, topSemiCircleSegments);
					break;
				}
				case CapsuleDirection2D.Horizontal: {
					if (size.y >= size.x) {
						isCircle = true;
						break;
					}
					int topSemiCircleSegments = segments / 2;
					int bottomSemiCircleSegments = segments - topSemiCircleSegments;
					float radius = size.y / 2;
					float semiCircleCenterOffset = size.x / 2 - radius;
					GetArcPointsWithoutCenterPoint(ref points, center + new Vector2(semiCircleCenterOffset, 0f), radius, 0f, 180f, topSemiCircleSegments, 0);
					GetArcPointsWithoutCenterPoint(ref points, center - new Vector2(semiCircleCenterOffset, 0f), radius, 180f, 180f, bottomSemiCircleSegments, topSemiCircleSegments);
					break;
				}
			}
			if (isCircle) {
				float radius = Mathf.Max(size.x, size.y) / 2;
				GetCirclePoints(ref points, center, radius, segments);
			}

			// Rotation.
			if (rotation != 0f) {
				for (int i = 0; i < points.Length; i++) {
					points[i] = points[i].RotateAround(rotation, center);
				}
			}
		}

		public static Vector2[] GetRegularStarPoints(in Vector2 center, float longRadius, float shortRadius, int vertexCount, float rotation) {
			Vector2[] points = new Vector2[vertexCount * 2];
			GetRegularStarPoints(ref points, center, longRadius, shortRadius, vertexCount, rotation);
			return points;
		}

		public static void GetRegularStarPoints(ref Vector2[] points, in Vector2 center, float longRadius, float shortRadius, int vertexCount, float rotation, int startIndex = 0) {
			if (longRadius <= 0 || shortRadius <= 0) {
				throw new InvalidOperationException("radius in a star should never be less than zero");
			}
			if (vertexCount <= 2) {
				throw new InvalidOperationException("vertexCount in a star should never be less than 3");
			}
			if (points == null || points.Length < vertexCount * 2 + startIndex) {
				points = new Vector2[vertexCount * 2 + startIndex];
			}
			float shortCircleAngleOffset = 360f / (float)vertexCount / 2f;
			Vector2[] longCirclePoints = GetCirclePoints(center, longRadius, vertexCount);
			Vector2[] shortCirclePoints = GetCirclePoints(center, shortRadius, vertexCount, shortCircleAngleOffset);

			for (int i = 0; i < vertexCount; i++) {
				points[i * 2 + startIndex] = longCirclePoints[i];
				points[i * 2 + 1 + startIndex] = shortCirclePoints[i];
			}

			// Rotation.
			if (rotation != 0f) {
				for (int i = 0; i < points.Length; i++) {
					points[i] = points[i].RotateAround(rotation, center);
				}
			}
		}

		/// <summary>
		/// Make a swirl star and return the points.
		/// </summary>
		/// <param name="center">The center of the swirl</param>
		/// <param name="longRadius">The longer radius</param>
		/// <param name="shortRadius">The shorter radius</param>
		/// <param name="vertexCount">How many vertex the swirl has</param>
		/// <param name="swirlStrength">How strong the swirl will perform, this value is in range 0 ~ 1</param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static Vector2[] GetSwirlStarPoints(in Vector2 center, float longRadius, float shortRadius, int vertexCount, float swirlStrength, float rotation) {
			Vector2[] points = new Vector2[vertexCount * 2];
			GetSwirlStarPoints(ref points, center, longRadius, shortRadius, vertexCount, swirlStrength, rotation);
			return points;
		}

		public static void GetSwirlStarPoints(ref Vector2[] points, in Vector2 center, float longRadius, float shortRadius, int vertexCount, float swirlStrength, float rotation, int startIndex = 0) {
			if (longRadius <= 0 || shortRadius <= 0) {
				throw new InvalidOperationException("radius in a swirl should never be less than zero");
			}
			if (vertexCount <= 2) {
				throw new InvalidOperationException("vertexCount in a swirl should never be less than 3");
			}
			if (points == null || points.Length < vertexCount * 2 + startIndex) {
				points = new Vector2[vertexCount * 2 + startIndex];
			}
			swirlStrength = Mathf.Clamp01(swirlStrength);
			float shortCircleAngleOffset = 360f / (float)vertexCount;
			float angleOffset = shortCircleAngleOffset * (1.0f + swirlStrength);
			Vector2[] longCirclePoints = GetCirclePoints(center, longRadius, vertexCount);
			Vector2[] shortCirclePoints = GetCirclePoints(center, shortRadius, vertexCount, angleOffset);

			for (int i = 0; i < vertexCount; i++) {
				points[i * 2 + startIndex] = longCirclePoints[i];
				points[i * 2 + 1 + startIndex] = shortCirclePoints[i];
			}

			// Rotation.
			if (rotation != 0f) {
				for (int i = 0; i < points.Length; i++) {
					points[i] = points[i].RotateAround(rotation, center);
				}
			}
		}
	}

	public static class GizmosUtil {
		public static void DrawCrossLines(in Vector3 position, float size, Color color) {
			Color oriColor = Gizmos.color;
			Gizmos.color = color;
			DrawCrossLines(position, size);
			Gizmos.color = oriColor;
		}

		public static void DrawCrossLines(in Vector3 position, float size) {
			Gizmos.DrawLine(new Vector3(position.x + size / 2, position.y + size / 2, position.z), new Vector3(position.x - size / 2, position.y - size / 2, position.z));
			Gizmos.DrawLine(new Vector3(position.x - size / 2, position.y + size / 2, position.z), new Vector3(position.x + size / 2, position.y - size / 2, position.z));
			Gizmos.DrawLine(new Vector3(position.x, position.y, position.z - size / 2), new Vector3(position.x, position.y, position.z + size / 2));
		}

		public static void DrawArrow2D(in Vector2 origin, in Vector2 direction, float distance = 0.1f, float width = 0.05f) {
			Gizmos.DrawRay(origin, direction * distance);
			Vector2 destination = origin + direction * distance;
			Gizmos.DrawRay(destination, (-direction).Rotate(30f) * width);
			Gizmos.DrawRay(destination, (-direction).Rotate(-30f) * width);
		}

		public static void DrawWireCircle2D(in Vector2 origin, float radius, int segments = 20) {
			Vector2[] points = Drawing2DUtil.GetCirclePoints(origin, radius, segments);
			DrawLines(points, true);
		}

		public static void DrawWireCircle2D(in Vector2 origin, float radius, float rotation, Vector2 scale, int segments = 20) {
			Vector2[] points = Drawing2DUtil.GetCirclePoints(origin, radius, segments);
			Vector2 center = origin;
			points = points.Select(p => Vector2.Scale((p - center), scale).Rotate(rotation) + center).ToArray();
			DrawLines(points, true);
		}

		public static void DrawWireArc2D(in Vector2 origin, float radius, float centerAngle, float angleRange, int segments = 20) {
			Vector2[] points = new Vector2[segments + 1];
			points[0] = origin;
			Drawing2DUtil.GetArcPointsWithoutCenterPoint(ref points, origin, radius, centerAngle, angleRange, segments, 1);
			DrawLines(points, true);
		}

		public static void DrawWireCapsule2D(in Vector2 origin, in Vector2 size, CapsuleDirection2D direction, float rotation, int segments = 20) {
			Vector2[] points = Drawing2DUtil.GetCapsulePoints(origin, size, direction, rotation, segments);
			DrawLines(points, true);
		}

		public static void Draw2DRectangle(in Vector2 origin, in Vector2 size, float rotation) {
			Vector2 extents = size * 0.5f;
			Vector2 lb = origin + new Vector2(-extents.x, -extents.y).Rotate(rotation);
			Vector2 lt = origin + new Vector2(-extents.x, extents.y).Rotate(rotation);
			Vector2 rb = origin + new Vector2(extents.x, -extents.y).Rotate(rotation);
			Vector2 rt = origin + new Vector2(extents.x, extents.y).Rotate(rotation);
			Gizmos.DrawLine(lb, lt);
			Gizmos.DrawLine(lt, rt);
			Gizmos.DrawLine(rt, rb);
			Gizmos.DrawLine(lb, rb);
		}

		public static void Draw2DRectangleSweep(in Vector2 origin, in Vector2 size, float rotation, in Vector2 direction, float distance) {
			Vector2 extents = size * 0.5f;
			Vector2 lb1 = origin + new Vector2(-extents.x, -extents.y).Rotate(rotation);
			Vector2 lt1 = origin + new Vector2(-extents.x, extents.y).Rotate(rotation);
			Vector2 rb1 = origin + new Vector2(extents.x, -extents.y).Rotate(rotation);
			Vector2 rt1 = origin + new Vector2(extents.x, extents.y).Rotate(rotation);
			Gizmos.DrawLine(lb1, lt1);
			Gizmos.DrawLine(lt1, rt1);
			Gizmos.DrawLine(rt1, rb1);
			Gizmos.DrawLine(lb1, rb1);

			Vector2 endOrigin = origin + direction * distance;
			Vector2 lb2 = endOrigin + new Vector2(-extents.x, -extents.y).Rotate(rotation);
			Vector2 lt2 = endOrigin + new Vector2(-extents.x, extents.y).Rotate(rotation);
			Vector2 rb2 = endOrigin + new Vector2(extents.x, -extents.y).Rotate(rotation);
			Vector2 rt2 = endOrigin + new Vector2(extents.x, extents.y).Rotate(rotation);
			Gizmos.DrawLine(lb2, lt2);
			Gizmos.DrawLine(lt2, rt2);
			Gizmos.DrawLine(rt2, rb2);
			Gizmos.DrawLine(lb2, rb2);

			Gizmos.DrawLine(lb1, lb2);
			Gizmos.DrawLine(lt1, lt2);
			Gizmos.DrawLine(rb1, rb2);
			Gizmos.DrawLine(rt1, rt2);
		}

		public static void DrawLines(Vector2[] points, bool looping = false) {
			for (int i = 0; i < points.Length; i++) {
				if (i == points.Length - 1) {
					if (looping) {
						Gizmos.DrawLine(points[i], points[0]);
					}
					break;
				}
				Gizmos.DrawLine(points[i], points[i + 1]);
			}
		}
		public static void DrawCollider(Collider2D collider) {
			DrawCollider(collider, collider.transform);
		}

		public static void DrawCollider(Collider2D collider, Transform transform) {
			DrawCollider(collider, collider.transform.position, collider.transform.rotation, collider.transform.lossyScale);
		}

		public static void DrawCollider(Collider2D collider, Vector3 position, Quaternion rotation, Vector3 scale) {
			var pushMatrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(position, rotation, scale);
			if (collider is CapsuleCollider2D capsule) {
				GizmosUtil.DrawWireCapsule2D(capsule.offset, capsule.size, capsule.direction, 0f);
			} else if (collider is BoxCollider2D box) {
				Gizmos.DrawWireCube(box.offset, new Vector3(box.size.x, box.size.y, 0.1f));
			} else if (collider is CircleCollider2D circle) {
				Gizmos.DrawWireSphere(circle.offset, circle.radius);
			}
			Gizmos.matrix = pushMatrix;
		}
	}

	public static class DebugDrawingUtil {
		public static void DrawCircle2D(in Vector2 origin, float radius, in Color color) {
			DrawCircle2D(origin, radius, color, Time.deltaTime);
		}
		public static void DrawCircle2D(in Vector2 origin, float radius, in Color color, float duration) {
			Vector2[] points = Drawing2DUtil.GetCirclePoints(origin, radius);
			DrawPolygon(points, color, duration);
		}
		public static void DrawCapsule2D(in Vector2 origin, in Vector2 size, CapsuleDirection2D direction, float rotation, Color color) {
			DrawCapsule2D(origin, size, direction, rotation, color, Time.deltaTime);
		}
		public static void DrawCapsule2D(in Vector2 origin, in Vector2 size, CapsuleDirection2D direction, float rotation, Color color, float duration) {
			Vector2[] points = Drawing2DUtil.GetCapsulePoints(origin, size, direction, rotation);
			DrawPolygon(points, color, duration);
		}

		public static void DrawPolygon(Vector2[] points, in Color color) {
			DrawPolygon(points, color, Time.deltaTime);
		}

		public static void DrawPolygon(Vector2[] points, in Color color, float duration) {
			for (int i = 0; i < points.Length - 1; i++) {
				Debug.DrawLine(points[i], points[i + 1], color, duration);
			}
			Debug.DrawLine(points[points.Length - 1], points[0], color, duration);
		}

		public static void DrawLines(Vector2[] points, in Color color) {
			DrawLines(points, color, Time.deltaTime);
		}
		public static void DrawLines(Vector2[] points, in Color color, float duration) {
			for (int i = 0; i < points.Length - 1; i++) {
				Debug.DrawLine(points[i], points[i + 1], color, duration);
			}
		}

		public static void DrawLinesLoop(Vector2[] points, in Color color) {
			DrawLinesLoop(points, color, Time.deltaTime);
		}
		public static void DrawLinesLoop(Vector2[] points, in Color color, float duration) {
			for (int i = 0; i < points.Length - 1; i++) {
				Debug.DrawLine(points[i], points[i + 1], color, duration);
			}
			if (points.Length < 3) { return; }
			Debug.DrawLine(points[points.Length - 1], points[0], color, duration);
		}

		public static void DrawRayFromInside(float insideAmount, in Vector3 start, in Vector3 direction, in Color color) {
			DrawRayFromInside(insideAmount, start, direction, color, Time.deltaTime);
		}

		public static void DrawRayFromInside(float insideAmount, in Vector3 start, in Vector3 direction, in Color color, float duration) {
			Vector3 dirNorm = direction.normalized;
			Debug.DrawRay(start - dirNorm * insideAmount, direction + dirNorm * insideAmount, color, duration);
		}

		public static void DrawCrossLines(in Vector3 position, float size, in Color color) {
			DrawCrossLines(position, size, color, Time.deltaTime);
		}
		public static void DrawCrossLines(in Vector3 position, float size, in Color color, float duration) {
			Debug.DrawLine(new Vector3(position.x + size / 2, position.y + size / 2, position.z), new Vector3(position.x - size / 2, position.y - size / 2, position.z), color, duration);
			Debug.DrawLine(new Vector3(position.x - size / 2, position.y + size / 2, position.z), new Vector3(position.x + size / 2, position.y - size / 2, position.z), color, duration);
			Debug.DrawLine(new Vector3(position.x, position.y, position.z - size / 2), new Vector3(position.x, position.y, position.z + size / 2), color, duration);
		}

		public static void Draw2DRectangle(in Vector2 origin, in Vector2 size, in Color color) {
			Draw2DRectangle(origin, size, color, Time.deltaTime);
		}

		public static void Draw2DRectangle(in Vector2 origin, in Vector2 size, in Color color, float duration) {
			Vector2 extents = size * 0.5f;
			Vector2 lb = new Vector2(origin.x - extents.x, origin.y - extents.y);
			Vector2 lt = new Vector2(origin.x - extents.x, origin.y + extents.y);
			Vector2 rb = new Vector2(origin.x + extents.x, origin.y - extents.y);
			Vector2 rt = new Vector2(origin.x + extents.x, origin.y + extents.y);
			Debug.DrawLine(lb, lt, color, duration);
			Debug.DrawLine(lt, rt, color, duration);
			Debug.DrawLine(rt, rb, color, duration);
			Debug.DrawLine(lb, rb, color, duration);
		}

		public static void Draw2DRectangle(in Vector2 origin, in Vector2 size, float angle, in Color color) {
			Draw2DRectangle(origin, size, angle, color, Time.deltaTime);
		}

		public static void Draw2DRectangle(in Vector2 origin, in Vector2 size, float angle, in Color color, float duration) {
			Vector2 extents = size * 0.5f;
			Vector2 lbExtents = new Vector2(-extents.x, -extents.y).Rotate(angle);
			Vector2 ltExtents = new Vector2(-extents.x, extents.y).Rotate(angle);
			Vector2 rbExtents = new Vector2(extents.x, -extents.y).Rotate(angle);
			Vector2 rtExtents = new Vector2(extents.x, extents.y).Rotate(angle);
			Vector2 lb = origin + lbExtents;
			Vector2 lt = origin + ltExtents;
			Vector2 rb = origin + rbExtents;
			Vector2 rt = origin + rtExtents;
			Debug.DrawLine(lb, lt, color, duration);
			Debug.DrawLine(lt, rt, color, duration);
			Debug.DrawLine(rt, rb, color, duration);
			Debug.DrawLine(lb, rb, color, duration);
		}
		public static void Draw2DRaysWithWidth(in Vector2 origin, in Vector2 direction, float width, in Color color) {
			Draw2DRaysWithWidth(origin, direction, width, color, Time.deltaTime);
		}

		public static void Draw2DRaysWithWidth(in Vector2 origin, in Vector2 direction, float width, in Color color, float duration) {
			Vector2 rectCenter = origin + direction / 2;
			Vector2 rectSize = new Vector2(direction.magnitude, width);
			float rectAngle = Vector2.SignedAngle(Vector2.right, direction);
			Draw2DRectangle(rectCenter, rectSize, rectAngle, color, duration);
		}

		public static void DrawArrow2D(in Vector2 origin, in Vector2 vector, in Color color) {
			DrawArrow2D(origin, vector, color, Time.deltaTime);
		}

		public static void DrawArrow2D(in Vector2 origin, in Vector2 vector, in Color color, float duration) {
			vector.Decompose(out Vector2 direction, out float distance);
			float arrowDistance = distance * 0.1f;
			DrawArrow2D(origin, direction, distance, arrowDistance, color, duration);
		}

		public static void DrawArrow2D(in Vector2 origin, in Vector2 direction, float distance, float arrowWidth, in Color color) {
			DrawArrow2D(origin, direction, distance, arrowWidth, color, Time.deltaTime);
		}

		public static void DrawArrow2D(in Vector2 origin, in Vector2 direction, float distance, float arrowWidth, in Color color, float duration) {
			Debug.DrawRay(origin, direction * distance, color, duration);
			Vector2 destination = origin + direction * distance;
			Debug.DrawRay(destination, (-direction).Rotate(30f) * arrowWidth, color, duration);
			Debug.DrawRay(destination, (-direction).Rotate(-30f) * arrowWidth, color, duration);
		}
	}

	public static class GUIUtil {
		private static Texture2D _whiteTexture;
		public static Texture2D WhiteTexture {
			get {
				if (_whiteTexture == null) {
					_whiteTexture = new Texture2D(1, 1);
					_whiteTexture.SetPixel(0, 0, Color.white);
					_whiteTexture.Apply();
				}

				return _whiteTexture;
			}
		}

		public static void DrawRectangle(Rect rect, Color color) {
			GUI.color = color;
			GUI.DrawTexture(rect, WhiteTexture);
			GUI.color = Color.white;
		}

		public static void DrawWireRectangle(Rect rect, float border, Color color) {
			// Top
			DrawRectangle(new Rect(rect.xMin, rect.yMin, rect.width, border), color);
			// Left
			DrawRectangle(new Rect(rect.xMin, rect.yMin, border, rect.height), color);
			// Right
			DrawRectangle(new Rect(rect.xMax - border, rect.yMin, border, rect.height), color);
			// Bottom
			DrawRectangle(new Rect(rect.xMin, rect.yMax - border, rect.width, border), color);
		}

		public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2) {
			// Move origin from bottom left to top left
			screenPosition1.y = Screen.height - screenPosition1.y;
			screenPosition2.y = Screen.height - screenPosition2.y;
			// Calculate corners
			var topLeft = Vector3.Min( screenPosition1, screenPosition2 );
			var bottomRight = Vector3.Max( screenPosition1, screenPosition2 );
			// Create Rect
			return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
		}

		public static Bounds GetScreenBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2) {
			Vector3 v1 = screenPosition1;
			Vector3 v2 = screenPosition2;
			Vector3 min = Vector3.Min( v1, v2 );
			Vector3 max = Vector3.Max( v1, v2 );
			min.z = camera.nearClipPlane;
			max.z = camera.farClipPlane;

			Bounds bounds = new Bounds();
			bounds.SetMinMax(min, max);
			return bounds;
		}
	}
}