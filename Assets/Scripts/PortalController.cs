using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for UI handling
using System.Collections; // Required for Coroutines

public class PortalController : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("Drag a black, full-screen UI Image here.")]
    public Image fadeImage; 
    public float fadeDuration = 1.0f;

    private bool isTriggered = false; // Ensures the portal only activates once

    private void Start()
    {
        // Ensure the fade image starts invisible and deactivated so it doesn't block the view
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered) return; // Stop double triggers

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered portal. Starting fade sequence...");
            StartCoroutine(FadeAndLoadSequence());
        }
    }

    private IEnumerator FadeAndLoadSequence()
    {
        isTriggered = true;

        // 1. Freeze the game (Optional: stops the player running around during fade)
        Time.timeScale = 0f;

        // 2. Start the fade
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float timer = 0;
            while (timer < fadeDuration)
            {
                // Use unscaledDeltaTime because we set timeScale to 0
                timer += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("Portal: No Fade Image assigned. Doing instant transition.");
            yield return new WaitForSecondsRealtime(0.5f); // Small delay if no image
        }

        // 3. Load the next level
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        // CRITICAL: Reset time scale before loading, or the next scene starts frozen!
        Time.timeScale = 1f;

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("<color=yellow>No more levels in Build Settings! Returning to Menu (Index 0).</color>");
            SceneManager.LoadScene(0); // Fallback to main menu
        }
    }
}