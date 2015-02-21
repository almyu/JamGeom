#if UNITY_4_0 || UNITY_5_0

using UnityEngine;

#if UNITY_EDITOR
using System.Diagnostics;
using UnityEditor;
#endif

namespace JamGeom {

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TextureToMeshPreview : MonoBehaviour {

        public Texture2D texture;
        public TextureToMesh settings;

        private void Awake() {
            Trace();
        }

        public void Trace() {
            GetComponent<MeshFilter>().sharedMesh = settings.Trace(texture);
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(TextureToMeshPreview))]
    public class TextureToMeshPreviewEditor : Editor {

        private static float lastTiming = 0f;

        private static bool SetReadable(Texture2D tex, bool readable) {
            var path = AssetDatabase.GetAssetPath(tex);
            var imp = (TextureImporter) AssetImporter.GetAtPath(path);

            if (imp.isReadable == readable)
                return readable;

            imp.isReadable = readable;
            imp.SaveAndReimport();

            return !readable;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var preview = target as TextureToMeshPreview;

            if (preview.texture && GUILayout.Button("Trace")) {
                var wasReadable = SetReadable(preview.texture, true);

                var sw = new Stopwatch();
                sw.Start();

                preview.Trace();

                sw.Stop();
                lastTiming = (float) sw.Elapsed.TotalMilliseconds;

                SetReadable(preview.texture, wasReadable);
            }

            var mesh = preview.GetComponent<MeshFilter>().sharedMesh;

            if (mesh && GUILayout.Button("Save")) {
                var path = EditorUtility.SaveFilePanelInProject("Save mesh...", mesh.name, "asset", "");
                if (!string.IsNullOrEmpty(path))
                    AssetDatabase.CreateAsset(mesh, path);
            }

            if (lastTiming != 0f)
                EditorGUILayout.HelpBox("Time: " + lastTiming + " ms", MessageType.Info); ;
        }
    }
#endif
}

#endif
