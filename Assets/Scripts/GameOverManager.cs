using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverCanvas;
    public Image blackScreen;
    public CanvasGroup contentGroup;

    [Header("Effects")]
    public ParticleSystem deathParticles;

    [Header("Audio Settings")]
    public AudioSource gameOverSource; // Kéo AudioSource của Canvas vào đây
    public AudioClip gameOverMusic;    // Kéo file nhạc buồn vào đây
    public float fadeDuration = 1.5f;  // Thời gian chuyển đổi âm thanh

    public static GameOverManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        gameOverCanvas.SetActive(false);
    }

    public void TriggerDeath(Transform playerTransform)
    {
        StartCoroutine(DeathSequence(playerTransform));
    }

    IEnumerator DeathSequence(Transform player)
    {
        // 1. Setup ban đầu
        gameOverCanvas.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 0);
        contentGroup.alpha = 0;

        // 2. XỬ LÝ ÂM THANH (Mới) -------------
        // Tìm nhạc nền game đang phát (thường gắn ở Camera chính) và tắt dần
        AudioSource currentBGM = Camera.main.GetComponent<AudioSource>();
        StartCoroutine(FadeOutBGM(currentBGM));

        // Bật nhạc Game Over
        if (gameOverSource != null && gameOverMusic != null)
        {
            gameOverSource.clip = gameOverMusic;
            gameOverSource.Play();
            // Optional: Nếu muốn fade in nhạc game over thì viết thêm coroutine, 
            // nhưng thường nhạc game over nên vang lên ngay để tạo cảm xúc.
        }
        // -------------------------------------

        // 3. FREEZE GAME
        Time.timeScale = 0f;

        // 4. Hiệu ứng tan biến
        deathParticles.transform.position = player.position;
        deathParticles.Play();

        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null) playerSprite.enabled = false;

        // 5. Màn hình tối dần
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 6. Hiện Pop-up
        timer = 0;
        float popupDuration = 1.0f;
        while (timer < popupDuration)
        {
            timer += Time.unscaledDeltaTime;
            contentGroup.alpha = Mathf.Lerp(0, 1, timer / popupDuration);
            yield return null;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Hàm phụ để tắt dần nhạc nền cũ cho êm tai
    IEnumerator FadeOutBGM(AudioSource bgm)
    {
        if (bgm == null) yield break;

        float startVolume = bgm.volume;
        float timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            // Giảm volume từ mức hiện tại về 0
            bgm.volume = Mathf.Lerp(startVolume, 0, timer / fadeDuration);
            yield return null;
        }

        bgm.Stop();
        bgm.volume = startVolume; // Trả lại volume cũ để lần sau chơi lại ko bị mất tiếng
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Load lại màn hiện tại
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}