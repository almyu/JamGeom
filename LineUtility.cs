#if UNITY_4_0 || UNITY_5_0

using UnityEngine;

namespace JamGeom {

    public static class LineUtility {

        public static Vector2 EdgeNormal(Vector2 a, Vector2 b) {
            return new Vector2(b.y - a.y, a.x - b.x).normalized;
        }

        public static Vector2 CornerNormal(Vector2 a, Vector2 b, Vector2 c) {
            var n0 = EdgeNormal(a, b);
            var n1 = EdgeNormal(b, c);

            var rlen = Mathf.Sqrt(Vector2.Dot(n0, n1) * 0.5f + 0.5f);

            return (n0 + n1).normalized / rlen;
        }

        public static Vector2 OffsetPoint(Vector2 prev, Vector2 point, Vector2 next, float offset) {
            if (prev == point) prev = point - next + point;
            if (next == point) next = point - prev + point;

            return point + CornerNormal(prev, point, next) * offset;
        }

        public static Vector2[] OffsetLine(Vector2[] points, float offset) {
            var count = points.Length;
            if (count < 2) return points;

            var result = new Vector2[count];

            result[0] = OffsetPoint(points[0], points[0], points[1], offset);
            result[count - 1] = OffsetPoint(points[count - 2], points[count - 1], points[count - 1], offset);

            for (int i = 1; i + 1 < count; ++i)
                result[i] = OffsetPoint(points[i - 1], points[i], points[i + 1], offset);

            return result;
        }

        public static Vector2[] OffsetLoop(Vector2[] points, float offset) {
            var count = points.Length;
            var result = new Vector2[count];

            for (int i = 0; i < count; ++i) {
                var iPrev = (i + count - 1) % count;
                var iNext = (i + 1) % count;

                result[i] = OffsetPoint(points[iPrev], points[i], points[iNext], offset);
            }

            return result;
        }
    }
}

#endif
