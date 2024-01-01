using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System.Runtime.CompilerServices;
using System.Collections;

namespace PeDev {

	public static class Vector2Extensions {
		// From 2D GameKit.Helper
		/// <summary>
		/// Rotate a vector2 by counter-clockwise with specific angle in degrees.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="degrees"></param>
		/// <returns></returns>
		public static Vector2 Rotate(this Vector2 v, float degrees) {
			if (degrees == 0f) { return v; }

			float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
			float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

			float tx = v.x;
			float ty = v.y;
			v.x = (cos * tx) - (sin * ty);
			v.y = (sin * tx) + (cos * ty);
			return v;
		}

		public static void RotateSelf(this ref Vector2 v, float degrees) {
			if (degrees == 0f) { return; }

			float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
			float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

			float tx = v.x;
			float ty = v.y;
			v.x = (cos * tx) - (sin * ty);
			v.y = (sin * tx) + (cos * ty);
		}

		/// <summary>
		/// Rotate a vector2 by counter-clockwise with specific angle in radian.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="degrees"></param>
		/// <returns></returns>
		public static Vector2 RotateByRadian(this Vector2 v, float radian) {
			if (radian == 0f) { return v; }

			float sin = Mathf.Sin(radian);
			float cos = Mathf.Cos(radian);

			float tx = v.x;
			float ty = v.y;
			v.x = (cos * tx) - (sin * ty);
			v.y = (sin * tx) + (cos * ty);
			return v;
		}

		/// <summary>
		/// Rotate the vector with negative direction.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="degrees"></param>
		/// <returns></returns>
		public static Vector2 Unrotate(this Vector2 v, float degrees) {
			if (degrees == 0f) { return v; }
			return Rotate(v, -degrees);
		}

		/// <summary>
		/// Rotate a Vector2 by counter-clockwise around the specific target center.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="degrees"></param>
		/// <param name="center"></param>
		/// <returns></returns>
		public static Vector2 RotateAround(this Vector2 v, float degrees, in Vector2 center) {
			v -= center;
			v = v.Rotate(degrees);
			v += center;
			return v;
		}

		public static float DistanceSqr(this in Vector2 p1, in Vector2 p2) {
			float dx = p1.x - p2.x;
			float dy = p1.y - p2.y;
			return dx * dx + dy * dy;
		}

		public static bool DistanceLessThanRadius(Vector2 p1, Vector2 p2, float radius) {
			return DistanceSqr(p1, p2) <= (radius * radius);
		}

		// From: https://gamedev.stackexchange.com/questions/45412/understanding-math-used-to-determine-if-vector-is-clockwise-counterclockwise-f
		/// <summary>
		/// return true if rhs is clockwise of this vector,
		/// minus if anticlockwise (Y axis pointing up, X axis to right)
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns></returns>
		public static bool IsClockwise(this Vector2 lhs, Vector2 rhs) {
			if (lhs.y * rhs.x >= lhs.x * rhs.y) {
				return true;
			}
			return false;
		}

		// From: Cinemachine.Utility
		/// <summary>
		/// Get the closest point on a line segment.
		/// </summary>
		/// <param name="p">A point in space</param>
		/// <param name="s0">Start of line segment</param>
		/// <param name="s1">End of line segment</param>
		/// <param name="eps">Epsilon means how small the number will be considered 0 (Default value is 0.0001f)</param>
		/// <returns>The interpolation parameter representing the point on the segment, with 0==s0, and 1==s1</returns>
		public static float ClosestPointOnSegment(this Vector2 p, Vector2 s0, Vector2 s1, float eps = 0.0001f) {
			Vector2 s = s1 - s0;
			float len2 = Vector2.SqrMagnitude(s);
			if (len2 < eps)
				return 0; // degenrate segment
			return Mathf.Clamp01(Vector2.Dot(p - s0, s) / len2);
		}

		/// <summary>
		/// Rotate a vector3 by counter-clockwise with specific angle in degrees.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="degrees"></param>
		/// <returns></returns>
		public static Vector3 Rotate(this Vector3 v, float degrees) {
			if (degrees == 0f) { return v; }

			float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
			float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

			float tx = v.x;
			float ty = v.y;
			v.x = (cos * tx) - (sin * ty);
			v.y = (sin * tx) + (cos * ty);
			return v;
		}


		// From: Cinemachine.Utility
		/// <summary>
		/// Get the closest point on a line segment.
		/// </summary>
		/// <param name="p">A point in space</param>
		/// <param name="s0">Start of line segment</param>
		/// <param name="s1">End of line segment</param>
		/// <returns>The interpolation parameter representing the point on the segment, with 0==s0, and 1==s1</returns>
		public static float ClosestPointOnSegment(this Vector3 p, Vector3 s0, Vector3 s1, float eps = 0.0001f) {
			Vector3 s = s1 - s0;
			float len2 = Vector3.SqrMagnitude(s);
			if (len2 < eps)
				return 0; // degenrate segment
			return Mathf.Clamp01(Vector3.Dot(p - s0, s) / len2);
		}

		public static float ClosestDistanceOnSegment(this Vector2 p, in Vector2 s0, in Vector2 s1, float eps = 0.0001f) {
			Vector2 s = s1 - s0;
			float len2 = Vector2.SqrMagnitude(s);
			if (len2 < eps)
				return Vector2.Distance(s0, p); // degenrate segment = point

			float linePoint = Mathf.Clamp01(Vector2.Dot(p - s0, s) / len2);
			Vector2 point = Vector2.Lerp(s0, s1, linePoint);

			return Vector2.Distance(point, p);
		}

		public static float ClosestDistanceSqrOnSegment(this Vector2 p, in Vector2 s0, in Vector2 s1, float eps = 0.0001f) {
			Vector2 s = s1 - s0;
			float len2 = Vector2.SqrMagnitude(s);
			if (len2 < eps)
				return DistanceSqr(s0, p); // degenrate segment = point

			float linePoint = Mathf.Clamp01(Vector2.Dot(p - s0, s) / len2);
			Vector2 point = Vector2.Lerp(s0, s1, linePoint);

			return DistanceSqr(point, p);
		}

		// From: Cinemachine.Utility
		/// <summary>Is the vector within Epsilon of zero length?</summary>
		/// <param name="v"></param>
		/// <param name="eps">Epsilon means how small the number will be considered 0 (Default value is 0.0001f)</param>
		/// <returns>True if the square magnitude of the vector is within Epsilon of zero</returns>
		public static bool AlmostZero(this Vector2 v, float eps = 0.0001f) {
			return v.sqrMagnitude < (eps * eps);
		}

		/// <summary>Is the vector within Epsilon of zero length?</summary>
		/// <param name="v"></param>
		/// <param name="axis"> 0 = x, 1 = y</param>
		/// <param name="eps">Epsilon means how small the number will be considered 0 (Default value is 0.0001f)</param>
		/// <returns>True if the square magnitude of the vector is within Epsilon of zero</returns>
		public static bool AlmostZero(this Vector2 v, int axis, float eps = 0.0001f) {
			return v[axis] < eps;
		}

		/// <summary>Is the vector within Epsilon of zero length?</summary>
		/// <param name="v"></param>
		/// <returns>True if the square magnitude of the vector is within Epsilon of zero</returns>
		public static bool AlmostZero(this Vector3 v, float eps = 0.0001f) {
			return v.sqrMagnitude < (eps * eps);
		}
		/// <summary>
		/// Check two vectors that represent velocity will collide together.
		/// Function is just meant if is dot(v1, v2) < 0f or not.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="vector"></param>
		/// <param name="zeroReturnTrue">Whether or not return True when the vectors are orthogonal</param>
		/// <param name="eps">Epsilon. Range[-eps, eps] means ZERO</param>
		/// <returns></returns>
		public static bool CollidedWith(this Vector2 v, Vector2 vector, bool zeroReturnTrue = false, float eps = 0.0001f) {
			if (v.AlmostZero() || vector.AlmostZero()) {
				return zeroReturnTrue;
			}
			if (zeroReturnTrue) {
				return Vector2.Dot(v, vector) <= eps;
			}
			return Vector2.Dot(v, vector) <= -eps;
		}
		/// <summary>
		/// Convert to vector3 with specific z axis.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="z">Z Axis</param>
		/// <returns></returns>
		public static Vector3 ToVector3(this Vector2 v, float z = 0f) {
			return new Vector3(v.x, v.y, z);
		}

		public static string ToStringPrecise(this Vector2 v) {
			return string.Format("{0}, {1}", v.x, v.y);
		}

		/// <summary>
		/// Get the multiple of two vectors component-wise.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="scalar">Scale</param>
		/// <returns>The multiple of two vector2 component-wise.</returns>
		public static Vector2 GetScale(this Vector2 v, in Vector2 scalar) {
			return new Vector2(v.x * scalar.x, v.y * scalar.y);
		}

		/// <summary>
		/// Get the vector by projecting on the specific vector which is normalized. 
		/// If the normal is not normalized, you should call ProjectSafely() instead.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="onNormal">This value have to be normalized!</param>
		/// <returns></returns>
		public static Vector2 Project(this Vector2 v, in Vector2 onNormal) {
			return onNormal * Vector2.Dot(v, onNormal);
		}

		/// <summary>
		/// Get the vector by projecting on the specific vector which can be unnormalized.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="onNormal"></param>
		/// <returns></returns>
		public static Vector2 ProjectSafely(this Vector2 v, in Vector2 onNormal) {
			Vector2 normalizedNormal = onNormal.normalized;
			return normalizedNormal * Vector2.Dot(v, normalizedNormal);
		}

		/// <summary>
		/// Get the vector by projecting on the specific plane(a vector in 2D) which is represented by a plane normal which is normalized. 
		/// If the normal is not normalized, you should call ProjectOnPlaneSafely() instead.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="planeNormal">This value have to be normalized!</param>
		/// <returns></returns>
		public static Vector2 ProjectOnPlane(this Vector2 v, in Vector2 planeNormal) {
			return v - planeNormal * Vector2.Dot(v, planeNormal);
		}

		/// <summary>
		/// Get the vector by projecting on the specific plane(a vector in 2D) which is represented by a plane normal which can be unnormalized. 
		/// </summary>
		/// <param name="v"></param>
		/// <param name="planeNormal">This value have to be normalized!</param>
		/// <returns></returns>
		public static Vector2 ProjectOnPlaneSafely(this Vector2 v, in Vector2 planeNormal) {
			Vector2 normalizedNormal = planeNormal.normalized;
			return v - normalizedNormal * Vector2.Dot(v, normalizedNormal);
		}

		public static bool IsNaN(this Vector2 v) {
			return float.IsNaN(v.x) || float.IsNaN(v.y);
		}

		public static bool IsInifinity(this Vector2 v) {
			return float.IsInfinity(v.x) || float.IsInfinity(v.y);
		}

		/// <summary>
		/// 叉積的結果為兩向量組成的平行四邊形之面積量(有正負號)，該值絕對值除以底的向量長度，得到高(Height)。
		/// </summary>
		public static float Cross(this Vector2 a, in Vector2 b) {
			return a.x * b.y - a.y * b.x;
		}

		public static Vector2 Cross(float s, in Vector2 a) {
			return new Vector2(-s * a.y, s * a.x);
		}

		public static Vector2 ComplexMultiply(this Vector2 vec, in Vector2 complex) {
			return new Vector2(
				vec.x * complex.x - vec.y * complex.y,
				vec.y * complex.x + vec.x * complex.y
				);
		}

		public static Vector2 RotatePoint(this Vector2 point, Vector2 center, float angle) {
			float s = Mathf.Sin(angle);
			float c = Mathf.Cos(angle);
			// 移回原點
			point -= center;
			Vector2 newPoint = new Vector2(point.x * c - point.y * s, point.x * s + point.y * c);
			// 再移回座標
			point = newPoint + center;
			return point;
		}

		/// <summary>
		/// 把deceleratee減decelerater(以decelerater的方向減純量)，最少至該方向純量為零(該方向無速度)
		/// 若雙方純量方向相反則直接返回deceleratee
		/// </summary>
		/// <returns></returns>
		public static Vector2 Decelerate(this Vector2 deceleratee, in Vector2 decelerater) {
			Vector2 direction = decelerater.normalized;
			float decelerateeValue = Vector2.Dot(deceleratee, direction);
			float deceleraterValue = decelerater.magnitude;

			float newMagnitude = decelerateeValue.Decelerate(deceleraterValue);

			return deceleratee + (newMagnitude - decelerateeValue) * direction;
		}

		/// <summary>
		/// 把acceleratee加上accelerater(以accelerater的方向加純量)
		/// 若雙方純量方向相反則直接返回acceleratee
		/// </summary>
		/// <returns></returns>
		public static Vector2 Accelerate(this Vector2 acceleratee, in Vector2 accelerater) {
			Vector2 direction = accelerater.normalized;
			float accelerateeValue = Vector2.Dot(acceleratee, direction);
			float acceleraterValue = accelerater.magnitude;

			float newMagnitude = accelerateeValue.Accelerate(acceleraterValue);

			return acceleratee + (newMagnitude - accelerateeValue) * direction;
		}

		/// <summary>
		/// 設定此向量在指定Axis上的純量value(有正負號)
		/// </summary>
		public static void SetValueAlongAxis(this ref Vector2 vec, in Vector2 axis, float value) {
			vec += (value - Vector2.Dot(vec, axis)) * axis;
		}

		/// <summary>
		/// 確定兩個線段有沒有相交
		/// </summary>
		/// <param name="p1">線段1起點</param>
		/// <param name="p2">線段1終點</param>
		/// <param name="p3">線段2起點</param>
		/// <param name="p4">線段2終點</param>
		/// <param name="lines_intersect">兩條線有沒有相交</param>
		/// <param name="segments_intersect">兩個線段有沒有相交</param>
		/// <param name="intersection">交點</param>
		/// <param name="close_p1"></param>
		/// <param name="close_p2"></param>
		public static void FindIntersection(
			Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4,
			out bool lines_intersect, out bool segments_intersect,
			out Vector2 intersection,
			out Vector2 close_p1, out Vector2 close_p2) {
			// Get the segments' parameters.
			float dx12 = p2.x - p1.x;
			float dy12 = p2.y - p1.y;
			float dx34 = p4.x - p3.x;
			float dy34 = p4.y - p3.y;

			// Solve for t1 and t2
			float denominator = (dy12 * dx34 - dx12 * dy34);

			float t1 = ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34) / denominator;
			if (float.IsInfinity(t1)) {
				// The lines are parallel (or close enough to it).
				lines_intersect = false;
				segments_intersect = false;
				intersection = new Vector2(float.NaN, float.NaN);
				close_p1 = new Vector2(float.NaN, float.NaN);
				close_p2 = new Vector2(float.NaN, float.NaN);
				return;
			}
			lines_intersect = true;

			float t2 = ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12) / -denominator;

			// Find the point of intersection.
			intersection = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);

			// The segments intersect if t1 and t2 are between 0 and 1.
			segments_intersect =
				((t1 >= 0) && (t1 <= 1) &&
				 (t2 >= 0) && (t2 <= 1));

			// Find the closest points on the segments.
			if (t1 < 0) {
				t1 = 0;
			} else if (t1 > 1) {
				t1 = 1;
			}

			if (t2 < 0) {
				t2 = 0;
			} else if (t2 > 1) {
				t2 = 1;
			}

			close_p1 = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);
			close_p2 = new Vector2(p3.x + dx34 * t2, p3.y + dy34 * t2);
		}

		/// <summary>
		/// 拿取離線段最近的點
		/// </summary>
		/// <param name="point"></param>
		/// <param name="lineStart"></param>
		/// <param name="lineEnd"></param>
		/// <returns></returns>
		public static Vector2 GetClosestPointOnFiniteLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd) {
			Vector2 lineDirection = lineEnd - lineStart;
			float lineLength = lineDirection.magnitude;
			lineDirection.Normalize();
			float projectLength = Mathf.Clamp(Vector2.Dot(point - lineStart, lineDirection), 0f, lineLength);
			return lineStart + lineDirection * projectLength;
		}
		
		public static float GetDistanceFromPointToSegment(Vector2 point, Vector2 s1, Vector2 s2, out Vector2 closestPoint) {
			Vector2 direction = s2 - s1;
			float length = direction.magnitude;
			direction.Normalize();

			float projectLength = Vector2.Dot(point - s1, direction);

			if (projectLength < 0) {
				closestPoint = s1;
			} else if (projectLength > length) {
				closestPoint = s2;
			} else {
				closestPoint = s1 + direction * projectLength;
			}
			return Vector2.Distance(closestPoint, point);
		}

		/// <summary>
		/// 把一個向量分解成direction和magnitude.
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="direction"></param>
		/// <param name="magnitude"></param>
		public static void Decompose(this Vector2 vector, out Vector2 direction, out float magnitude) {
			magnitude = vector.magnitude;
			if (magnitude == 0f) {
				direction = Vector2.zero;
			} else {
				direction = vector / magnitude;
			}
		}

		/// <summary>
		/// Transforms position from local space to world space.
		/// </summary>
		public static Vector2 TransformPoint2D(Vector2 localPoint, Vector2 origin, float rotation, Vector2 scale) {
			localPoint.Scale(scale);
			localPoint.RotateSelf(rotation);
			return localPoint + origin;
		}

		/// <summary>
		/// Transforms vector from local space to world space.
		/// </summary>
		public static Vector2 TransformVector2D(Vector2 localVector, float rotation, Vector2 scale) {
			localVector.Scale(scale);
			localVector.RotateSelf(rotation);
			return localVector;
		}

		/// <summary>
		/// Transforms direction from local space to world space.
		/// </summary>
		public static Vector2 TransformDirection2D(Vector2 localVector, float rotation, Vector2 scale) {
			Vector2 worldVector = TransformVector2D(localVector, rotation, scale);
			worldVector.Normalize();
			return worldVector;
		}

		/// <summary>
		/// Transforms position from world space to local space.
		/// </summary>
		public static Vector2 InverseTransformPoint2D(Vector2 worldPoint, Vector2 origin, float rotation, Vector2 scale) {
			Vector2 localPoint = (worldPoint - origin);
			localPoint.RotateSelf(-rotation);
			localPoint.x /= scale.x;
			localPoint.y /= scale.y;
			return localPoint;
		}

		/// <summary>
		/// Transforms vector from world space to local space.
		/// </summary>
		public static Vector2 InverseTransformVector2D(Vector2 worldVector, float rotation, Vector2 scale) {
			worldVector.RotateSelf(-rotation);
			worldVector.x /= scale.x;
			worldVector.y /= scale.y;
			return worldVector;
		}

		/// <summary>
		/// Transforms direction from world space to local space.
		/// </summary>
		public static Vector2 InverseTransformDirection2D(Vector2 worldDirection, float rotation, Vector2 scale) {
			Vector2 localVec = InverseTransformVector2D(worldDirection, rotation, scale);
			localVec.Normalize();
			return localVec;
		}

		/// <summary>
		/// 回傳向量的各個純量之絕對值最大值。例如(-10, 5).AbsMaxComponent()會回傳10。
		/// 通常用於計算Circle Collider的radius in world space
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static float AbsMaxComponent(this Vector2 vector) {
			return Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
		}

		/// <summary>
		/// Scale the vector with specific value and return the absolute vector.
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="scale"></param>
		/// <returns></returns>
		public static Vector2 AbsScale(this Vector2 vector, Vector2 scale) {
			return new Vector2(Mathf.Abs(vector.x * scale.x), Mathf.Abs(vector.y * scale.y));
		}

		/// <summary>
		/// 回傳向量的各個純量之絕對值相乘後的積。例如(-10, 5).AbsComponentMultiply()會回傳50。
		/// 通常用於計算scale後的體積
		/// </summary>
		public static float AbsComponentMultiply(this Vector2 vector) {
			return Mathf.Abs(vector.x) * Mathf.Abs(vector.y);
		}
	}

	public static class Vector3Extensions {
		/// <summary>
		/// Return a vector3 with specific z axis.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static Vector3 ReplaceZAxis(this Vector3 v, float z) {
			return new Vector3(v.x, v.y, z);
		}

		public static Vector3 ReplaceYAxis(this Vector3 v, float y) {
			return new Vector3(v.x, y, v.z);
		}

		public static Vector3 ReplaceXAxis(this Vector3 v, float x) {
			return new Vector3(x, v.y, v.z);
		}


		/// <summary>
		/// Return a vector3 with specific x, y axis.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="vector2"></param>
		/// <returns></returns>
		public static Vector3 ReplaceXYAxis(this Vector3 v, Vector2 vector2) {
			return new Vector3(vector2.x, vector2.y, v.z);
		}
	}

	// From 2D GameKit.Helper
	public static class TransformExtensions {
		public static Bounds TransformBounds(this Transform transform, Bounds localBounds) {
			var center = transform.TransformPoint(localBounds.center);

			// transform the local extents' axes
			var extents = localBounds.extents;
			var axisX = transform.TransformVector(extents.x, 0, 0);
			var axisY = transform.TransformVector(0, extents.y, 0);
			var axisZ = transform.TransformVector(0, 0, extents.z);

			// sum their absolute value to get the world extents
			extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
			extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
			extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

			return new Bounds { center = center, extents = extents };
		}

		public static Bounds InverseTransformBounds(this Transform transform, Bounds worldBounds) {
			var center = transform.InverseTransformPoint(worldBounds.center);

			// transform the local extents' axes
			var extents = worldBounds.extents;
			var axisX = transform.InverseTransformVector(extents.x, 0, 0);
			var axisY = transform.InverseTransformVector(0, extents.y, 0);
			var axisZ = transform.InverseTransformVector(0, 0, extents.z);

			// sum their absolute value to get the world extents
			extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
			extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
			extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

			return new Bounds { center = center, extents = extents };
		}

		public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position) {
			var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
			return localToWorldMatrix.MultiplyPoint3x4(position);
		}

		public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position) {
			var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
			return worldToLocalMatrix.MultiplyPoint3x4(position);
		}

		public static Vector3 TransformVectorUnscaled(this Transform transform, Vector3 vector) {
			var localToWorldMatrix = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one);
			return localToWorldMatrix.MultiplyPoint3x4(vector);
		}

		public static Vector3 InverseTransformVectorUnscaled(this Transform transform, Vector3 vector) {
			var worldToLocalMatrix = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one).inverse;
			return worldToLocalMatrix.MultiplyPoint3x4(vector);
		}

		/// <summary>
		/// Get the eulerAngles.z of transform for 2D game.
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static float Get2DRotationAngle(this Transform transform) {
			return transform.eulerAngles.z;
		}

		/// <summary>
		/// Get the localEulerAngles.z of transform for 2D game.
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static float Get2DRotationAngleLocal(this Transform transform) {
			return transform.localEulerAngles.z;
		}

		/// <summary>
		/// Set the value to eulerAngles.z
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="angle"></param>
		public static void Set2DRotationAngle(this Transform transform, float angle) {
			Vector3 euler = transform.eulerAngles;
			euler.z = angle;
			transform.eulerAngles = euler;
		}

		/// <summary>
		/// Set the value to localEulerAngles.z
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="angle"></param>
		public static void Set2DRotationAngleLocal(this Transform transform, float angle) {
			Vector3 euler = transform.localEulerAngles;
			euler.z = angle;
			transform.localEulerAngles = euler;
		}

		/// <summary>
		/// Flip the 2d object by axis-y via scale.x
		/// </summary>
		/// <param name="transform"></param>
		public static void Flip2DByScaleX(this Transform transform) {
			// set scale.
			transform.SetLossyScale(new Vector3(-transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
			// rotation角度要反過來.
			transform.Set2DRotationAngleLocal(-transform.Get2DRotationAngle());
		}

		public static Vector3 GetLocalScale(this Transform transform, Transform refParent) {
			Vector3 lossy1 = transform.lossyScale;
			Vector3 lossy2 = refParent.lossyScale;
			return new Vector3((Mathf.Abs(lossy2.x) > 0f ? lossy1.x / lossy2.x : 0f), (Mathf.Abs(lossy2.y) > 0f ? lossy1.y / lossy2.y : 0f), (Mathf.Abs(lossy2.z) > 0f ? lossy1.z / lossy2.z : 0f));
		}

		/// <summary>
		/// 透過父物件的lossyScale來算該物件的localScale
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="scale"></param>
		public static void SetLossyScale(this Transform transform, in Vector3 scale) {
			Vector3 newLocalScale = transform.localScale;
			Transform parent = transform.parent;
			if (parent != null) {
				transform.localScale = Vector3.one;
				Vector3 originalLossyScale = transform.lossyScale;
				for (int i = 0; i < 3; i++) {
					if (Mathf.Abs(originalLossyScale[i]) > 0f) {
						newLocalScale[i] = scale[i] / originalLossyScale[i];
					} else {
						newLocalScale[i] = 0f;
					}
				}
			}
			transform.localScale = newLocalScale;
		}

		public static void SetLossyScale2D(this Transform transform, in Vector2 scale) {
			Vector3 newLocalScale = transform.localScale;
			Transform parent = transform.parent;
			if (parent != null) {
				transform.localScale = Vector3.one;
				Vector3 originalLossyScale = transform.lossyScale;
				for (int i = 0; i < 2; i++) {
					if (Mathf.Abs(originalLossyScale[i]) > 0f) {
						newLocalScale[i] = scale[i] / originalLossyScale[i];
					} else {
						newLocalScale[i] = 0f;
					}
				}
			}
			transform.localScale = newLocalScale;
		}

		/// <summary>
		/// 設定2D物件的scale並保留其z scale
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="scale"></param>
		public static void Set2DLocalScale(this Transform transform, Vector2 scale) {
			transform.localScale = new Vector3(scale.x, scale.y, transform.localScale.z);
		}

		/// <summary>
		/// Set local scale by the specific anchor point.
		/// </summary>
		public static void SetLocalScaleByAnchor(this Transform transform, in Vector2 newScale, in Vector2 anchor) {
			Vector2 newPosition = MathfExtensions.ComputeNewPositionForSetScaleByAnchor(transform.position, transform.localScale, newScale, anchor);
			transform.localScale = newScale;
			transform.position = newPosition.ToVector3(transform.position.z);
		}

		/// <summary>
		/// Find the transform with the specific name from the transform's children.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Transform RecursiveFind(this Transform parent, string name) {
			if (parent.name == name) {
				return parent;
			}

			foreach (Transform child in parent) {
				var result = child.RecursiveFind(name);
				if (result != null) {
					return result;
				}
			}

			return null;
		}

		/// <summary>
		/// Get the full path of the transform with the root.
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="root"></param>
		/// <returns></returns>
		public static string GetFullPath(this Transform transform, Transform root) {
			string path = "";

			Transform current = transform;

			if (root) {
				while (current && current != root) {
					path = current.name + path;

					current = current.parent;

					if (current != root) {
						path = "/" + path;
					}
				}

				if (!current) {
					path = "";
				}
			}

			return path;
		}

		/// <summary>
		/// Copy the parent, localPosition, localRotation, and localScale from the specific transform.
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="from"></param>
		public static void DeepCopyFrom(this Transform transform, Transform from) {
			transform.parent = from.parent;
			transform.localPosition = from.localPosition;
			transform.localRotation = from.localRotation;
			transform.localScale = from.localScale;
		}

		/// <summary>
		/// Copy the parent, localPosition, localRotation, and localScale to the specific transform.
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="from"></param>
		public static void DeepCopyTo(this Transform transform, Transform to) {
			to.parent = transform.parent;
			to.localPosition = transform.localPosition;
			to.localRotation = transform.localRotation;
			to.localScale = transform.localScale;
		}

		/// <summary>
		/// Set position, rotation, and localScale.
		/// </summary>
		public static void SetTRS(this Transform transform, Vector3 position, Quaternion rotation, Vector3 localScale) {
			// Scale優先設定，以防scale原先為0時會導致position設定失敗。
			transform.localScale = localScale;
			transform.rotation = rotation;
			transform.position = position;
		}

		/// <summary>
		/// Add the position, rotation(euler angle), and localScale to this transform.
		/// The order is localScale -> Rotation -> Position.
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="position"></param>
		/// <param name="localScale"></param>
		public static void AddTRS(this Transform transform, Vector3 position, Vector3 eulerAngle, Vector3 localScale, Space relativeTo) {
			// Scale優先設定，以防scale原先為0時會導致position設定失敗。
			transform.localScale += localScale;
			transform.Rotate(eulerAngle, relativeTo);
			transform.Translate(position, relativeTo);
		}

		/// <summary>
		/// 將local position/rotation歸零且local Scale設回1。
		/// </summary>
		/// <param name="transform"></param>
		public static void ResetLocals(this Transform transform) {
			transform.localScale = Vector3.one;
			transform.localRotation = Quaternion.identity;
			transform.localPosition = Vector3.zero;
		}

		public static void SetParentAndResetLocals(this Transform transform, Transform parent) {
			transform.SetParent(parent, false);
			transform.ResetLocals();
		}
	}

	public static class Matrix4x4Extensions {
		public static Vector2 TransformPoint2D(this in Matrix4x4 matrix, in Vector2 point) {
			return new Vector2(
				matrix.m00 * point.x + matrix.m01 * point.y + matrix.m03,
				matrix.m10 * point.x + matrix.m11 * point.y + matrix.m13
				);
		}

		public static Vector2 TransformVector2D(this in Matrix4x4 matrix, in Vector2 vector) {
			return new Vector2(
				matrix.m00 * vector.x + matrix.m01 * vector.y,
				matrix.m10 * vector.x + matrix.m11 * vector.y
				);
		}

		public static Vector2 TransformDirection2D(this in Matrix4x4 matrix, in Vector2 vector) {
			Vector2 worldVector = TransformVector2D(matrix, vector);
			worldVector.Normalize();
			return worldVector;
		}
	}

	public static class PlatformerEffector2DExtensions {
		/// <summary>
		/// Check whether or not this collider have the contact with its specific surface.
		/// </summary>
		/// <param name="effector"></param>
		/// <param name="normal"> the surface normal of contact point</param>
		/// <returns></returns>
		public static bool ValidCollision(this PlatformEffector2D effector, Vector2 normal) {
			float dot = Vector2.Dot(effector.transform.up, normal);
			float cos = Mathf.Cos(effector.surfaceArc * 0.5f * Mathf.Deg2Rad);

			//we round both the dot & cos to 1/1000 precision to avoid undefined behaviour on edge case (e.g. side of a paltform with 180 side arc)
			dot = Mathf.Round(dot * 1000.0f) / 1000.0f;
			cos = Mathf.Round(cos * 1000.0f) / 1000.0f;

			if (dot > cos) {
				return true;
			}

			return false;
		}
	}


	// Copyright(c) 2018 Pete Michaud, github.com/PeteMichaud
	// https://gist.github.com/PeteMichaud/3ad508d323a971638f54f127bed84609
	public static class AnimationCurveExtensions {
		// Notice that DON'T use ASIIGN OPERATOR(=) here, or you will get same reference!
		// So I modified to a getter to avoid referece issue.
		public static AnimationCurve LINEAR { get { return AnimationCurve.Linear(0, 0, 1, 1); } }
		public static AnimationCurve FLAT_ONE { get { return AnimationCurve.Linear(0, 1, 1, 1); } }
		public static AnimationCurve FLAT_ZERO { get { return AnimationCurve.Linear(0, 0, 1, 0); } }

		/// <summary>
		/// Find first derivative of curve at point x
		/// </summary>
		/// <param name="curve"></param>
		/// <param name="x"></param>
		/// <returns>Slope of curve at point x as float</returns>
		public static float Differentiate(this AnimationCurve curve, float x) {
			IEnumerable<Keyframe> keyCaches = curve.keys;
			return curve.Differentiate(x, keyCaches.First().time, keyCaches.Last().time);
		}

		const float Delta = .000001f;
		public static float Differentiate(this AnimationCurve curve, float x, float xMin, float xMax) {
			var x1 = Mathf.Max(xMin, x - Delta);
			var x2 = Mathf.Min(xMax, x + Delta);
			var y1 = curve.Evaluate(x1);
			var y2 = curve.Evaluate(x2);

			return (y2 - y1) / (x2 - x1);
		}


		static IEnumerable<float> GetPointSlopes(AnimationCurve curve, int resolution) {
			for (var i = 0; i < resolution; i++) {
				yield return curve.Differentiate((float)i / resolution);
			}
		}

		public static AnimationCurve Derivative(this AnimationCurve curve, int resolution = 100, float smoothing = .05f) {
			if (curve.length < 2) { return AnimationCurveExtensions.FLAT_ZERO; }

			var slopes = GetPointSlopes(curve, resolution).ToArray();

			var curvePoints = slopes
				.Select((s, i) => new Vector2((float)i / resolution, s))
				.ToList();

			var simplifiedCurvePoints = new List<Vector2>();

			if (smoothing > 0) {
				LineUtility.Simplify(curvePoints, smoothing, simplifiedCurvePoints);
			} else {
				simplifiedCurvePoints = curvePoints;
			}

			var derivative = new AnimationCurve(
				simplifiedCurvePoints.Select(v => new Keyframe(v.x, v.y)).ToArray());

			Keyframe[] keyframeCache = derivative.keys;
			for (int i = 0, len = keyframeCache.Length; i < len; i++) {
				derivative.SmoothTangents(i, 0);
			}

			return derivative;
		}

		/// <summary>
		/// Find integral of curve between xStart and xEnd using the trapezoidal rule
		/// </summary>
		/// <param name="curve"></param>
		/// <param name="xStart"></param>
		/// <param name="xEnd"></param>
		/// <param name="sliceCount">Resolution of calculation. Increase for better
		/// precision, at cost of computation</param>
		/// <returns>Area under the curve between xStart and xEnd as float</returns>
		public static float Integrate(this AnimationCurve curve, float xStart = 0f, float xEnd = 1f, int sliceCount = 100) {
			var sliceWidth = (xEnd - xStart) / sliceCount;
			var accumulatedTotal = (curve.Evaluate(xStart) + curve.Evaluate(xEnd)) / 2;

			for (var i = 1; i < sliceCount; i++) {
				accumulatedTotal += curve.Evaluate(xStart + i * sliceWidth);
			}

			return sliceWidth * accumulatedTotal;
		}
	}

	public static class IEnumerableExtensions {
		// from: https://stackoverflow.com/questions/1287567/is-using-random-and-orderby-a-good-shuffle-algorithm
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) {
			T[] elements = source.ToArray();
			for (int i = elements.Length - 1; i >= 0; i--) {
				// Swap element "i" with a random earlier element it (or itself)
				// ... except we don't really need to swap it fully, as we can
				// return it immediately, and afterwards it's irrelevant.
				int swapIndex = Random.Range(0, i + 1);
				yield return elements[swapIndex];
				elements[swapIndex] = elements[i];
			}
		}
	}

	public static class IEnumeratorExtensions {
		/// <summary>
		/// 執行enumerator到下一個yield return，支援nested yield return
		/// </summary>
		/// <param name="enumerator"></param>
		/// <returns></returns>
		public static bool RunNext(this IEnumerator enumerator) {
			if (enumerator == null) { return false; }
			if (enumerator.Current is IEnumerator nested &&
				nested.RunNext()) {
				return true;
			} else {
				return enumerator.MoveNext();
			}
		}

		/// <summary>
		/// 完整跑整個IEnumerator的內容，支援nested yeild return
		/// </summary>
		/// <param name="enumerator"></param>
		public static void RunCompletely(this IEnumerator enumerator) {
			if (enumerator == null) { return; }
			do {
				if (enumerator.Current is IEnumerator nested) {
					nested.RunCompletely();
				}
			} while (enumerator.MoveNext());
		}
	}
	public static class GameObjectExtensions {
		/// <summary>
		/// Toggle the active/inactive of gameobject.
		/// </summary>
		/// <param name="gameObject"></param>
		public static void ToggleActive(this GameObject gameObject) {
			gameObject.SetActive(!gameObject.activeSelf);
		}

		// From: SRFGameObjectExtensions
		/// <summary>
		/// Get the component T, or add it to the GameObject if none exists
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetComponentOrAdd<T>(this GameObject obj) where T : Component {
			var t = obj.GetComponent<T>();

			if (t == null) {
				t = obj.AddComponent<T>();
			}

			return t;
		}

		/// <summary>
		/// Removed component of type T if it exists on the GameObject
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		public static void RemoveComponentIfExists<T>(this GameObject obj) where T : Component {
			var t = obj.GetComponent<T>();

			if (t != null) {
				Object.Destroy(t);
			}
		}

		/// <summary>
		/// Removed components of type T if it exists on the GameObject
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		public static void RemoveComponentsIfExists<T>(this GameObject obj) where T : Component {
			var t = obj.GetComponents<T>();

			for (var i = 0; i < t.Length; i++) {
				Object.Destroy(t[i]);
			}
		}

		/// <summary>
		/// Set enabled property MonoBehaviour of type T if it exists
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="enable"></param>
		/// <returns>True if the component exists</returns>
		public static bool EnableComponentIfExists<T>(this GameObject obj, bool enable = true) where T : MonoBehaviour {
			var t = obj.GetComponent<T>();

			if (t == null) {
				return false;
			}

			t.enabled = enable;

			return true;
		}

		/// <summary>
		/// Set the layer of a gameobject and all child objects
		/// </summary>
		/// <param name="o"></param>
		/// <param name="layer"></param>
		public static void SetLayerRecursive(this GameObject o, int layer) {
			SetLayerInternal(o.transform, layer);
		}

		private static void SetLayerInternal(Transform t, int layer) {
			t.gameObject.layer = layer;

			foreach (Transform o in t) {
				SetLayerInternal(o, layer);
			}
		}

		// From: https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html?_ga=2.218918876.496413200.1595297844-254103663.1587534286
		public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component {
			return go.AddComponent<T>().GetCopyOf(toAdd) as T;
		}

		/// <summary>
		/// Removes a GameObject with SetActive(false).
		/// Because of the unity bug that it doesn't call OnDisable() directly on child objects.
		/// </summary>
		/// <param name="go"></param>
		public static void DestroySafe(this GameObject go) {
			if (go) {
				go.SetActive(false);
				Object.Destroy(go);
			}
		}
	}

	public static class ComponentExtensions {
		// From: https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html?_ga=2.218918876.496413200.1595297844-254103663.1587534286
		public static T GetCopyOf<T>(this Component comp, T other) where T : Component {
			Type type = comp.GetType();
			if (type != other.GetType()) return null; // type mis-match
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
			PropertyInfo[] pinfos = type.GetProperties(flags);
			foreach (var pinfo in pinfos) {
				if (pinfo.CanWrite) {
					try {
						pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
					} catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
				}
			}
			FieldInfo[] finfos = type.GetFields(flags);
			foreach (var finfo in finfos) {
				finfo.SetValue(comp, finfo.GetValue(other));
			}
			return comp as T;
		}

		public static T GetComponentOrAdd<T>(this Component comp) where T : Component {
			var t = comp.GetComponent<T>();

			if (t == null) {
				t = comp.gameObject.AddComponent<T>();
			}

			return t;
		}
	}

	/// <summary>
	/// Include float Extensions methods.
	/// </summary>
	public static class MathfExtensions {
		private const float DEFAULT_EPSILON = 0.0001f;

		public static float Floor(this float f, int decimalNum) {
			decimal decPower = ((decimal)Math.Pow(10, decimalNum));
			return (float)(Math.Floor((decimal)f * decPower) / decPower);
		}

		public static float Ceiling(this float f, int decimalNum) {
			// Implement by decimal type to avoid to precision-error.
			decimal decPower = ((decimal)Math.Pow(10, decimalNum));
			return (float)(Math.Ceiling((decimal)f * decPower) / decPower);
		}

		public static float Round(this float f, int decimalNum) {
			// Implement by decimal type to avoid to precision-error.
			return (float)Math.Round((decimal)f, decimalNum);

			// From: https://stackoverflow.com/questions/24548957/casting-a-float-number-into-int-number-will-result-to-invalid-int
			// 必須先強轉型成float讓他的精度校正，才不會出現0.65f.Floor(2)=0.64f的情況
			//return ((int)(float)((f * Mathf.Pow(10, decimalNum)) + 0.5f)) / Mathf.Pow(10, decimalNum);
		}

		/// <summary>
		/// Get the 
		/// </summary>
		/// <param name="f"></param>
		/// <param name="decimalNum"></param>
		/// <returns></returns>
		public static int GetDecimalNumber(this float f, int decimalNum, int approxEpsilon = 6) {
			// Implement by decimal type to avoid to precision-error.
			decimal d = (decimal)f;
			d = Math.Round(d, approxEpsilon);
			return ((int)(d * ((decimal)Math.Pow(10, decimalNum)))) % 10;
		}

		/// <summary>Is the number within Epsilon of zero length?</summary>
		/// <param name="f"></param>
		/// <param name="eps">Epsilon means how small the number will be considered 0</param>
		/// <returns>True if the absolute value of number is within Epsilon of zero</returns>
		public static bool AlmostZero(this float f, float eps = DEFAULT_EPSILON) {
			return Mathf.Abs(f) < eps;
		}

		/// <summary>
		/// Is the number greater than Epsilon?
		/// </summary>
		/// <param name="f"></param>
		/// <param name="eps">Epsilon means how small the number will be considered 0</param>
		/// <returns></returns>
		public static bool GreaterAlmostZero(this float f, float eps = DEFAULT_EPSILON) {
			return f > eps;
		}

		/// <summary>
		/// Is the number less than Epsilon?
		/// </summary>
		/// <param name="f"></param>
		/// <param name="eps">Epsilon means how small the number will be considered 0</param>
		/// <returns></returns>
		public static bool LessAlmostZero(this float f, float eps = DEFAULT_EPSILON) {
			return f < -eps;
		}

		/// <summary>
		/// Wrap angle in range[-180f, 180f)
		/// </summary>
		/// <param name="angle">The angle number</param>
		/// <returns>the number in range[-180f, 180f)</returns>
		public static float WrapAngle180(this float angle) {
			return Mathf.Repeat(angle + 180f, 360f) - 180f;
		}

		/// <summary>
		/// Wrap angle in range[0f, 360f)
		/// </summary>
		/// <param name="angle">The angle number</param>
		/// <returns>the number in range[0f, 360f)</returns>
		public static float WrapAngle360(this float angle) {
			return Mathf.Repeat(angle, 360f);
		}

		/// <summary>
		/// Returns relative angle on the interval (-pi, pi]
		/// </summary>
		/// <param name="current"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static float DeltaAngleRadian(float current, float target) {
			float delta = Mathf.Repeat((target - current), 2 * Mathf.PI);
			if (delta > Mathf.PI)
				delta -= 2 * Mathf.PI;
			return delta;
		}

		/// <summary>
		/// Returns 2 raised to the specified power.
		/// </summary>
		/// <param name="x"></param>
		/// <returns>2^x</returns>
		public static float Exp2(float x) {
			// Magic: https://github.com/Unity-Technologies/Unity.Mathematics/blob/0d092d7b7b10ffb055bd390f4a0cf1d140160d7f/src/Unity.Mathematics/math.cs#L1478
			return (float)System.Math.Exp(x * 0.69314718f);
		}

		/// <summary>
		/// Wrap value to the range[0, max) that means the return value will be equal or greater than 0 and less than max.
		/// </summary>
		/// <param name="max">Max value (non-inclusive)</param>
		/// <param name="value"></param>
		/// <returns>Value wrapped from [0, max)</returns>
		public static int Wrap(this int value, int max) {
			if (max < 0) {
				throw new System.ArgumentOutOfRangeException("max", "max must be greater than 0");
			}

			while (value < 0) {
				value += max;
			}

			while (value >= max) {
				value -= max;
			}

			return value;
		}

		/// <summary>
		/// Wrap value to the range[0, max) that means the return value will be equal or greater than 0 and less than max.
		/// </summary>
		/// <param name="max">Max value (non-inclusive)</param>
		/// <param name="value"></param>
		/// <returns>Value wrapped from [0, max)</returns>
		public static float Wrap(this float value, float max) {
			if (max < 0) {
				throw new System.ArgumentOutOfRangeException("max", "max must be greater than 0");
			}

			while (value < 0) {
				value += max;
			}

			while (value >= max) {
				value -= max;
			}

			return value;
		}

		public static float Average(float v1, float v2) {
			return (v1 + v2) * 0.5f;
		}

		/// <summary>
		/// Return true if two values are of the same signs.
		/// False if two values are of the opposite signs.
		/// If anyone is zero then it always return true.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		public static bool AreSameSign(this float v1, float v2, float eps = 0.0001f) {
			if (v1.AlmostZero(eps) || v2.AlmostZero(eps)) {
				return true;
			}
			return Mathf.Sign(v1) == Mathf.Sign(v2);
		}

		/// <summary>
		/// Return true if two values are of the same signs.
		/// False if two values are of the opposite signs.
		/// If anyone is zero then it always return false.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		public static bool AreSameSignAndNotZero(this float v1, float v2, float eps = 0.0001f) {
			if (v1.AlmostZero(eps) || v2.AlmostZero(eps)) {
				return false;
			}
			return Mathf.Sign(v1) == Mathf.Sign(v2);
		}

		/// <summary>
		/// Return the sign of the f.
		/// Return value is 1 if f is positive, -1 if f is negative, and return 0 if f is zero.
		/// It is identical with Mathf.Sign(f)
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public static float SignOrZero(this float f) {
			return f == 0f ? 0f : Mathf.Sign(f);
		}

		/// <summary>
		/// Returns the absolute value of f.
		/// It is identical with Mathf.Abs(f).
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public static float Abs(this float f) {
			return Mathf.Abs(f);
		}

		/// <summary>
		/// Check two normals are within the specific degrees.
		/// </summary>
		/// <param name="normal1"></param>
		/// <param name="normal2"></param>
		/// <param name="degrees"></param>
		/// <returns></returns>
		public static bool WithinDegrees(in Vector2 normal1, in Vector2 normal2, float degrees) {
			return Vector2.Dot(normal1, normal2) >= Mathf.Cos(degrees * Mathf.Deg2Rad);
		}

		/// <summary>
		/// 把deceleratee減decelerater，最少至零(無速度)
		/// 若雙方方向相反則直接返回deceleratee
		/// </summary>
		/// <returns></returns>
		public static float Decelerate(this float deceleratee, float decelerater) {
			if (deceleratee.AlmostZero()) { return 0f; }
			if (!deceleratee.AreSameSign(decelerater)) {
				return deceleratee;
			}
			if (Mathf.Abs(deceleratee) < Mathf.Abs(decelerater)) {
				return 0f;
			}
			return deceleratee - decelerater;
		}

		/// <summary>
		/// 把acceleratee加accelerater
		/// 若雙方方向相反則直接返回acceleratee
		/// </summary>
		/// <returns></returns>
		public static float Accelerate(this float acceleratee, float accelerater) {
			if (!acceleratee.AreSameSign(accelerater)) {
				return acceleratee;
			}
			return acceleratee + accelerater;
		}

		/// <summary>
		/// 給定一個0 ~ 1的randomize range，回傳一個[value - value * randomize, value]的值
		/// 若randomize range = 2，將會回傳[-value, value]的值
		/// </summary>
		/// <param name="value"></param>
		/// <param name="randomizeRange"></param>
		/// <returns></returns>
		public static float Randomize(this float value, float randomizeRange) {
			float lowerBound = value * (1.0f - randomizeRange);
			if (lowerBound < value) {
				return Random.Range(lowerBound, value);
			} else if (lowerBound > value) {
				return Random.Range(value, lowerBound);
			}
			// range = 0
			return value;
		}

		/// <summary>
		/// Compute the new position for the situation that you want ot set scale based on the specific anchor.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="scale"></param>
		/// <param name="newScale"></param>
		/// <param name="anchor"></param>
		/// <returns></returns>
		public static Vector2 ComputeNewPositionForSetScaleByAnchor(in Vector2 position, in Vector2 scale, in Vector2 newScale, in Vector2 anchor) {
			Vector2 offset = position - anchor;
			Vector2 scaleFactor = new Vector2(
				scale.x != 0f ? newScale.x / scale.x : newScale.x,
				scale.y != 0f ? newScale.y / scale.y : newScale.y
				);
			offset.Scale(scaleFactor);
			return anchor + offset;
		}

		/// <summary>
		/// Signed版本的maximum。
		/// 假設兩者的sign一致，則比較Abs()下的最大值並回傳signed value。
		/// 換句話說是根據sign來找最大 or 最小值。
		/// 若兩者sign不相同，則以major的sign為優先考量，直接回傳major。
		/// 若有一方為0f，則回傳另一方(保證是Abs()最大)。
		/// </summary>
		/// <param name="major"></param>
		/// <param name="minor"></param>
		/// <returns></returns>
		public static float SignedMax(float major, float minor) {
			if (major == 0f && minor == 0f) {
				return 0f;
			}
			if (major == 0f) {
				return minor;
			}
			if (minor == 0f) {
				return major;
			}
			float sign = Mathf.Sign(major);
			if (sign != Mathf.Sign(minor)) {
				return major;
			}

			if (sign > 0f) {
				return Mathf.Max(major, minor);
			} else {
				return Mathf.Min(major, minor);
			}
		}

		public static float Remap(float value, float srcMin, float srcMax, float dstMin, float dstMax) {
			return Mathf.Lerp(dstMin, dstMax, Mathf.InverseLerp(srcMin, srcMax, value));
		}


		/// <summary>
		/// Return the fractional portion of a float value.
		/// </summary>
		/// <param name="floatValue"></param>
		/// <returns></returns>
		public static float Frac(float floatValue) {
			return floatValue - (float)System.Math.Truncate(floatValue);
		}
	}

	public static class StringExtensions {
		/// <summary>
		/// Insensitive Contains(string)
		/// </summary>
		/// <param name="source"></param>
		/// <param name="toCheck"></param>
		/// <param name="comp"> call StringComparison.OrdinalIgnoreCase</param>
		/// <returns></returns>
		public static bool Contains(this string source, string toCheck, StringComparison comp) {
			return source?.IndexOf(toCheck, comp) >= 0;
		}
	}

	public static class ColorExtensions {
		public static readonly Color navy = new Color(0, 0, .5f, 1f);
		public static readonly Color darkBlue = new Color(0, 0, .55f, 1f);
		public static readonly Color darkGreen = new Color(0, .39f, 0, 1f);
		public static readonly Color darkCyan = new Color(0, .55f, .55f, 1f);
		public static readonly Color turquoise = new Color(0, .77f, .8f, 1f);
		public static readonly Color turquoiseBlue = new Color(0, .78f, .55f, 1f);
		public static readonly Color violet = new Color(.31f, .18f, .31f, 1f);
		public static readonly Color purple = new Color(.5f, 0f, .5f, 1f);
		public static readonly Color darkPurple = new Color(.53f, .12f, .47f, 1f);
		public static readonly Color lightBlue = new Color(.56f, .85f, .85f, 1f);
		public static readonly Color yellowGreen = new Color(.6f, .8f, .2f, 1f);
		public static readonly Color brown = new Color(.5f, .16f, .16f, 1f);
		public static readonly Color latte = new Color(.62f, .44f, .23f, 1f);
		public static readonly Color silver = new Color(.75f, .75f, .75f, 1f);
		public static readonly Color orange = new Color(.87f, .46f, 0f, 1f);
		public static readonly Color pink = new Color(.93f, .78f, .93f, 1f);
		public static readonly Color maroon = new Color(.5f, 0, 0, 1f);

		/// <summary>
		/// The color of broken shader.
		/// </summary>
		public static readonly Color brokenShader = new Color(1f, 0f, 1f);
		public static Color RandomColor => new Color(Random.value, Random.value, Random.value);

		/// <summary>
		/// Get this color with specific value of alpha channel.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="alpha">The value of ahpha channel.</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Color WithAlpha(this in Color color, float alpha) {
			return new Color(color.r, color.g, color.b, alpha);
		}

		/// <summary>
		/// 取得輸入的顏色值的RGB，回傳的color的alpha會被設為1.0f
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Color ToRGB1(this in Color color) {
			return new Color(color.r, color.g, color.b, 1.0f);
		}

		public static void DecomposeRGBA(this in Color color, out Color rgb, out float a) {
			rgb = color.ToRGB1();
			a = color.a;
		}

		public static void DecomposeRGBA(this in Color color, out float r, out float g, out float b, out float a) {
			r = color.r;
			g = color.g;
			b = color.b;
			a = color.a;
		}

		/// <summary>
		/// 比原本的Equals和 == 運算子快很多的Equals方法
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool EqualsRGBA(this in Color color, in Color other) {
			return color.r == other.r && color.g == other.g && color.b == other.b && color.a == other.a;
		}

		/// <summary>
		/// 比原本的Equals和 == 運算子快很多的Equals方法
		/// 且不會檢查alpha channel
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool EqualsRGB(this in Color color, in Color other) {
			return color.r == other.r && color.g == other.g && color.b == other.b;
		}

		/// <summary>
		/// Mix two colors correctly with square root.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Color MixColor(in Color a, in Color b, float alpha) {
			return new Color() {
				r = MixColorComponent(a.r, b.r),
				g = MixColorComponent(a.g, b.g),
				b = MixColorComponent(a.b, b.b),
				a = alpha
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float MixColorComponent(float a, float b) {
			return Mathf.Sqrt((a * a + b * b) / 2);
		}

		/// <summary>
		/// Mix two colors by average rgb.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Color MixColorFast(in Color a, in Color b, float alpha) {
			return new Color() {
				r = (a.r + b.r) / 2,
				g = (a.g + b.g) / 2,
				b = (a.b + b.b) / 2,
				a = alpha
			};
		}

		public static string ToHexString(this Color color, bool displayAlpha) {
			Color32 color32 = color;
			return displayAlpha	? 
				string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color32.r, color32.g, color32.b, color32.a)
				: string.Format("#{0:X2}{1:X2}{2:X2}", color32.r, color32.g, color32.b);
		}

		public static bool TryHexToColor(string hexString, out UnityEngine.Color color) {
			if (hexString.Length < 7 || !hexString.StartsWith("#")) {
				color = default;
				return false;
			}
			if (UnityEngine.ColorUtility.TryParseHtmlString(hexString, out color)) {
				color.a = 1f;
				return true;
			}
			return false;
		}
	}

	public static class LayerMaskExtensions {
		/// <summary>
		/// Check if the gameobject is valid or not with this layerMask.
		/// </summary>
		public static bool IsValid(this LayerMask layerMask, GameObject gameObject) {
			return (layerMask & (1 << gameObject.layer)) > 0;
		}
	}

	public static class RendererExtensions {
		/// <summary>
		/// Counts the bounding box corners of the given RectTransform that are visible from the given Camera in screen space.
		/// </summary>
		/// <returns>The amount of bounding box corners that are visible from the Camera.</returns>
		/// <param name="rectTransform">Rect transform.</param>
		/// <param name="camera">Camera.</param>
		private static int CountCornersVisibleFrom(this RectTransform rectTransform) {
			Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
			Vector3[] objectCorners = new Vector3[4];
			rectTransform.GetWorldCorners(objectCorners);

			int visibleCorners = 0;
			for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
			{
				if (screenBounds.Contains(objectCorners[i])) // If the corner is inside the screen
				{
					visibleCorners++;
				}
			}
			return visibleCorners;
		}

		/// <summary>
		/// Determines if this RectTransform is fully visible from the specified camera.
		/// Works by checking if each bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
		/// </summary>
		/// <returns><c>true</c> if is fully visible from the specified camera; otherwise, <c>false</c>.</returns>
		/// <param name="rectTransform">Rect transform.</param>
		/// <param name="camera">Camera.</param>
		public static bool IsFullyVisibleFrom(this RectTransform rectTransform) {
			return CountCornersVisibleFrom(rectTransform) == 4; // True if all 4 corners are visible
		}

		/// <summary>
		/// Determines if this RectTransform is at least partially visible from the specified camera.
		/// Works by checking if any bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
		/// </summary>
		/// <returns><c>true</c> if is at least partially visible from the specified camera; otherwise, <c>false</c>.</returns>
		/// <param name="rectTransform">Rect transform.</param>
		/// <param name="camera">Camera.</param>
		public static bool IsVisibleFrom(this RectTransform rectTransform) {
			return CountCornersVisibleFrom(rectTransform) > 0; // True if any corners are visible
		}
	}

	public static class RenderTextureExtensions {
		/// <summary>
		/// make RenderTexture to Texture2D
		/// </summary>
		/// <param name="renderTexture"></param>
		/// <returns></returns>
		public static Texture2D ToTexture2D(this RenderTexture renderTexture) {
			Texture2D newTexture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
			RenderTexture.active = renderTexture;
			newTexture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			newTexture2D.Apply();

			return newTexture2D;
		}
	}

	public static class ButtonExtentsions {
		public static void SetButtonNormalColor(this UnityEngine.UI.Button button, Color normalColor) {
			var colors = button.colors;
			colors.normalColor = normalColor;
			button.colors = colors;
		}

		public static void SetButtonHighlightColor(this UnityEngine.UI.Button button, Color highlightedColor) {
			var colors = button.colors;
			colors.highlightedColor = highlightedColor;
			button.colors = colors;
		}

		public static void SetButtonPressedColor(this UnityEngine.UI.Button button, Color pressedColor) {
			var colors = button.colors;
			colors.pressedColor = pressedColor;
			button.colors = colors;
		}

		public static void SetButtonSelectedColor(this UnityEngine.UI.Button button, Color selectedColor) {
			var colors = button.colors;
			colors.selectedColor = selectedColor;
			button.colors = colors;
		}

		public static void SetButtonDisabledColor(this UnityEngine.UI.Button button, Color disabledColor) {
			var colors = button.colors;
			colors.disabledColor = disabledColor;
			button.colors = colors;
		}
	}

	public static class SpriteExtensions {
		public static Bounds GetLocalBound(this Sprite sprite) {
			return new Bounds() {
				center = new Vector2((-sprite.pivot.x + sprite.rect.width / 2) / sprite.pixelsPerUnit, (-sprite.pivot.y + sprite.rect.height / 2) / sprite.pixelsPerUnit),
				size = new Vector2(sprite.rect.width / sprite.pixelsPerUnit, sprite.rect.height / sprite.pixelsPerUnit),
			};
		}
	}

	public static class MaterialExtensions {
		public static void SetBooleanParameter(this Material mat, int shaderPropertyID, string shaderKeyword, bool value) {
			if (value) {
				mat.SetFloat(shaderPropertyID, 1f);
				mat.EnableKeyword(shaderKeyword);
			} else {
				mat.SetFloat(shaderPropertyID, 0f);
				mat.DisableKeyword(shaderKeyword);
			}
		}

		public static void SetEnumKeyword(this Material mat, int shaderPropertyID, string[] keywords, int value) {
			mat.SetFloat(shaderPropertyID, value);
			for (int i = 0; i < keywords.Length; i++) {
				string keyword = keywords[i];
				if (i == value) {
					mat.EnableKeyword(keyword);
				} else {
					mat.DisableKeyword(keyword);
				}
			}
		}

		public static int GetEnumValueByKeywordArray(this Material mat, string[] keywords) {
			for (int i = 0; i < keywords.Length; i++) {
				if (mat.IsKeywordEnabled(keywords[i])) {
					return i;
				}
			}
			return -1;
		}
	}

	public static class ListExtensions {
		/// <summary>
		/// Remove all null elements effectively O(n).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		public static void RemoveAllUnityNulls<T>(this List<T> list)
			where T : UnityEngine.Object {
			// https://zhuanlan.zhihu.com/p/23188352
			int count = list.Count;
			// Find first null element O(n)
			int newCount = -1;
			for (int i = 0; i < count; i++) {
				if (!list[i]) {
					newCount = i;
					break;
				}
			}
			if (newCount < 0) {
				return;
			}

			// Find other null elements O(n)
			for (int i = newCount + 1; i < count; i++) {
				// Replace their location to make null elements continous.
				if (list[i]) {
					list[newCount++] = list[i];
				}
			}
			// Remove the continous null elements O(n)
			list.RemoveRange(newCount, count - newCount);
		}

		/// <summary>
		/// Remove all null elements effectively O(n).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		public static void RemoveAllNulls<T>(this List<T> list) {
			// https://zhuanlan.zhihu.com/p/23188352
			int count = list.Count;
			// Find first null element O(n)
			int newCount = -1;
			for (int i = 0; i < count; i++) {
				if (list[i] == null) {
					newCount = i;
					break;
				}
			}
			if (newCount < 0) {
				return;
			}

			// Find other null elements O(n)
			for (int i = newCount + 1; i < count; i++) {
				// Replace their location to make null elements continous.
				if (list[i] != null) {
					list[newCount++] = list[i];
				}
			}
			// Remove the continous null elements O(n)
			list.RemoveRange(newCount, count - newCount);
		}

		/// <summary>
		/// Remove all inactive or disabled elements effectively O(n).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		public static void RemoveAllInactiveOrDisabled<T>(this List<T> list)
			where T : Behaviour {
			// https://zhuanlan.zhihu.com/p/23188352
			int count = list.Count;
			// Find first null element O(n)
			int newCount = -1;
			for (int i = 0; i < count; i++) {
				if (!list[i].isActiveAndEnabled) {
					newCount = i;
					break;
				}
			}
			if (newCount < 0) {
				return;
			}

			// Find other null elements O(n)
			for (int i = newCount + 1; i < count; i++) {
				// Replace their location to make null elements continous.
				if (list[i].isActiveAndEnabled) {
					list[newCount++] = list[i];
				}
			}

			// Remove the continous null elements O(n)
			list.RemoveRange(newCount, count - newCount);
		}

		// From: Unity.Collection packages.
		/// <summary>
		/// Truncates the list by replacing the item at the specified index with the last item in the list. The list
		/// is shortened by one.
		/// </summary>
		/// <typeparam name="T">Source type of elements</typeparam>
		/// <param name="list">List to perform removal.</param>
		/// <param name="item">Item value to remove.</param>
		/// <returns>Returns true if item is removed, if item was not in the container returns false.</returns>
		public static bool RemoveSwapBack<T>(this List<T> list, T item) {
			int index = list.IndexOf(item);
			if (index < 0)
				return false;

			RemoveAtSwapBack(list, index);
			return true;
		}

		/// <summary>
		/// Truncates the list by replacing the item at the specified index with the last item in the list. The list
		/// is shortened by one.
		/// </summary>
		/// <typeparam name="T">Source type of elements</typeparam>
		/// <param name="list">List to perform removal.</param>
		/// <param name="matcher"></param>
		/// <returns>Returns true if item is removed, if item was not in the container returns false.</returns>
		public static bool RemoveSwapBack<T>(this List<T> list, Predicate<T> matcher) {
			int index = list.FindIndex(matcher);
			if (index < 0)
				return false;

			RemoveAtSwapBack(list, index);
			return true;
		}

		/// <summary>
		/// Truncates the list by replacing the item at the specified index with the last item in the list. The list
		/// is shortened by one.
		/// </summary>
		/// <typeparam name="T">Source type of elements</typeparam>
		/// <param name="list">List to perform removal.</param>
		/// <param name="index">The index of the item to delete.</param>
		public static void RemoveAtSwapBack<T>(this List<T> list, int index) {
			int lastIndex = list.Count - 1;
			list[index] = list[lastIndex];
			list.RemoveAt(lastIndex);
		}

		/// <summary>
		/// Insert a element by replacing the item at the specified index with the last item in the list. The list is lengthen by one.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="index"></param>
		/// <param name="element"></param>
		public static void InsertAtSwapBack<T>(this List<T> list, int index, T element) {
			if (index >= list.Count) {
				list.Add(element);
				return;
			}
			list.Add(list[index]);
			list[index] = element;
		}

		/// <summary>
		/// List.AddRange(List) implementation without allocation by IEnumerable.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="toList"></param>
		/// <param name="fromList"></param>
		public static void AddRangeNonAlloc<T>(this List<T> toList, List<T> fromList) {
			foreach (var item in fromList) {
				toList.Add(item);
			}
		}

		/// <summary>
		/// List.AddRange(Array) implementation without allocation by IEnumerable.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="toList"></param>
		/// <param name="fromList"></param>
		public static void AddRangeNonAlloc<T>(this List<T> toList, T[] fromList) {
			foreach (var item in fromList) {
				toList.Add(item);
			}
		}

		public static List<T> Clone<T>(this List<T> list) {
			return new List<T>(list);
		}

		public static void CopyTo<T>(this List<T> from, ref List<T> to) {
			if (from == null) { return; }
			if (to == null) {
				to = new List<T>(from.Capacity);
			}
			to.Clear();
			to.AddRangeNonAlloc(from);
		}
	}

	public static class ArrayExtensions {
		/// <summary>
		/// Truncates the array by replacing the item at the specified index with the last item in the array. The length
		/// is shortened by one.
		/// </summary>
		/// <typeparam name="T">Source type of elements</typeparam>
		/// <param name="array">Array to perform removal.</param>
		/// <param name="index">The index of the item to delete.</param>
		/// <param name="length">The length of the array(must be less than real length of array)</param>
		public static void RemoveAtSwapBack<T>(this T[] array, int index, ref int length) {
			int lastIndex = length - 1;
			array[index] = array[lastIndex];
			length -= 1;
		}
	}

	public static class HashSetExtensions {
		/// <summary>
		/// Add element to hashset if it doesn't contain.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="hashSet"></param>
		/// <param name="value"></param>
		public static void AddIfNotContain<T>(this HashSet<T> hashSet, T value) {
			if (!hashSet.Contains(value)) {
				hashSet.Add(value);
			}
		}
	}
}