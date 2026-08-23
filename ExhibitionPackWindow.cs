using UnityEditor;
using UnityEngine;

namespace YourNamespace
{
    public class ExhibitionPackWindow : EditorWindow
    {
        private string manifestPath = "Assets/Exhibition/exhibition.json";
        private string outputPath = "Assets/Exhibition/Generated";
        private GameObject photoFramePrefab;

        [MenuItem("Tools/Exhibition Pack Generator")]
        public static void ShowWindow() => GetWindow<ExhibitionPackWindow>("Exhibition Pack Generator");

        private void OnGUI()
        {
            GUILayout.Label("Build-time Exhibition Pack", EditorStyles.boldLabel);
            manifestPath = EditorGUILayout.TextField("Manifest Asset Path", manifestPath);
            outputPath = EditorGUILayout.TextField("Output Asset Directory", outputPath);
            photoFramePrefab = EditorGUILayout.ObjectField(
                "Photo Frame Prefab", photoFramePrefab, typeof(GameObject), false) as GameObject;
            EditorGUILayout.HelpBox(
                "Generates fixed Unity assets from an explicit manifest. It never enumerates an end user's PC at VRChat runtime.",
                MessageType.Info);

            if (GUILayout.Button("Generate Exhibition Pack")) Generate();
        }

        private void Generate()
        {
            if (photoFramePrefab == null)
            {
                Debug.LogError("Photo Frame Prefab is not assigned.");
                return;
            }

            try
            {
                bool generated = ExhibitionPackGenerator.Generate(manifestPath, outputPath, photoFramePrefab);
                if (generated) Debug.Log("Exhibition Pack generated. Review exhibition-report.json/.md before delivery.");
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Exhibition Pack generation failed: {exception.Message}");
            }
        }
    }
}
