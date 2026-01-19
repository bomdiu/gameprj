using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Chỉ số")]
    public float maxHealth = 1000f;
    
    [Header("Debug Info")]
    public float currentHealth;

    [Header("Kết nối")]
    public DamageFlash damageFlash; // Kéo script DamageFlash vào
    public BossHealthUI healthUI;   // Kéo script BossHealthUI vào

    // Logic Phase giữ nguyên
    public bool IsPhase2 
    {
        get 
        {
            if (currentHealth <= 0) return false;
            return currentHealth <= (maxHealth * 0.5f);
        }
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        
        // Reset UI về đầy cây lúc đầu
        if (healthUI != null) healthUI.UpdateHealth(currentHealth, maxHealth);
    }

    // --- HÀM NHẬN SÁT THƯƠNG ---
    // Player sẽ gọi hàm này: boss.GetComponent<BossHealth>().TakeDamage(10);
    public void TakeDamage(float damage)
    {
        // 1. Trừ máu
        currentHealth -= damage;

        // 2. Kích hoạt hiệu ứng nháy trắng
        if (damageFlash != null) damageFlash.Flash();

        // 3. Cập nhật thanh máu UI
        if (healthUI != null) healthUI.UpdateHealth(currentHealth, maxHealth);

        // 4. Kiểm tra chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Update()
    {
        // Bấm phím T để tự gây damage cho Boss
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(50);
        }
    }

    void Die()
    {
        Debug.Log("BOSS DEAD!");
        // Tắt boss, chạy animation chết, rớt đồ, v.v...
        // Tạm thời destroy
        Destroy(gameObject); 
        
        // Lưu ý: Nếu Destroy ngay lập tức thì Flash chưa kịp tắt. 
        // Sau này nên dùng Coroutine DieRoutine để chờ anim chết xong mới Destroy.
    }
}