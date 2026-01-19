using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public Slider healthSlider; // Kéo cái Slider vào đây
    public float updateSpeed = 5f; // Tốc độ trượt của thanh máu (cho mượt)

    private float targetValue = 1f;

    private void Update()
    {
        // Hiệu ứng tụt máu từ từ (Lerp) cho đẹp mắt
        if (healthSlider.value != targetValue)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * updateSpeed);
        }
    }

    // Hàm này sẽ được gọi từ BossHealth
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        // Chuyển đổi sang tỉ lệ 0 - 1
        targetValue = currentHealth / maxHealth;
    }
}