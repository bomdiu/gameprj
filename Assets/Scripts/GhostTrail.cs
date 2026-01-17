using UnityEngine;

public class DashGhost : MonoBehaviour
{
    private SpriteRenderer sr;
    private float fadeTimer;
    private float fadeDuration;
    private Color startColor;

    public void Init(Sprite sprite, Vector3 scale, Color ghostColor, float duration, Material silhouetteMat = null)
    {
        sr = GetComponent<SpriteRenderer>();
        
        // Use a silhouette material if provided, otherwise standard
        if (silhouetteMat != null) sr.material = silhouetteMat;

        sr.sprite = sprite;
        transform.localScale = scale;
        
        startColor = ghostColor;
        sr.color = startColor;
        
        fadeDuration = duration;
        fadeTimer = duration;
    }

    private void Update()
    {
        fadeTimer -= Time.deltaTime;
        
        // Fades the alpha from the ghostColor's alpha down to 0
        float alpha = Mathf.Lerp(0, startColor.a, fadeTimer / fadeDuration);
        sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (fadeTimer <= 0) Destroy(gameObject);
    }
}