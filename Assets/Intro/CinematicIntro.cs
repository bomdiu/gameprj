using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CinematicIntro : MonoBehaviour
{
    [Header("Overlay & Effects")]
    public CanvasGroup fadeOverlay;      // Màn che đen (Fade Overlay)
    public float transitionSpeed = 1.0f; // Tốc độ chuyển cảnh đen
    public RawImage noiseOverlay;        // (Mới) Lớp nhiễu hạt/Texture động

    [Header("1. Video Settings")]
    public VideoClip introVideo;
    [Range(0.1f, 3.0f)] public float videoSpeed = 1.0f;
    public RawImage videoDisplay;
    public VideoPlayer videoPlayer;
    public CanvasGroup videoCG;

    [System.Serializable]
    public class StoryPair
    {
        public string note;             // Ghi chú cho dễ nhớ
        public Sprite image;            // Ảnh hiển thị
        public float displayTime = 4f;  // Thời gian dừng hình
        public bool isNewScene;         // True = Tối sầm màn hình rồi mới hiện
        public bool useKenBurns = true; // True = Bật hiệu ứng Zoom chậm
        public float zoomScale = 0.1f;  // Zoom đến mức nào (VD: 1.1 là to hơn 10%)
    }

    [Header("2. Image Sequence")]
    public List<StoryPair> imageSequence;
    
    [Header("UI Components")]
    public Image backImage;  // Layer hiển thị chính
    public Image frontImage; // Layer dùng để cross-fade đè lên
    public CanvasGroup frontCG;

    [Header("Navigation")]
    public string nextSceneName = "GameplayScene";

    // Biến nội bộ để quản lý Ken Burns
    private Coroutine currentKenBurns;

    void Start()
    {
        // Setup ban đầu
        videoDisplay.gameObject.SetActive(false); 
        backImage.color = Color.white;
        frontImage.color = Color.white;
        frontCG.alpha = 0; 
        
        // Đảm bảo Noise Overlay luôn bật (nếu có)
        if (noiseOverlay != null) noiseOverlay.gameObject.SetActive(true);

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // ============ PHASE 1: VIDEO ============
        if (introVideo != null)
        {
            videoDisplay.gameObject.SetActive(true);
            videoPlayer.clip = introVideo;
            videoPlayer.playbackSpeed = videoSpeed;
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared) yield return null;
            
            videoDisplay.texture = videoPlayer.texture;
            videoPlayer.Play();

            // Fade in Video từ đen
            yield return StartCoroutine(FadeAlpha(fadeOverlay, 1f, 0f));

            // Chờ video chạy
            float duration = (float)introVideo.length / videoSpeed;
            yield return new WaitForSeconds(duration);

            // Fade out Video về đen
            yield return StartCoroutine(FadeAlpha(fadeOverlay, 0f, 1f));
            
            videoPlayer.Stop();
            videoDisplay.gameObject.SetActive(false);
        }

        // ============ PHASE 2: ẢNH TĨNH (KÈM KEN BURNS) ============
        
        // Setup ảnh đầu tiên trong bóng tối
        if (imageSequence.Count > 0)
        {
            backImage.sprite = imageSequence[0].image;
            // Reset scale ảnh đầu
            backImage.rectTransform.localScale = Vector3.one; 
        }

        // Hiện ảnh đầu tiên: Mở màn đen
        yield return StartCoroutine(FadeAlpha(fadeOverlay, 1f, 0f));

        // Bắt đầu chạy từng ảnh
        for (int i = 0; i < imageSequence.Count; i++)
        {
            StoryPair step = imageSequence[i];

            // 1. Kích hoạt Ken Burns cho ảnh hiện tại (backImage)
            // Nếu có coroutine cũ đang chạy, dừng nó lại
            if (currentKenBurns != null) StopCoroutine(currentKenBurns);
            if (step.useKenBurns)
            {
                // Chạy Ken Burns trên backImage trong suốt thời gian hiển thị
                currentKenBurns = StartCoroutine(RunKenBurns(backImage.rectTransform, step.zoomScale, step.displayTime + 2f)); 
                // Cộng thêm 2s để nó vẫn zoom tiếp lúc đang chuyển cảnh sau
            }

            // 2. Chờ người chơi xem ảnh
            yield return new WaitForSeconds(step.displayTime);

            // 3. Chuẩn bị chuyển sang ảnh tiếp theo (nếu còn)
            if (i < imageSequence.Count - 1)
            {
                StoryPair nextStep = imageSequence[i + 1];

                if (nextStep.isNewScene)
                {
                    // === CHUYỂN CẢNH ĐEN (NEW SCENE) ===
                    yield return StartCoroutine(FadeAlpha(fadeOverlay, 0f, 1f)); // Tối dần
                    
                    // Trong bóng tối, thay ảnh và reset vị trí
                    backImage.sprite = nextStep.image;
                    backImage.rectTransform.localScale = Vector3.one; // Reset zoom về 1
                    frontCG.alpha = 0; 
                    
                    yield return new WaitForSeconds(0.5f); // Nghỉ
                    yield return StartCoroutine(FadeAlpha(fadeOverlay, 1f, 0f)); // Sáng dần
                }
                else
                {
                    // === CROSS-FADE (NỐI TIẾP) ===
                    // Setup ảnh mới lên lớp trên (Front)
                    frontImage.sprite = nextStep.image;
                    frontImage.rectTransform.localScale = Vector3.one; // Reset zoom layer trên
                    frontCG.alpha = 0;

                    // Chạy Ken Burns nhẹ cho cả layer trên lúc nó đang hiện ra (tùy chọn)
                    StartCoroutine(RunKenBurns(frontImage.rectTransform, nextStep.zoomScale, nextStep.displayTime + 2f));

                    // Fade layer trên hiện ra
                    float t = 0f;
                    while (t < transitionSpeed)
                    {
                        t += Time.deltaTime;
                        frontCG.alpha = Mathf.Lerp(0f, 1f, t / transitionSpeed);
                        yield return null;
                    }
                    frontCG.alpha = 1f;

                    // TRÁO ĐỔI LAYER (Swap)
                    // Đưa ảnh từ Front xuống Back để làm nền cho vòng lặp sau
                    backImage.sprite = frontImage.sprite;
                    // Đồng bộ scale của Back cho bằng Front để không bị giật hình
                    backImage.rectTransform.localScale = frontImage.rectTransform.localScale; 
                    
                    frontCG.alpha = 0;

                    // Tiếp tục chạy Ken Burns cho Back (vì giờ nó đang giữ hình ảnh chính)
                    if (currentKenBurns != null) StopCoroutine(currentKenBurns);
                    if (nextStep.useKenBurns)
                    {
                        // Tính toán thời gian còn lại cần zoom
                        currentKenBurns = StartCoroutine(RunKenBurns(backImage.rectTransform, nextStep.zoomScale, nextStep.displayTime));
                    }
                }
            }
        }

        // ============ PHASE 3: KẾT THÚC ============
        yield return StartCoroutine(FadeAlpha(fadeOverlay, 0f, 1f));
        yield return new WaitForSeconds(1f); 
        SceneManager.LoadScene(nextSceneName);
    }

    // Hàm phụ trợ: Fade CanvasGroup
    IEnumerator FadeAlpha(CanvasGroup cg, float start, float end)
    {
        float t = 0f;
        while (t < transitionSpeed)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / transitionSpeed);
            yield return null;
        }
        cg.alpha = end;
    }

    // Hàm phụ trợ: Hiệu ứng Ken Burns (Zoom từ từ)
    IEnumerator RunKenBurns(RectTransform target, float targetScale, float duration)
    {
        float t = 0f;
        Vector3 startSize = target.localScale;
        Vector3 endSize = new Vector3(targetScale, targetScale, 1f);

        while (t < duration)
        {
            t += Time.deltaTime;
            // Dùng SmoothStep cho chuyển động mượt mà hơn Lerp thường
            float progress = t / duration;
            target.localScale = Vector3.Lerp(startSize, endSize, progress);
            yield return null;
        }
    }
}