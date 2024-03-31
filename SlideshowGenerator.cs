using UnityEngine;
using UnityEditor;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace YourNamespace
{
    public class SlideshowGenerator : EditorWindow
    {
        private string folderPath = "Assets/Slideshow/Photos";
        private GameObject slideshowPrefab;
        private GameObject photoFramePrefab;

        [MenuItem("Tools/Slideshow Generator")]
        public static void ShowWindow()
        {
            GetWindow<SlideshowGenerator>("Slideshow Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Slideshow Generator", EditorStyles.boldLabel);

            folderPath = EditorGUILayout.TextField("Photo Folder Path", folderPath);
            slideshowPrefab = EditorGUILayout.ObjectField("Slideshow Prefab", slideshowPrefab, typeof(GameObject), false) as GameObject;
            photoFramePrefab = EditorGUILayout.ObjectField("Photo Frame Prefab", photoFramePrefab, typeof(GameObject), false) as GameObject;

            if (GUILayout.Button("Generate Slideshow"))
            {
                GenerateSlideshow();
            }
        }

        private void GenerateSlideshow()
        {
            if (slideshowPrefab == null || photoFramePrefab == null)
            {
                Debug.LogError("Slideshow Prefab or Photo Frame Prefab is not assigned.");
                return;
            }

            GameObject slideshowParent = new GameObject("Slideshow Parent");

            SlideshowController slideshowController = slideshowParent.AddComponent<SlideshowController>();
            slideshowController.folderPath = folderPath;
            slideshowController.photoFramePrefab = photoFramePrefab;
            slideshowController.slideshowParent = slideshowParent.transform;

            GameObject canvas = new GameObject("Slideshow Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            canvas.transform.SetParent(slideshowParent.transform, false);

            GameObject inputField = CreateUIObject("Folder Input Field", canvas.transform, typeof(VRCUrlInputField));
            slideshowController.folderInputField = inputField.GetComponent<VRCUrlInputField>();

            GameObject enlargeButton = CreateUIObject("Enlarge Button", canvas.transform, typeof(Button));
            slideshowController.enlargeButton = enlargeButton.GetComponent<Button>();

            GameObject overviewButton = CreateUIObject("Overview Button", canvas.transform, typeof(Button));
            slideshowController.overviewButton = overviewButton.GetComponent<Button>();

            GameObject nextButton = CreateUIObject("Next Button", canvas.transform, typeof(Button));
            slideshowController.nextButton = nextButton.GetComponent<Button>();

            GameObject prevButton = CreateUIObject("Prev Button", canvas.transform, typeof(Button));
            slideshowController.prevButton = prevButton.GetComponent<Button>();

            GameObject folderSelectButton = CreateUIObject("Folder Select Button", canvas.transform, typeof(Button));
            slideshowController.folderSelectButton = folderSelectButton.GetComponent<Button>();

            // Canvasの位置とスケールを調整
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.localPosition = Vector3.zero;
            canvasRect.localScale = Vector3.one;

            // InputFieldの位置とサイズを調整
            RectTransform inputFieldRect = inputField.GetComponent<RectTransform>();
            inputFieldRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputFieldRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputFieldRect.anchoredPosition = new Vector2(0f, 100f);
            inputFieldRect.sizeDelta = new Vector2(400f, 50f);

            // ボタンの位置とサイズを調整
            RectTransform enlargeButtonRect = enlargeButton.GetComponent<RectTransform>();
            enlargeButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            enlargeButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            enlargeButtonRect.anchoredPosition = new Vector2(-150f, -100f);
            enlargeButtonRect.sizeDelta = new Vector2(100f, 50f);

            RectTransform overviewButtonRect = overviewButton.GetComponent<RectTransform>();
            overviewButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            overviewButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            overviewButtonRect.anchoredPosition = new Vector2(-50f, -100f);
            overviewButtonRect.sizeDelta = new Vector2(100f, 50f);

            RectTransform nextButtonRect = nextButton.GetComponent<RectTransform>();
            nextButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            nextButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            nextButtonRect.anchoredPosition = new Vector2(50f, -100f);
            nextButtonRect.sizeDelta = new Vector2(100f, 50f);

            RectTransform prevButtonRect = prevButton.GetComponent<RectTransform>();
            prevButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            prevButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            prevButtonRect.anchoredPosition = new Vector2(150f, -100f);
            prevButtonRect.sizeDelta = new Vector2(100f, 50f);

            RectTransform folderSelectButtonRect = folderSelectButton.GetComponent<RectTransform>();
            folderSelectButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            folderSelectButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            folderSelectButtonRect.anchoredPosition = new Vector2(0f, 0f);
            folderSelectButtonRect.sizeDelta = new Vector2(200f, 50f);
        }

        private GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
        {
            GameObject uiObject = new GameObject(name, components);
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }
    }
}