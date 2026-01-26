using UnityEngine;
using System.Collections;

public class BossMusicController : MonoBehaviour
{
    [Header("Cài đặt Nhạc")]
    [Tooltip("Nhạc dạo đầu (Chạy 1 lần rồi thôi). Nếu không có thì để trống.")]
    public AudioClip introClip; 

    [Tooltip("Nhạc nền chính (Sẽ lặp lại mãi mãi). Bắt buộc phải có.")]
    public AudioClip loopClip;

    [Header("Cấu hình")]
    [Range(0f, 1f)] public float volume = 0.5f; // Âm lượng nhạc (đừng để to quá át tiếng SFX)
    
    private AudioSource audioSource;

    private void Awake()
    {
        // Tự tạo AudioSource nếu chưa có
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Setup cơ bản
        audioSource.volume = volume;
        audioSource.playOnAwake = false; // Chúng ta sẽ điều khiển bằng code
    }

    private void Start()
    {
        StartCoroutine(PlayMusicRoutine());
    }

    private IEnumerator PlayMusicRoutine()
    {
        // TRƯỜNG HỢP 1: Có nhạc Intro
        if (introClip != null)
        {
            // 1. Chơi nhạc Intro
            audioSource.loop = false;
            audioSource.clip = introClip;
            audioSource.Play();

            // 2. Chờ cho đến khi nhạc Intro chạy xong
            // (WaitForSeconds bằng đúng độ dài của clip)
            yield return new WaitForSeconds(introClip.length);
        }

        // TRƯỜNG HỢP 2: Chuyển sang nhạc Loop (hoặc chỉ có nhạc Loop)
        if (loopClip != null)
        {
            audioSource.clip = loopClip;
            audioSource.loop = true; // Bật chế độ lặp
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("⚠️ Chưa gắn nhạc Loop vào BossMusicController!");
        }
    }

    // Hàm tiện ích để dừng nhạc khi Boss chết (gọi từ BossHealth)
    public void StopMusic()
    {
        if (audioSource != null)
        {
            // Hiệu ứng Fade out (nhỏ dần) trong 2 giây
            StartCoroutine(FadeOutRoutine(4f));
        }
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Trả lại volume cũ đề phòng
    }
}