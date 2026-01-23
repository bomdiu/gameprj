using UnityEngine;
using System.Collections;

public class VampireIconVFX : MonoBehaviour
{
    private SpriteRenderer sprite;

    [Header("Position & Opacity")]
    [Tooltip("Start position relative to the player center. Y=1.5 is usually good head height.")]
    public Vector3 spawnOffset = new Vector3(0, 1.5f, 0); 

    [Tooltip("Maximum opacity the icon reaches. 0.6 = 60% visible.")]
    [Range(0f, 1f)] public float maxAlpha = 0.6f;

    [Header("Animation Timers & Speed")]
    public float fadeTime = 0.3f;
    public float stayTime = 0.5f;
    public float speed = 1.2f;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            // 1. Apply initial offset relative to parent (player)
            transform.localPosition = spawnOffset;
            
            // 2. Start completely invisible
            SetAlpha(0); 
        }
    }

    IEnumerator Start()
    {
        // --- PHASE 1: FADE IN (0% -> 60%) ---
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            // Calculate percentage of fade completion (0 to 1)
            float t = elapsed / fadeTime;
            // Interpolate alpha from 0 up to maxAlpha (0.6)
            SetAlpha(Mathf.Lerp(0f, maxAlpha, t));
            
            // Move up slightly during fade in
            transform.Translate(Vector3.up * speed * Time.deltaTime);
            yield return null;
        }
        // Ensure it ends exactly at maxAlpha
        SetAlpha(maxAlpha);


        // --- PHASE 2: STAY PUT ---
        yield return new WaitForSeconds(stayTime);


        // --- PHASE 3: FADE OUT (60% -> 0%) ---
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            // Interpolate alpha from maxAlpha (0.6) down to 0
            SetAlpha(Mathf.Lerp(maxAlpha, 0f, t));

            // Continue moving up while fading out
            transform.Translate(Vector3.up * speed * Time.deltaTime);
            yield return null;
        }
        // Ensure it ends exactly at 0
        SetAlpha(0f);


        // --- CLEANUP ---
        Destroy(gameObject); 
    }

    // Helper method to change sprite opacity
    void SetAlpha(float a) 
    { 
        if(sprite) 
        { 
            Color c = sprite.color; 
            c.a = a; // Only change alpha value
            sprite.color = c; 
        } 
    }
}