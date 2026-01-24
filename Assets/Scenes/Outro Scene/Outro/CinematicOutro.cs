using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Tự động thêm 1 AudioSource (chúng ta sẽ dùng cái này làm BGM)
[RequireComponent(typeof(AudioSource))]
public class CinematicOutro : MonoBehaviour
{
    [Header("=== 1. CẤU HÌNH CHUNG ===")]
    public CanvasGroup fadeOverlay;      
    public float transitionSpeed = 1.0f; 
    public string menuSceneName = "MainMenu"; 

    [Header("=== 2. CẤU HÌNH AUDIO (BGM) ===")]
    public AudioClip outroMusic;       
    [Range(0f, 1f)] public float maxBgmVolume = 0.6f;
    public float audioFadeDuration = 3f; 

    [System.Serializable]
    public class OutroSlide
    {
        public string note;            // Ghi chú
        public Sprite image;           // Ảnh
        public float displayTime = 5f; // Thời gian hiện
        
        [Header("Tùy chọn")]
        [Tooltip("Chuyển cảnh ngay lập tức (không fade)")]
        public bool instantTransition = false; 

        // --- MỚI: CẤU HÌNH SFX ---
        [Tooltip("Âm thanh sẽ phát khi ảnh này hiện ra")]
        public AudioClip soundEffect;  
        [Range(0f, 1f)] public float sfxVolume = 1.0f; // Chỉnh to nhỏ cho âm thanh đó
    }

    [Header("=== 3. DANH SÁCH ẢNH ===")]
    public List<OutroSlide> outroSequence;

    [Header("=== 4. UI REFERENCES ===")]
    public Image backImage;
    public Image frontImage;
    public CanvasGroup frontCG;

    // Biến riêng để quản lý 2 luồng âm thanh
    private AudioSource bgmSource; // Nhạc nền
    private AudioSource sfxSource; // Hiệu ứng (tạo thêm bằng code)

    void Start()
    {
        // 1. Setup BGM Source (Lấy cái có sẵn trên object)
        bgmSource = GetComponent<AudioSource>();

        // 2. Setup SFX Source (Tạo mới 1 cái nữa để không đụng hàng)
        sfxSource = gameObject.AddComponent<AudioSource>();
        
        // Setup UI
        if (fadeOverlay != null) fadeOverlay.alpha = 1f; 
        backImage.color = Color.white;
        frontImage.color = Color.white;
        frontCG.alpha = 0;

        StartCoroutine(PlayOutro());
    }

    IEnumerator PlayOutro()
    {
        // --- BẮT ĐẦU NHẠC NỀN ---
        if (outroMusic != null)
        {
            bgmSource.clip = outroMusic;
            bgmSource.volume = 0f;
            bgmSource.loop = false; 
            bgmSource.Play();
            StartCoroutine(FadeMusic(maxBgmVolume, audioFadeDuration));
        }

        // --- XỬ LÝ ẢNH ĐẦU TIÊN ---
        if (outroSequence.Count > 0)
        {
            OutroSlide firstSlide = outroSequence[0];
            backImage.sprite = firstSlide.image;

            // Mở màn
            yield return StartCoroutine(FadeCanvasGroup(fadeOverlay, 1f, 0f));

            // PHÁT SFX CHO ẢNH ĐẦU TIÊN (NẾU CÓ)
            PlaySFX(firstSlide);

            // Chờ
            yield return new WaitForSeconds(firstSlide.displayTime);
        }

        // --- VÒNG LẶP CÁC ẢNH TIẾP THEO ---
        for (int i = 1; i < outroSequence.Count; i++)
        {
            OutroSlide slide = outroSequence[i];

            // 1. PHÁT SFX NGAY KHI BẮT ĐẦU CHUYỂN CẢNH
            PlaySFX(slide);

            // 2. XỬ LÝ HÌNH ẢNH
            if (slide.instantTransition)
            {
                // Instant Cut
                backImage.sprite = slide.image;
                frontCG.alpha = 0; 
            }
            else
            {
                // Cross-Fade
                frontImage.sprite = slide.image;
                frontCG.alpha = 0;

                float t = 0f;
                while (t < transitionSpeed)
                {
                    t += Time.deltaTime;
                    frontCG.alpha = Mathf.Lerp(0f, 1f, t / transitionSpeed);
                    yield return null;
                }
                frontCG.alpha = 1f;

                // Swap
                backImage.sprite = frontImage.sprite;
                frontCG.alpha = 0;
            }

            yield return new WaitForSeconds(slide.displayTime);
        }

        // --- KẾT THÚC ---
        StartCoroutine(FadeMusic(0f, 2f)); // Tắt nhạc nền
        yield return StartCoroutine(FadeCanvasGroup(fadeOverlay, 0f, 1f)); // Tắt màn hình
        yield return new WaitForSeconds(2f); 
        SceneManager.LoadScene(menuSceneName);
    }

    // --- HÀM PHÁT SFX RIÊNG ---
    void PlaySFX(OutroSlide slide)
    {
        if (slide.soundEffect != null)
        {
            // PlayOneShot cho phép phát chồng âm thanh (nếu ảnh chuyển nhanh)
            sfxSource.PlayOneShot(slide.soundEffect, slide.sfxVolume);
        }
    }

    // --- CÁC HÀM FADE ---
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end)
    {
        float t = 0f;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / 1.5f);
            yield return null;
        }
        cg.alpha = end;
    }

    IEnumerator FadeMusic(float targetVol, float duration)
    {
        float startVol = bgmSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, targetVol, t / duration);
            yield return null;
        }
        bgmSource.volume = targetVol;
    }
}