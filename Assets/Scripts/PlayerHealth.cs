using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100; // Khởi tạo Max Health
    
    // Tham chiếu đến script UI
    public HealthBarUI healthBarUI; 

    private DamageFlash damageFlash;

    void Start()
    {
        currentHealth = maxHealth; 
        
        damageFlash = GetComponent<DamageFlash>(); // Lấy component DamageFlash

        // Cập nhật UI lần đầu
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar();
        }
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        
        // Đảm bảo máu không vượt quá maxHealth và không dưới 0
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Gọi hàm cập nhật UI sau khi máu thay đổi
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(); 
        }

        //Nếu nhận sát thương (số âm) và component có sẵn, thì nhấp nháy
        if (amount < 0 && damageFlash != null)
        {
            damageFlash.Flash();
        }

        if (currentHealth <= 0)
        {
            gameObject.SetActive(false); // Player chết
            // (Nên thêm logic chơi animation chết hoặc chuyển cảnh tại đây)
        }
    }
}