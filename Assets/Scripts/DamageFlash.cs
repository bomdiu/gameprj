using UnityEngine;
using System.Collections; // Cần thiết cho Coroutine

public class DamageFlash : MonoBehaviour
{
    // Màu sẽ nhấp nháy
    public Color flashColor = Color.red;
    // Thời gian flash
    public float flashDuration = 0.1f; 

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        // Lấy SpriteRenderer, component hiển thị hình ảnh
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Lưu lại màu gốc của sprite
            originalColor = spriteRenderer.color;
        }
    }

    // Hàm public được gọi khi nhận sát thương
    public void Flash()
    {
        if (spriteRenderer == null) return;

        Debug.Log("Flash đã được kích hoạt!");
        
        // Dừng Coroutine hiện tại (nếu đang chạy) để tránh xung đột
        StopAllCoroutines(); 
        
        // Bắt đầu nhấp nháy màu
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // 1. Chuyển sang màu flash
        spriteRenderer.color = flashColor;

        // 2. Đợi trong thời gian flashDuration
        yield return new WaitForSeconds(flashDuration);

        // 3. Quay lại màu gốc
        spriteRenderer.color = originalColor;
    }
}