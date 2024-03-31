using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace YourNamespace
{
    public class SlideshowController : MonoBehaviour
    {
        public string folderPath = "Assets/Slideshow/Photos";
        public GameObject photoFramePrefab;
        public Transform slideshowParent;
        public VRCUrlInputField folderInputField;

        public Button enlargeButton;
        public Button overviewButton;
        public Button nextButton;
        public Button prevButton;
        public Button folderSelectButton;

        private GameObject[] photoFrames;
        private int currentIndex = 0;

        private void Start()
        {
            // ボタンのイベントリスナーを設定
            enlargeButton.onClick.AddListener(EnlargeCurrentSlideshow);
            overviewButton.onClick.AddListener(ShowOverview);
            nextButton.onClick.AddListener(ShowNextSlideshow);
            prevButton.onClick.AddListener(ShowPreviousSlideshow);
            folderSelectButton.onClick.AddListener(SetFolderPath);

            // 初期スライドショーを生成
            GenerateSlideshow();
        }

        private void GenerateSlideshow()
        {
            // 既存のフォトフレームを削除
            foreach (Transform child in slideshowParent)
            {
                Destroy(child.gameObject);
            }

            // 新しいフォトフレームを生成
            string[] photoPaths = System.IO.Directory.GetFiles(folderPath);
            photoFrames = new GameObject[photoPaths.Length];

            for (int i = 0; i < photoPaths.Length; i++)
            {
                GameObject photoFrame = Instantiate(photoFramePrefab, slideshowParent);
                // フォトフレームの設定を行う（例：テクスチャの割り当てなど）
                photoFrames[i] = photoFrame;
            }

            // 最初のスライドショーを表示
            ShowSlideshow(0);
        }

        private void ShowSlideshow(int index)
        {
            // 現在のフォトフレームを非アクティブ化
            if (currentIndex >= 0 && currentIndex < photoFrames.Length)
            {
                photoFrames[currentIndex].SetActive(false);
            }

            // 指定されたインデックスのフォトフレームをアクティブ化
            currentIndex = index;
            photoFrames[currentIndex].SetActive(true);
        }

        private void EnlargeCurrentSlideshow()
        {
            // 現在のスライドショーを拡大表示する処理を実装
        }

        private void ShowOverview()
        {
            // スライドショーの一覧を表示する処理を実装
        }

        private void ShowNextSlideshow()
        {
            int nextIndex = (currentIndex + 1) % photoFrames.Length;
            ShowSlideshow(nextIndex);
        }

        private void ShowPreviousSlideshow()
        {
            int prevIndex = (currentIndex - 1 + photoFrames.Length) % photoFrames.Length;
            ShowSlideshow(prevIndex);
        }

        private void SetFolderPath()
        {
            folderPath = folderInputField.GetUrl().Get();
            GenerateSlideshow();
        }
    }
}