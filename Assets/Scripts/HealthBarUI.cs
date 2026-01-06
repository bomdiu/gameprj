using UnityEngine;
using UnityEngine.UI; // Cần thiết cho Image và Text

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    // Tham chiếu đến Image (phần màu xanh lá) đã được thiết lập Image Type = Filled
    public Image healthFillImage;
    
    // Tham chiếu đến Text hiển thị số HP
    public Text hpText;

    // Biến lưu trữ nội bộ để vẽ UI
    private float _currentHealth;
    private float _maxHealth;

    // --- CÁC HÀM NÀY SẼ ĐƯỢC GỌI TỪ PLAYERSTATS ---

    // 1. Hàm thiết lập Max HP (Gọi khi Start game hoặc khi Nâng cấp)
    public void SetMaxHealth(float maxHealth)
    {
        _maxHealth = maxHealth;
        UpdateHealthBar(); // Vẽ lại ngay
    }

    // 2. Hàm thiết lập Máu hiện tại (Gọi khi Start, khi bị đánh, hồi máu, hoặc nâng cấp)
    public void SetHealth(float currentHealth)
    {
        _currentHealth = currentHealth;
        UpdateHealthBar(); // Vẽ lại ngay
    }

    // Hàm nội bộ để cập nhật hình ảnh và chữ
    public void UpdateHealthBar()
    {
        // Kiểm tra an toàn để tránh lỗi chia cho 0
        if (_maxHealth <= 0) _maxHealth = 1;

        // 1. Cập nhật thanh Fill (độ dài)
        if (healthFillImage != null)
        {
            float fillAmount = _currentHealth / _maxHealth;
            healthFillImage.fillAmount = fillAmount;
        }

        // 2. Cập nhật Text (số)
        if (hpText != null)
        {
            hpText.text = "HP " + _currentHealth.ToString() + "/" + _maxHealth.ToString();
        }
    }
}