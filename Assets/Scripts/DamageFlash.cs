using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    [Header("Fade Settings")]
    public float flashDuration = 0.15f; // Thời gian dài hơn một chút để thấy rõ fade
    [Range(0, 1)] public float maxFlashAmount = 0.9f; 

    private SpriteRenderer[] spriteRenderers;
    private MaterialPropertyBlock propBlock;

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
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Dùng unscaled để mượt khi hitstop

            // Tính toán độ trắng giảm dần từ 0.9 về 0
            float currentAmount = Mathf.Lerp(maxFlashAmount, 0f, elapsed / flashDuration);

            // Cập nhật giá trị vào Shader mà không cần tạo Material mới (tối ưu hiệu năng)
            foreach (var sr in spriteRenderers)
            {
                sr.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_FlashAmount", currentAmount);
                sr.SetPropertyBlock(propBlock);
            }

            yield return null;
        }

        // Đảm bảo kết thúc trả về 0
        SetFlashAmount(0);
    }
public void FlashIndefinitely() 
{
    StopAllCoroutines(); // Stops the FlashRoutine from fading the color back to normal
    
    // Forces the shader to stay at the maximum flash amount
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