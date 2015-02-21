#if UNITY_4_0 || UNITY_5_0

using System;
using UnityEngine;

namespace JamGeom {

    [Serializable]
    public class TextureToMesh {

        public byte alphaThreshold = 128;
        public float pixelsPerUnit = 100f;
        public PathQuality quality = PathQuality.Medium;


        public Mesh Trace(Texture2D tex) {
            var mask = MaskTexture(tex, alphaThreshold);
            var polygons = mask.TracePolygons();

            Polygon.Simplify(polygons, quality);

            var tri = new Triangulation(polygons);
            var mesh = MakeMesh(tri, 1f / pixelsPerUnit);

            mesh.name = tex.name;

            var uvScaleOffset = new Vector4(
                pixelsPerUnit / tex.width,
                pixelsPerUnit / tex.height,
                0f, 0f);

            MapPlanarUV(mesh, uvScaleOffset);

            return mesh;
        }


        public static Mask MaskTexture(Texture2D tex, byte alphaThreshold = 128) {
            return MaskTexture(tex, c => c.a >= alphaThreshold);
        }

        public static Mask MaskTexture(Texture2D tex, Func<Color32, bool> pred) {
            int w = tex.width, h = tex.height;
            var bits = new bool[h, w];

            var colors = tex.GetPixels32();

            for (int y = 0; y < h; ++y) {
                int scan = w * y;
                for (int x = 0; x < w; ++x)
                    bits[y, x] = pred(colors[scan + x]);
            }

            return new Mask(bits);
        }

        public static Mesh MakeMesh(Triangulation tri, float scale = 0.01f) {
            var mesh = new Mesh();

            mesh.vertices = Array.ConvertAll(tri.Vertices, p => new Vector3(p.X * scale, p.Y * scale));
            mesh.triangles = tri.Indices;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

        public static void MapPlanarUV(Mesh mesh, Vector4 uvScaleOffset) {
            mesh.uv = Array.ConvertAll(mesh.vertices, v =>
                new Vector2(v.x * uvScaleOffset.x + uvScaleOffset.z, v.y * uvScaleOffset.y + uvScaleOffset.w));
        }
    }
}

#endif
