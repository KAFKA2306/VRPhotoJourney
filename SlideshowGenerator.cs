using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace YourNamespace
{
    public class SlideshowGenerator : EditorWindow
    {
        private string folderPath = "Assets/Slideshow/Photos";
        private GameObject slideshowPrefab;
        private GameObject photoFramePrefab;

        [MenuItem("Tools/Slideshow Generator")]
        public static void ShowWindow() => GetWindow<SlideshowGenerator>("Slideshow Generator");

        private void OnGUI()
        {
            GUILayout.Label("Slideshow Generator", EditorStyles.boldLabel);
            folderPath = EditorGUILayout.TextField("Photo Folder Path", folderPath);
            slideshowPrefab = EditorGUILayout.ObjectField("Slideshow Prefab", slideshowPrefab, typeof(GameObject), false) as GameObject;
            photoFramePrefab = EditorGUILayout.ObjectField("Photo Frame Prefab", photoFramePrefab, typeof(GameObject), false) as GameObject;

            if (GUILayout.Button("Generate Slideshow")) GenerateSlideshow();
        }

        private void GenerateSlideshow()
        {
            if (slideshowPrefab == null || photoFramePrefab == null)
            {
                Debug.LogError("Slideshow Prefab or Photo Frame Prefab is not assigned.");
                return;
            }

            GameObject slideshowParent = new GameObject("Slideshow Parent");
            SlideshowController controller = slideshowParent.AddComponent<SlideshowController>();
            controller.folderPath = folderPath;
            controller.photoFramePrefab = photoFramePrefab;
            controller.slideshowParent = slideshowParent.transform;

            GameObject canvas = new GameObject("Slideshow Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            canvas.transform.SetParent(slideshowParent.transform, false);

            GameObject inputField = CreateUIObject("Local Folder Input Field", canvas.transform, typeof(Image), typeof(InputField));
            controller.folderInputField = inputField.GetComponent<InputField>();
            controller.folderInputField.text = folderPath;

            GameObject status = CreateUIObject("Status Text", canvas.transform, typeof(Text));
            controller.statusText = status.GetComponent<Text>();
            controller.statusText.text = "Not loaded";

            controller.enlargeButton = CreateButton("Enlarge Button", canvas.transform);
            controller.overviewButton = CreateButton("Overview Button", canvas.transform);
            controller.nextButton = CreateButton("Next Button", canvas.transform);
            controller.prevButton = CreateButton("Prev Button", canvas.transform);
            controller.folderSelectButton = CreateButton("Folder Select Button", canvas.transform);
        }

        private static Button CreateButton(string name, Transform parent)
        {
            return CreateUIObject(name, parent, typeof(Image), typeof(Button)).GetComponent<Button>();
        }

        private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
        {
            GameObject uiObject = new GameObject(name, components);
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }
    }
}
