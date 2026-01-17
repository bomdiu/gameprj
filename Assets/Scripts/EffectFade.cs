using UnityEngine;
using System.Collections;

public class EffectCleanup : MonoBehaviour
{
    [Header("Fade Settings")]
    public bool fadeOutOnStart = true;
    public float fadeDuration = 0.3f;

    private SpriteRenderer sr;
    private bool isFading = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (fadeOutOnStart)
        {
            StartCoroutine(FadeAndDestroy());
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        isFading = true;
        if (sr != null)
        {
            Color startColor = sr.color;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeDuration);
                
                // We re-apply the color every frame here
                sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        DestroySelf();
    }

    public void DestroySelf()
    {
        // Stop all logic before destroying to prevent errors
        StopAllCoroutines();
        Destroy(gameObject);
    }
}