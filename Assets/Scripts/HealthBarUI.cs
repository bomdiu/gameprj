using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Image healthFillImage; 
    public Text hpText;           

    private float _currentHealth;
    private float _maxHealth;

    // Hàm đặt Max HP (Dùng cho PlayerStats và PlayerHealth)
    public void SetMaxHealth(float max)
    {
        _maxHealth = max;
        UpdateHealthDisplay();
    }

    // Hàm đặt Health hiện tại (Dùng cho PlayerStats và PlayerHealth)
    public void SetHealth(float current)
    {
        _currentHealth = current;
        UpdateHealthDisplay();
    }

    // Hàm bổ trợ UpdateHealthBar để xóa lỗi CS1061 của bạn
    public void UpdateHealthBar()
    {
        UpdateHealthDisplay();
    }

    private void UpdateHealthDisplay()
    {
        if (_maxHealth <= 0) _maxHealth = 1;

        if (healthFillImage != null)
        {
            // Image Type phải là Filled mới chạy được dòng này
            healthFillImage.fillAmount = _currentHealth / _maxHealth;
        }

        if (hpText != null)
        {
            float displayHP = Mathf.Max(0, _currentHealth); // Không hiện số âm
            hpText.text = "HP " + Mathf.RoundToInt(displayHP) + " / " + Mathf.RoundToInt(_maxHealth);
        }
    }
}