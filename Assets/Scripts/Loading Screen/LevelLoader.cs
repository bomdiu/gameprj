using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    // Singleton: Allows you to call LevelLoader.instance.LoadLevel(index) from anywhere.
    public static LevelLoader instance;

    [Header("UI References")]
    public GameObject loadingScreenPrefab;

    [Header("Settings")]
    [Tooltip("How fast the bar fills (0.5 = 2 seconds, 1 = 1 second)")]
    public float loadSpeedMultiplier = 0.5f;
    [Tooltip("How fast the screen fades to/from black")]
    public float fadeDuration = 0.5f; 

    private void Awake()
    {
        // Keep this object alive across all scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    private IEnumerator LoadAsynchronously(int sceneIndex)
    {
        // 1. Create the Loading Screen
        GameObject loadingScreen = Instantiate(loadingScreenPrefab);
        DontDestroyOnLoad(loadingScreen);

        CanvasGroup canvasGroup = loadingScreen.GetComponent<CanvasGroup>();
        Slider slider = loadingScreen.GetComponentInChildren<Slider>();
        
        // Ensure it starts completely invisible so we don't 'pop' into view
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (slider != null) slider.value = 0f;

        // --- STEP 2: THE ANTI-FLICKER FADE IN ---
        // We PAUSE here until the screen is 100% solid. 
        // We do NOT start SceneManager until the game is hidden.
        yield return StartCoroutine(Fade(canvasGroup, 0f, 1f));

        // --- STEP 3: START LOADING (ONLY NOW) ---
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        
        // Stops Unity from switching scenes the second it hits 90%
        operation.allowSceneActivation = false;

        float visualProgress = 0f;

        // 4. THE LOADING LOOP
        while (visualProgress < 1f || operation.progress < 0.9f)
        {
            // Move the bar visually based on time
            visualProgress = Mathf.MoveTowards(visualProgress, 1f, Time.deltaTime * loadSpeedMultiplier);
            
            // Unity progress caps at 0.9 until activation
            float actualUnityProgress = Mathf.Clamp01(operation.progress / 0.9f);

            if (slider != null)
            {
                // Slider value is whichever is lower (visual vs reality)
                slider.value = Mathf.Min(visualProgress, actualUnityProgress);
            }

            // Only switch when visual bar is 100% AND Unity is ready
            if (visualProgress >= 1f && operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Wait until Unity confirms the scene has fully swapped
        while (!operation.isDone)
        {
            yield return null;
        }

        // --- THE SETTLING BUFFER ---
        // Give 2D Lighting and Shadow Casters a moment to stabilize hidden behind black
        yield return new WaitForSeconds(0.4f);

        // 5. FADE OUT
        yield return StartCoroutine(Fade(canvasGroup, 1f, 0f));
        
        // Cleanup
        Destroy(loadingScreen);
    }

    private IEnumerator Fade(CanvasGroup cg, float start, float end)
    {
        if (cg == null) yield break;

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, timer / fadeDuration);
            yield return null;
        }
        cg.alpha = end;
    }
}