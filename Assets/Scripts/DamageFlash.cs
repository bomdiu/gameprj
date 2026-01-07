using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f; 

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        // Tìm SpriteRenderer ở cả cha và các object con
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Flash()
    {
        if (spriteRenderer == null) return;

        Debug.Log("Flash đã được kích hoạt!"); // Dòng này sẽ hiện trong Console
        StopAllCoroutines(); 
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}