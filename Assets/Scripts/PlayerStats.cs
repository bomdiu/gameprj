using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float moveSpeed = 5f;
    public float attackDamage = 10f;

    // --- THÊM: Tham chiếu đến script di chuyển ---
    // (Bạn hãy thay 'PlayerMovement' bằng tên chính xác script di chuyển của bạn)
    public PlayerMovement movementScript; 
    
    // --- THÊM: Tham chiếu đến script thanh máu (nếu có) ---
    public HealthBarUI healthBarScript; 

    private void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Tự động tìm script di chuyển trên cùng nhân vật (nếu quên kéo thả)
        if (movementScript == null) 
            movementScript = GetComponent<PlayerMovement>();
            
        // Cập nhật tốc độ ban đầu cho script di chuyển luôn cho chắc
        if (movementScript != null)
             movementScript.moveSpeed = moveSpeed; // Thay 'moveSpeed' bằng tên biến trong script di chuyển của bạn
        
        if (healthBarScript != null)
        {
            healthBarScript.SetMaxHealth(maxHealth);
            healthBarScript.SetHealth(currentHealth);
        }
        // Khởi tạo hiển thị thanh máu lúc đầu game
        if (healthBarScript != null)
        {
            healthBarScript.SetMaxHealth(maxHealth);
            healthBarScript.SetHealth(currentHealth);
        }
    }

    public void ApplyUpgrade(SkillData.SkillType type, float amount)
    {
        switch (type)
        {
            case SkillData.SkillType.AttackUp:
                attackDamage += amount;
                break;

            case SkillData.SkillType.HealthUp:
                maxHealth += amount;
                currentHealth += amount;
                
                // --- CẬP NHẬT UI THANH MÁU ---
                if (healthBarScript != null)
                {
                    healthBarScript.SetMaxHealth(maxHealth); // Cần viết hàm này bên script HealthBar
                    healthBarScript.SetHealth(currentHealth);
                }
                break;

            case SkillData.SkillType.SpeedUp:
                moveSpeed += amount;
                
                // --- CẬP NHẬT TỐC ĐỘ DI CHUYỂN NGAY LẬP TỨC ---
                if (movementScript != null)
                {
                    // Giả sử bên script PlayerMovement biến tốc độ tên là 'moveSpeed'
                    // Nếu nó tên là 'speed', hãy sửa thành movementScript.speed = moveSpeed;
                    movementScript.moveSpeed = moveSpeed; 
                }
                break;
        }
        Debug.Log($"Nâng cấp {type} thành công! Chỉ số mới: HP={maxHealth}, Speed={moveSpeed}");
    }
    
    public void TakeDamage(float damage)
    {
        // 1. Trừ máu
        currentHealth -= damage;
        Debug.Log("Player bị đánh! Máu còn: " + currentHealth);

        // 2. Cập nhật ngay lên thanh máu UI (Đây là cái bạn đang thiếu)
        if (healthBarScript != null)
        {
            healthBarScript.SetHealth(currentHealth);
        }

        // 3. Kiểm tra chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player đã nghẻo!");
        // Thêm code Game Over hoặc reload scene ở đây
        // Destroy(gameObject); 
    }
}