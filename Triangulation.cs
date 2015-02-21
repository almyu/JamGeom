using System;
using System.Collections.Generic;
using JamGeom.ClipperLib;
using JamGeom.TessLib;

namespace JamGeom {

    public struct Triangulation {

        public Vec3[] Vertices;
        public int[] Indices;

        public Triangulation(List<Polygon> polygons, bool clockwise = true) {
            var tess = new Tess();

            foreach (var poly in polygons) {
                tess.AddContour(PathToContour(poly.Outline));

                foreach (var hole in poly.Holes)
                    tess.AddContour(PathToContour(hole));
            }

            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            Vertices = Array.ConvertAll(tess.Vertices, v => v.Position);
            Array.Resize(ref Vertices, tess.VertexCount);

            Indices = tess.Elements;
            Array.Resize(ref Indices, tess.ElementCount * 3);

            if (clockwise) Array.Reverse(Indices);
        }

        private static ContourVertex[] PathToContour(List<IntPoint> path) {
            var result = new ContourVertex[path.Count];

            for (int i = 0; i < result.Length; ++i) {
                var p = path[i];
                result[i] = new ContourVertex { Position = new Vec3 { X = p.X, Y = p.Y } };
            }
            return result;
        }
    }
}
