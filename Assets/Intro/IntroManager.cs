using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    // Định nghĩa các loại nội dung
    public enum MediaType { Image, Video }

    // Tạo một class để cấu hình cho từng "Slide" trong Inspector
    [System.Serializable]
    public class IntroStep
    {
        public string description;  // Tên gợi nhớ (ví dụ: "Cảnh trường học", "Video quái vật")
        public MediaType type;      // Chọn loại: Image hoặc Video
        
        [Header("Nếu là Ảnh")]
        public Sprite sprite;       // Kéo ảnh vào đây
        public float duration = 3f; // Thời gian hiện ảnh
        
        [Header("Nếu là Video")]
        public VideoClip video;     // Kéo video vào đây
    }

    [Header("Playlist")]
    public List<IntroStep> introSequence; // Danh sách các bước Intro

    [Header("UI Components")]
    public Image storyImageUI;           // Object hiển thị ảnh
    public CanvasGroup storyCanvasGroup; // CanvasGroup của ảnh
    
    public RawImage videoRawImage;       // Object hiển thị video
    public VideoPlayer videoPlayer;      // Video Player Component
    public CanvasGroup videoCanvasGroup; // CanvasGroup của video

    [Header("Settings")]
    public float fadeDuration = 1f;      // Thời gian mờ dần
    public string nextSceneName = "GameplayScene";

    private bool isSkipping = false;     // Biến kiểm tra nếu người chơi bấm Skip

    void Start()
    {
        // Ẩn tất cả ban đầu
        storyCanvasGroup.alpha = 0;
        videoCanvasGroup.alpha = 0;

        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        // Tính năng Skip: Nhấn Space hoặc Enter để bỏ qua toàn bộ Intro
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            SkipIntro();
        }
    }

    IEnumerator PlaySequence()
    {
        foreach (IntroStep step in introSequence)
        {
            if (isSkipping) break; // Nếu bấm skip thì thoát vòng lặp ngay

            if (step.type == MediaType.Image)
            {
                // --- XỬ LÝ ẢNH ---
                storyImageUI.sprite = step.sprite;
                
                // Fade In Ảnh
                yield return StartCoroutine(FadeCanvasGroup(storyCanvasGroup, 0f, 1f));
                
                // Chờ thời gian hiển thị (hoặc chờ bấm chuột để next - tùy chọn)
                yield return new WaitForSeconds(step.duration);
                
                // Fade Out Ảnh
                yield return StartCoroutine(FadeCanvasGroup(storyCanvasGroup, 1f, 0f));
            }
            else if (step.type == MediaType.Video)
            {
                // --- XỬ LÝ VIDEO ---
                if (step.video != null)
                {
                    videoPlayer.clip = step.video;
                    videoPlayer.Prepare();

                    // Đợi video load xong để tránh màn hình đen
                    while (!videoPlayer.isPrepared) yield return null;

                    videoRawImage.texture = videoPlayer.texture;
                    videoPlayer.Play();

                    // Fade In Video
                    yield return StartCoroutine(FadeCanvasGroup(videoCanvasGroup, 0f, 1f));

                    // Chờ hết thời gian video
                    yield return new WaitForSeconds((float)step.video.length);

                    // Fade Out Video
                    yield return StartCoroutine(FadeCanvasGroup(videoCanvasGroup, 1f, 0f));
                    
                    videoPlayer.Stop(); // Dừng hẳn video
                }
            }

            // Nghỉ 1 xíu giữa các chuyển cảnh cho mượt (tùy chọn)
            yield return new WaitForSeconds(0.5f);
        }

        LoadNextScene();
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            if (isSkipping) break; // Thoát fade nếu skip
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }
        if (!isSkipping) cg.alpha = endAlpha;
    }

    public void SkipIntro()
    {
        if (!isSkipping)
        {
            isSkipping = true;
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}