using UnityEngine;

public class Enemy_Health : MonoBehaviour
{
    public GameObject damageTextPrefab;
    private EnemyStats stats;

    public int currentHealth;
    public int energyReward = 10;

    private DamageFlash damageFlash;

    private void Start()
    {
        stats = GetComponent<EnemyStats>();

        if (stats != null)
        {
            currentHealth = stats.maxHealth;
        }
        else
        {
            currentHealth = 10;
            Debug.LogError("EnemyStats component missing! Using default health.");
        }

        damageFlash = GetComponent<DamageFlash>();
    }

    // =============================
    // 🔥 HÀM MỚI – CÓ PHÂN LOẠI DAMAGE
    // =============================
    public void TakeDamage(int amount, DamageType damageType)
    {
        ShowDamage(amount);
        ApplyDamage(amount);

        if (currentHealth <= 0)
        {
            // Logic cộng năng lượng riêng cho đánh thường
            if (damageType == DamageType.NormalAttack)
            {
                GiveEnergy();
            }
            
            Die(); // Gọi hàm chết chung
        }
    }

    // =============================
    // ⚠ GIỮ NGUYÊN – DÙNG CHO SKILL CŨ
    // =============================
    public void ChangeHealth(int amount)
    {
        ShowDamage(amount);
        ApplyDamage(amount);

        if (currentHealth <= 0)
        {
            GiveEnergy(); // Vẫn cộng năng lượng như cũ
            Die(); // Gọi hàm chết chung (QUAN TRỌNG ĐỂ FIX LỖI WAVE)
        }
    }

    // --- HÀM XỬ LÝ CHẾT (GỘP CHUNG ĐỂ TRÁNH LỖI) ---
    void Die()
    {
        // 1. Báo cáo cho WaveManager (Quan trọng nhất)
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyKilled();
        }

        // 2. Hủy object
        Destroy(gameObject);
    }

    // --- HÀM CỘNG NĂNG LƯỢNG ---
    void GiveEnergy()
    {
        Player_Energy energy = FindObjectOfType<Player_Energy>();
        if (energy != null)
        {
            energy.AddEnergy(energyReward);
        }
    }

    // ===== TÁCH LOGIC PHỤ =====
    void ShowDamage(int amount)
    {
        if (damageTextPrefab != null && amount < 0)
        {
            GameObject textInstance = Instantiate(
                damageTextPrefab,
                transform.position,
                Quaternion.identity
            );

            textInstance.GetComponent<DamageText>()
                        ?.SetDamageValue(amount);
        }

        if (amount < 0 && damageFlash != null)
        {
            damageFlash.Flash();
        }
    }

    void ApplyDamage(int amount)
    {
        currentHealth += amount;

        if (stats != null && currentHealth > stats.maxHealth)
        {
            currentHealth = stats.maxHealth;
        }
    }
}