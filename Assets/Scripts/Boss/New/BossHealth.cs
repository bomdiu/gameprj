using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public float maxHealth = 1000f;
    [Header("Debug Info")]
    public float currentHealth; // Hãy quan sát biến này khi chạy game

    // Sửa logic Phase 2: Phải đảm bảo maxHealth > 0 để tránh lỗi chia cho 0
    public bool IsPhase2 
    {
        get 
        { 
            // Nếu chưa start game hoặc máu đầy thì ko bao giờ là Phase 2
            if (currentHealth == maxHealth) return false;
            return currentHealth <= (maxHealth * 0.5f); 
        }
    }

    // ĐỔI TỪ START THÀNH AWAKE
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject); 
    }
}