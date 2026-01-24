using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; 

public class CinematicCredits : MonoBehaviour
{
    [Header("Settings")]
    public float scrollSpeed = 60f; 
    public float endYPosition = 3000f; 

    [Header("References")]
    public RectTransform containerRect;
    public CanvasGroup contentAlpha;

    [Header("Audio")]
    public AudioSource bgmAudioSource; // Kéo Audio Source vào đây

    void Start()
    {
        StartCoroutine(PlayCreditsRoutine());
    }

    IEnumerator PlayCreditsRoutine()
    {
        // 1. Chạy chữ
        while (containerRect.anchoredPosition.y < endYPosition)
        {
            containerRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            yield return null; 
        }

        // 2. KẾT THÚC: Fade Out cả Hình ảnh lẫn Âm thanh
        Debug.Log("Credit finished. Fading out...");
        float fadeDuration = 3f; //
        float timer = 0;
        
        float startAlpha = contentAlpha.alpha;
        float startVolume = 0;

        // Lấy volume hiện tại của nhạc (đề phòng bạn set volume ban đầu khác 1)
        if (bgmAudioSource != null) startVolume = bgmAudioSource.volume;

        while(timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Làm mờ hình
            contentAlpha.alpha = Mathf.Lerp(startAlpha, 0, progress);

            // Làm nhỏ nhạc (Nếu có gán Audio Source)
            if (bgmAudioSource != null)
            {
                bgmAudioSource.volume = Mathf.Lerp(startVolume, 0, progress);
            }

            yield return null;
        }
        
        // 3. Chuyển Scene
        // SceneManager.LoadScene("MainMenu");
    }
}