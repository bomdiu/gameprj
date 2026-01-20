using UnityEngine;
using UnityEngine.UI; // Cần thư viện này

public class BossHealthUI : MonoBehaviour
{
    // Đổi từ Slider sang Image
    public Image healthBarFill; 
    public float updateSpeed = 5f;

    private float targetFillAmount = 1f;

    private void Update()
    {
        // Hiệu ứng tụt máu từ từ
        if (healthBarFill.fillAmount != targetFillAmount)
        {
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * updateSpeed);
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        // Tính tỉ lệ 0 - 1
        targetFillAmount = currentHealth / maxHealth;
    }
}