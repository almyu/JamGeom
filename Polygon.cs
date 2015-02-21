using System.Collections.Generic;
using JamGeom.ClipperLib;

namespace JamGeom {

    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    public enum PathQuality { Fine, Medium, Rough }

    public class Polygon {
        public Path Outline;
        public Paths Holes = new Paths();

        public Polygon(Path points) {
            Outline = points;
        }


        public static readonly double[] QualitySlopes = new[] { 0.1, 0.5, 1.0 };

        public static Path SimplifyPath(Path path, PathQuality quality = PathQuality.Medium) {
            return Clipper.CleanPolygon(path, QualitySlopes[(int) quality]);
        }

        public void Simplify(PathQuality quality = PathQuality.Medium) {
            Outline = SimplifyPath(Outline, quality);

            for (int i = 0; i < Holes.Count; ++i)
                Holes[i] = SimplifyPath(Holes[i], quality);
        }

        public static void Simplify(List<Polygon> polygons, PathQuality quality = PathQuality.Medium) {
            foreach (var poly in polygons)
                poly.Simplify(quality);
        }
    }
}
