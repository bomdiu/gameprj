using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class CinematicIntro : MonoBehaviour
{
    [Header("=== 1. CẤU HÌNH UI & OVERLAY ===")]
    public CanvasGroup fadeOverlay;      
    public float transitionSpeed = 1.0f; 
    public RawImage noiseOverlay;        
    public TextMeshProUGUI narrativeTextUI; 
    public float textFadeDuration = 0.5f;

    [Header("=== 2. CẤU HÌNH AUDIO ===")]
    public AudioClip bgmClip;          
    [Range(0f, 1f)] public float maxVolume = 0.5f;     
    public float audioFadeDuration = 2f; 
    
    [Tooltip("Số giây im lặng trước khi nhạc bắt đầu chạy")]
    public float musicStartDelay = 0f; 

    private AudioSource audioSource;

    [System.Serializable]
    public class VideoSubtitle
    {
        [TextArea(2, 3)] public string text; 
        public float duration = 3f;          
    }

    [Header("=== 3. CẤU HÌNH VIDEO ===")]
    public VideoClip introVideo;       
    [Range(0.1f, 3.0f)] public float videoSpeed = 1.0f; 
    
    // --- MỚI: DELAY CHO VIDEO ---
    [Tooltip("Số giây màn hình đen trước khi Video bắt đầu hiện")]
    public float videoStartDelay = 0f;

    public RawImage videoDisplay;      
    public VideoPlayer videoPlayer;
    public List<VideoSubtitle> videoSubtitles; 

    [System.Serializable]
    public class StoryStep
    {
        public string note;             
        public Sprite image;
        [TextArea(2, 4)] public string[] dialogues; 
        public float timePerLine = 3f;  
        public bool isNewScene;         
    }

    [Header("=== 4. CẤU HÌNH ẢNH ===")]
    public List<StoryStep> imageSequence; 
    
    [Header("=== 5. UI REFERENCES ===")]
    public Image backImage;  
    public Image frontImage; 
    public CanvasGroup frontCG; 

    [Header("=== 6. ĐIỀU HƯỚNG ===")]
    public string nextSceneName = "GameplayScene"; 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Setup ban đầu
        videoDisplay.gameObject.SetActive(false); 
        backImage.color = Color.white;
        frontImage.color = Color.white;
        frontCG.alpha = 0; 
        
        if (noiseOverlay != null) noiseOverlay.gameObject.SetActive(true);
        // Đảm bảo màn hình đen thui ngay từ đầu
        if (fadeOverlay != null) fadeOverlay.alpha = 1f;
        if (narrativeTextUI != null) narrativeTextUI.alpha = 0f;

        // Chạy song song 2 luồng
        StartCoroutine(PlayVisualSequence());
        StartCoroutine(PlayMusicWithDelay());
    }

    // --- LUỒNG 1: XỬ LÝ NHẠC ---
    IEnumerator PlayMusicWithDelay()
    {
        if (bgmClip == null) yield break;

        if (musicStartDelay > 0)
        {
            yield return new WaitForSeconds(musicStartDelay);
        }

        audioSource.clip = bgmClip;
        audioSource.volume = 0f; 
        audioSource.Play();

        yield return StartCoroutine(FadeMusic(maxVolume, audioFadeDuration));
    }

    // --- LUỒNG 2: XỬ LÝ HÌNH ẢNH (VIDEO + ẢNH) ---
    IEnumerator PlayVisualSequence()
    {
        // ==========================================
        // PHASE 1: VIDEO (CÓ DELAY)
        // ==========================================
        if (introVideo != null)
        {
            // --- MỚI: CHỜ VIDEO START DELAY ---
            // Trong thời gian này màn hình vẫn đen (do fadeOverlay.alpha = 1 từ Start)
            if (videoStartDelay > 0)
            {
                yield return new WaitForSeconds(videoStartDelay);
            }

            videoDisplay.gameObject.SetActive(true);
            videoPlayer.clip = introVideo;
            videoPlayer.playbackSpeed = videoSpeed;
            videoPlayer.Prepare();
            
            // Đợi video load texture
            while (!videoPlayer.isPrepared) yield return null;
            
            videoDisplay.texture = videoPlayer.texture;
            videoPlayer.Play();

            float totalVideoDuration = (float)introVideo.length / videoSpeed;
            float startTime = Time.time; 

            // Fade Mở màn hình (Video hiện ra)
            yield return StartCoroutine(FadeCanvasGroup(fadeOverlay, 1f, 0f));

            // Chạy Subtitles Video
            foreach (VideoSubtitle sub in videoSubtitles)
            {
                if (Time.time - startTime >= totalVideoDuration) break;
                narrativeTextUI.text = sub.text;
                yield return StartCoroutine(FadeTextAlpha(1f, textFadeDuration));

                float wait = sub.duration - (textFadeDuration * 2);
                if (wait < 0.2f) wait = 0.2f;
                yield return new WaitForSeconds(wait);

                yield return StartCoroutine(FadeTextAlpha(0f, textFadeDuration));
            }

            // Chờ hết video
            float elapsedTime = Time.time - startTime;
            float remainingTime = totalVideoDuration - elapsedTime;
            if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

            // Fade Đóng màn hình (Video tối đi)
            yield return StartCoroutine(FadeCanvasGroup(fadeOverlay, 0f, 1f));
            
            videoPlayer.Stop();
            videoDisplay.gameObject.SetActive(false);
        }
        else 
        {
            // Nếu không có video, nhưng người dùng vẫn set delay cho video?
            // Ta coi như đây là thời gian chờ trước khi vào ảnh
            if (videoStartDelay > 0)
            {
                yield return new WaitForSeconds(videoStartDelay);
            }
        }

        // ==========================================
        // PHASE 2: ẢNH TĨNH
        // ==========================================
        if (imageSequence.Count > 0) backImage.sprite = imageSequence[0].image;

        // Fade Mở màn hình (Ảnh hiện ra)
        yield return StartCoroutine(FadeCanvasGroup(fadeOverlay, 1f, 0f));

        if (imageSequence.Count > 0)
        {
            yield return StartCoroutine(HandleDialogueLoop(imageSequence[0]));
        }

        for (int i = 1; i < imageSequence.Count; i++)
        {
            StoryStep step = imageSequence[i];
            if (narrativeTextUI != null) narrativeTextUI.alpha = 0f;

            if (step.isNewScene)
            {
                yield return StartCoroutine(FadeCanvasGroup(fadeOverlay, 0f, 1f));
                backImage.sprite = step.image;
                frontCG.alpha = 0; 
                yield return new WaitForSeconds(0.5f); 
                yield return StartCoroutine(FadeCanvasGroup(fadeOverlay, 1f, 0f));
            }
            else
            {
                frontImage.sprite = step.image;
                frontCG.alpha = 0;
                float t = 0f;
                while (t < transitionSpeed)
                {
                    t += Time.deltaTime;
                    frontCG.alpha = Mathf.Lerp(0f, 1f, t / transitionSpeed);
                    yield return null;
                }
                frontCG.alpha = 1f;
                backImage.sprite = frontImage.sprite;
                frontCG.alpha = 0;
            }

            yield return StartCoroutine(HandleDialogueLoop(step));
        }

        // ==========================================
        // PHASE 3: KẾT THÚC
        // ==========================================
        if (narrativeTextUI != null) StartCoroutine(FadeTextAlpha(0f, 0.5f));
        
        StartCoroutine(FadeMusic(0f, 1.5f)); 

        yield return StartCoroutine(FadeCanvasGroup(fadeOverlay, 0f, 1f));
        
        yield return new WaitForSeconds(1.5f); 
        SceneManager.LoadScene(nextSceneName);
    }

    // --- CÁC HÀM PHỤ TRỢ GIỮ NGUYÊN ---
    IEnumerator FadeMusic(float targetVol, float duration)
    {
        if (audioSource == null) yield break;
        float startVol = audioSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, targetVol, t / duration);
            yield return null;
        }
        audioSource.volume = targetVol;
    }

    IEnumerator FadeTextAlpha(float targetAlpha, float duration)
    {
        if (narrativeTextUI == null) yield break;
        float startAlpha = narrativeTextUI.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            narrativeTextUI.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            yield return null;
        }
        narrativeTextUI.alpha = targetAlpha;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end)
    {
        if (cg == null) yield break;
        float t = 0f;
        while (t < transitionSpeed)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / transitionSpeed);
            yield return null;
        }
        cg.alpha = end;
    }

    IEnumerator HandleDialogueLoop(StoryStep step)
    {
        if (step.dialogues == null || step.dialogues.Length == 0)
        {
            yield return new WaitForSeconds(step.timePerLine);
            yield break;
        }
        foreach (string line in step.dialogues)
        {
            if (string.IsNullOrEmpty(line)) continue;
            narrativeTextUI.text = line;
            yield return StartCoroutine(FadeTextAlpha(1f, textFadeDuration));
            float waitTime = step.timePerLine - (textFadeDuration * 2);
            if (waitTime < 0.5f) waitTime = 0.5f; 
            yield return new WaitForSeconds(waitTime);
            yield return StartCoroutine(FadeTextAlpha(0f, textFadeDuration));
            yield return new WaitForSeconds(0.2f);
        }
    }
}