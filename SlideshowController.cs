using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace YourNamespace
{
    public class SlideshowController : MonoBehaviour
    {
        public string folderPath = "Assets/Slideshow/Photos";
        public GameObject photoFramePrefab;
        public Transform slideshowParent;
        public InputField folderInputField;
        public Text statusText;

        public Button enlargeButton;
        public Button overviewButton;
        public Button nextButton;
        public Button prevButton;
        public Button folderSelectButton;

        private GameObject[] photoFrames = Array.Empty<GameObject>();
        private int currentIndex = -1;

        private void Start()
        {
            if (enlargeButton != null) enlargeButton.onClick.AddListener(EnlargeCurrentSlideshow);
            if (overviewButton != null) overviewButton.onClick.AddListener(ShowOverview);
            if (nextButton != null) nextButton.onClick.AddListener(ShowNextSlideshow);
            if (prevButton != null) prevButton.onClick.AddListener(ShowPreviousSlideshow);
            if (folderSelectButton != null) folderSelectButton.onClick.AddListener(SetFolderPath);

            GenerateSlideshow();
        }

        public void GenerateSlideshow()
        {
            ClearFrames();

            string[] photoPaths;
            try
            {
                photoPaths = SlideshowPathPolicy.EnumerateImages(folderPath);
            }
            catch (Exception exception) when (exception is ArgumentException || exception is DirectoryNotFoundException || exception is IOException)
            {
                SetEmptyState(exception.Message);
                Debug.LogWarning(exception.Message);
                return;
            }

            if (photoPaths.Length == 0)
            {
                SetEmptyState("No supported images were found. Supported formats: .jpg, .jpeg, .png");
                return;
            }

            photoFrames = new GameObject[photoPaths.Length];
            for (int i = 0; i < photoPaths.Length; i++)
            {
                GameObject photoFrame = Instantiate(photoFramePrefab, slideshowParent);
                photoFrame.name = Path.GetFileNameWithoutExtension(photoPaths[i]);
                photoFrame.SetActive(false);
                photoFrames[i] = photoFrame;
            }

            SetNavigationEnabled(true);
            SetStatus($"Loaded {photoFrames.Length} image(s).");
            ShowSlideshow(0);
        }

        public void ShowSlideshow(int index)
        {
            if (!SlideshowPathPolicy.IsValidIndex(index, photoFrames.Length))
            {
                return;
            }

            if (SlideshowPathPolicy.IsValidIndex(currentIndex, photoFrames.Length))
            {
                photoFrames[currentIndex].SetActive(false);
            }

            currentIndex = index;
            photoFrames[currentIndex].SetActive(true);
        }

        private void ClearFrames()
        {
            if (slideshowParent != null)
            {
                foreach (Transform child in slideshowParent)
                {
                    Destroy(child.gameObject);
                }
            }

            photoFrames = Array.Empty<GameObject>();
            currentIndex = -1;
        }

        private void SetEmptyState(string message)
        {
            SetNavigationEnabled(false);
            SetStatus(message);
        }

        private void SetNavigationEnabled(bool enabled)
        {
            if (nextButton != null) nextButton.interactable = enabled;
            if (prevButton != null) prevButton.interactable = enabled;
            if (enlargeButton != null) enlargeButton.interactable = enabled;
            if (overviewButton != null) overviewButton.interactable = enabled;
        }

        private void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        private void EnlargeCurrentSlideshow() { }
        private void ShowOverview() { }

        public void ShowNextSlideshow()
        {
            int nextIndex = SlideshowPathPolicy.WrapIndex(currentIndex, 1, photoFrames.Length);
            ShowSlideshow(nextIndex);
        }

        public void ShowPreviousSlideshow()
        {
            int previousIndex = SlideshowPathPolicy.WrapIndex(currentIndex, -1, photoFrames.Length);
            ShowSlideshow(previousIndex);
        }

        public void SetFolderPath()
        {
            if (folderInputField == null)
            {
                SetEmptyState("Local folder input is not configured.");
                return;
            }

            folderPath = folderInputField.text;
            GenerateSlideshow();
        }
    }
}
