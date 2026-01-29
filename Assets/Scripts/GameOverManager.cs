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

    [Header("Cinematic Settings")]
    public string deathAnimationState = "Player_Death"; // Tên state animation chết trong Animator
    public float slowdownDuration = 1.2f;              // Thời gian giảm tốc độ game về 0
    public float postDeathWaitTime = 1.0f;             // Thời gian chờ sau khi chết trước khi hiện màn hình đen

    [Header("Audio Settings")]
    public AudioSource gameOverSource; 
    public AudioClip gameOverMusic;    
    public float fadeDuration = 1.5f;  

    public static GameOverManager Instance;
    public static bool IsGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        IsGameOver = false;
    }

    public void TriggerDeath(Transform playerTransform)
    {
        if (IsGameOver) return;
        StartCoroutine(DeathSequence(playerTransform));
    }

    IEnumerator DeathSequence(Transform player)
    {
        IsGameOver = true;

        // --- PHASE 1: SLOW MOTION ---
        // Giảm dần tốc độ game về 0
        float slowTimer = 0;
        float initialTimeScale = Time.timeScale;
        while (slowTimer < slowdownDuration)
        {
            slowTimer += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(initialTimeScale, 0f, slowTimer / slowdownDuration);
            yield return null;
        }
        Time.timeScale = 0f; // Game đóng băng hoàn toàn

        // --- PHASE 2: PLAYER DEATH ANIMATION ---
        // Chơi animation chết bằng thời gian thực (Unscaled Time)
        Animator anim = player.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime; // Bắt buộc Animator chạy bất chấp TimeScale = 0
            anim.Play(deathAnimationState);
        }

        // --- PHASE 3: PARTICLES & WAIT ---
        // Kích hoạt hiệu ứng tan biến
        if (deathParticles != null)
        {
            deathParticles.transform.position = player.position;
            // Đảm bảo particle chạy theo thời gian thực
            var main = deathParticles.main;
            main.useUnscaledTime = true; 
            deathParticles.Play();
        }

        // Chờ animation và particle diễn ra xong
        yield return new WaitForSecondsRealtime(postDeathWaitTime);

        // --- PHASE 4: UI FADE IN ---
        gameOverCanvas.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 0);
        contentGroup.alpha = 0;

        // Tắt nhạc nền cũ và bật nhạc Game Over
        AudioSource currentBGM = Camera.main.GetComponent<AudioSource>();
        StartCoroutine(FadeOutBGM(currentBGM));
        if (gameOverSource != null && gameOverMusic != null)
        {
            gameOverSource.clip = gameOverMusic;
            gameOverSource.Play();
        }

        // Đóng băng vật lý và các object khác để đảm bảo không có gì di chuyển
        FreezeWorldState();

        // Làm tối màn hình dần dần
        float fadeTimer = 0;
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0, 1, fadeTimer / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Hiện nút bấm và chữ
        float popupTimer = 0;
        while (popupTimer < 0.5f)
        {
            popupTimer += Time.unscaledDeltaTime;
            contentGroup.alpha = Mathf.Lerp(0, 1, popupTimer / 0.5f);
            yield return null;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void FreezeWorldState()
    {
        // Đóng băng Vật lý
        Rigidbody2D[] allRbs = Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        foreach (Rigidbody2D rb in allRbs) { rb.simulated = false; }

        // Đóng băng Particle môi trường (trừ hiệu ứng chết)
        ParticleSystem[] allParticles = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        foreach (ParticleSystem ps in allParticles)
        {
            if (ps != deathParticles) ps.Pause(true);
        }

        // Đóng băng các Animator khác (quái vật, v.v.)
        Animator[] allAnims = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (Animator a in allAnims) { a.speed = 0f; }
    }

    IEnumerator FadeOutBGM(AudioSource bgm)
    {
        if (bgm == null) yield break;
        float startVolume = bgm.volume;
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            bgm.volume = Mathf.Lerp(startVolume, 0, timer / fadeDuration);
            yield return null;
        }
        bgm.Stop();
        bgm.volume = startVolume;
    }

    // --- ĐÃ SỬA ĐỔI PHẦN NÀY ---
    public void RetryGame()
    {
        Time.timeScale = 1f; // Trả lại tốc độ game bình thường
        IsGameOver = false;
        
        // Thay vì load scene hiện tại, ta load cứng Scene index 2 (Gameplay Scene)
        SceneManager.LoadScene(2); 
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        IsGameOver = false;
        SceneManager.LoadScene(0); // Về Main Menu (Index 0)
    }
}