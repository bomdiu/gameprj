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
    public string deathAnimationState = "Player_Death"; // Exact name of your death state in Animator
    public float slowdownDuration = 1.2f;              // Time to slow from 1 to 0 speed
    public float postDeathWaitTime = 1.0f;             // How long to stay on the dead body before fading

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
        // Gradually decrease game speed to zero
        float slowTimer = 0;
        float initialTimeScale = Time.timeScale;
        while (slowTimer < slowdownDuration)
        {
            slowTimer += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(initialTimeScale, 0f, slowTimer / slowdownDuration);
            yield return null;
        }
        Time.timeScale = 0f; // Game is now fully frozen

        // --- PHASE 2: PLAYER DEATH ANIMATION ---
        // We play the animation while time is 0, so the Animator MUST use UnscaledTime
        Animator anim = player.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime; // Force it to ignore Time.timeScale
            anim.Play(deathAnimationState);
        }

        // --- PHASE 3: PARTICLES & WAIT ---
        // Trigger the death particles
        if (deathParticles != null)
        {
            deathParticles.transform.position = player.position;
            // Ensure particles ignore the paused clock
            var main = deathParticles.main;
            main.useUnscaledTime = true; 
            deathParticles.Play();
        }

        // Wait for the animation and particles to finish (using Realtime/Unscaled)
        yield return new WaitForSecondsRealtime(postDeathWaitTime);

        // --- PHASE 4: UI FADE IN ---
        gameOverCanvas.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 0);
        contentGroup.alpha = 0;

        // Start BGM Fade and Play Music
        AudioSource currentBGM = Camera.main.GetComponent<AudioSource>();
        StartCoroutine(FadeOutBGM(currentBGM));
        if (gameOverSource != null && gameOverMusic != null)
        {
            gameOverSource.clip = gameOverMusic;
            gameOverSource.Play();
        }

        // Hard freeze all world objects (Exactly like Upgrade Manager)
        FreezeWorldState();

        // Fade in the black screen
        float fadeTimer = 0;
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0, 1, fadeTimer / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Show buttons
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
        // Freeze Physics
        Rigidbody2D[] allRbs = Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        foreach (Rigidbody2D rb in allRbs) { rb.simulated = false; }

        // Freeze world particles (except death effect)
        ParticleSystem[] allParticles = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        foreach (ParticleSystem ps in allParticles)
        {
            if (ps != deathParticles) ps.Pause(true);
        }

        // Freeze all other animators (enemies, etc.)
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

    public void RetryGame()
    {
        Time.timeScale = 1f;
        IsGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        IsGameOver = false;
        SceneManager.LoadScene(0);
    }
}