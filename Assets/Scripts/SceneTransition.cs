using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    private Image _fadeImage;
    private float _fadeDuration = 1.0f;

    private void Awake()
    {
        // 1. Singleton: Stays alive from Scene 1 all the way to Boss_Scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreatePersistentUI();
        }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        // Triggers automatically whenever any level (Map 2, Map 3, Boss_Scene) starts
        SceneManager.sceneLoaded += OnLevelLoadFadeIn;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelLoadFadeIn;
    }

    private void OnLevelLoadFadeIn(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        StartCoroutine(ExecuteFadeIn());
    }

    // Called by your WaveManager: SceneTransitionManager.Instance.ToBlack("Map 3");
    public void ToBlack(string sceneName)
    {
        StopAllCoroutines();
        StartCoroutine(ExecuteFadeOut(sceneName));
    }

    private void CreatePersistentUI()
    {
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(this.transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767; // Stays on top of everything

        canvasObj.AddComponent<CanvasScaler>();

        GameObject imageObj = new GameObject("FadeOverlay");
        imageObj.transform.SetParent(canvasObj.transform);
        _fadeImage = imageObj.AddComponent<Image>();
        _fadeImage.color = new Color(0, 0, 0, 1); // Start Black
        _fadeImage.raycastTarget = false;

        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.one;
    }

    private IEnumerator ExecuteFadeIn()
    {
        Time.timeScale = 0; // Freeze at the start of Map 2, Map 3, etc.
        if (_fadeImage != null) _fadeImage.color = new Color(0, 0, 0, 1);
        
        yield return new WaitForSecondsRealtime(0.1f); // Buffer for rendering
        yield return StartCoroutine(LerpAlpha(1, 0));
        
        Time.timeScale = 1; // Unfreeze once clear
    }

    private IEnumerator ExecuteFadeOut(string sceneName)
    {
        Time.timeScale = 1; // Ensure logic moves to process loading
        yield return StartCoroutine(LerpAlpha(0, 1));
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator LerpAlpha(float start, float end)
    {
        float timer = 0f;
        while (timer < _fadeDuration)
        {
            timer += Time.unscaledDeltaTime; // Critical: works while timeScale is 0
            if (_fadeImage != null)
                _fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(start, end, timer / _fadeDuration));
            yield return null;
        }
        if (_fadeImage != null) _fadeImage.color = new Color(0, 0, 0, end);
    }
}