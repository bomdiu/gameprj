using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    [Header("Fade Settings")]
    public float flashDuration = 0.15f; 
    [Range(0, 1)] public float maxFlashAmount = 0.9f; 

    private SpriteRenderer[] spriteRenderers;
    private MaterialPropertyBlock propBlock;

    // --- NEW PROPERTY: This allows other scripts to check the flash status ---
    public bool IsFlashing { get; private set; } 

    void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        propBlock = new MaterialPropertyBlock();
    }

    public void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        IsFlashing = true; // State is now active
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime; 

            float currentAmount = Mathf.Lerp(maxFlashAmount, 0f, elapsed / flashDuration);

            foreach (var sr in spriteRenderers)
            {
                sr.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_FlashAmount", currentAmount);
                sr.SetPropertyBlock(propBlock);
            }

            yield return null;
        }

        SetFlashAmount(0);
        IsFlashing = false; // State is now inactive
    }

    public void FlashIndefinitely() 
    {
        StopAllCoroutines();
        IsFlashing = true; 
        SetFlashAmount(maxFlashAmount); 
    }

    private void SetFlashAmount(float amount)
    {
        foreach (var sr in spriteRenderers)
        {
            sr.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_FlashAmount", amount);
            sr.SetPropertyBlock(propBlock);
        }
    }
}