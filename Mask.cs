using System.Collections.Generic;
using JamGeom.ClipperLib;

namespace JamGeom {

    public struct Mask {

        public readonly int Width, Height;
        private bool[,] bits;

        public Mask(int width, int height) : this(new bool[height, width]) {}

        public Mask(bool[,] bits) {
            this.bits = bits;
            Width = bits.GetUpperBound(1);
            Height = bits.GetUpperBound(0);
        }

        public bool this[int x, int y] {
            get { return Contains(x, y) && bits[y, x]; }
            set { if (Contains(x, y)) bits[y, x] = value; }
        }

        public bool this[IntPoint p] {
            get { return this[p.X, p.Y]; }
            set { this[p.X, p.Y] = value; }
        }

        public bool Contains(int x, int y) {
            return y >= 0 && y < Height && x >= 0 && x < Width;
        }
    }


    public static class MaskExt {

        private static readonly IntPoint[] offsets = new[] {
            new IntPoint( 1,  0), new IntPoint( 1,  1),
            new IntPoint( 0,  1), new IntPoint(-1,  1),
            new IntPoint(-1,  0), new IntPoint(-1, -1),
            new IntPoint( 0, -1), new IntPoint( 1, -1)
        };

        private static IntPoint OffsetPoint(IntPoint point, int offsetIndex) {
            var offset = offsets[offsetIndex];
            return new IntPoint(point.X + offset.X, point.Y + offset.Y);
        }

        public static List<IntPoint> RollPath(Mask mask, IntPoint start) {
            var @ref = mask[start];

            var points = new List<IntPoint>();
            var current = start;

            var nextOffset = 0;

            // go outside
            for (; nextOffset < 8; ++nextOffset)
                if (mask[OffsetPoint(current, nextOffset)] != @ref)
                    break;

            do {
                int i;
                for (i = 0; i < 8; ++i) {
                    var offset = (nextOffset + i) & 7;
                    var candidate = OffsetPoint(current, offset);

                    if (mask[candidate] != @ref) continue;

                    points.Add(candidate);
                    current = candidate;

                    nextOffset = offset + 5;
                    break;
                }
                if (i == 8) break; // single point case
            }
            while (current != start);

            return points;
        }

        public static List<Polygon> TracePolygons(this Mask mask) {
            int[,] idMap;
            return TracePolygons(mask, out idMap);
        }

        public static List<Polygon> TracePolygons(this Mask mask, out int[,] idMap) {
            var polygons = new List<Polygon>();

            idMap = new int[mask.Height, mask.Width];

            for (int y = 0; y < mask.Height; ++y) {
                var wasInside = false;
                var lastId = 0;

                for (int x = 0; x <= mask.Width; ++x) {
                    var inside = x != mask.Width && mask[x, y];
                    if (inside == wasInside) continue;

                    wasInside = inside;

                    var edge = inside ? x : x - 1;
                    var edgeOwner = idMap[y, edge];

                    if (edgeOwner != 0) {
                        lastId = edgeOwner;
                        continue;
                    }

                    var points = RollPath(mask, new IntPoint(edge, y));

                    if (inside) {
                        polygons.Add(new Polygon(points));
                        lastId = polygons.Count;
                    }
                    else polygons[lastId - 1].Holes.Add(points);

                    foreach (var p in points)
                        idMap[p.Y, p.X] = lastId;
                }
            }
            return polygons;
        }
    }
}
