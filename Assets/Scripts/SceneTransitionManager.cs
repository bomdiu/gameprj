using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Settings")]
    public float fadeDuration = 1.0f;
    private Image fadeImage;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        CreateFadeOverlay();
    }

    private void Start()
    {
        // Always fade in at the start of any scene this is in
        StartCoroutine(Fade(1, 0));
    }

    private void CreateFadeOverlay()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("TransitionCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject fadeObj = new GameObject("FadeOverlay");
        fadeObj.transform.SetParent(canvas.transform, false);
        fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.raycastTarget = false;

        RectTransform rect = fadeObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }

    public void ToBlack(string sceneName = "")
    {
        StartCoroutine(Fade(0, 1, sceneName));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, string sceneName = "")
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (endAlpha <= 0) fadeImage.gameObject.SetActive(false);
        
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}