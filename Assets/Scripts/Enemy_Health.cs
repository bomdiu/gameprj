using UnityEngine;

public class Enemy_Health : MonoBehaviour
{
    [Header("Settings")]
    public GameObject damageTextPrefab; // Kéo Prefab số nhảy vào đây
    public int energyReward = 10;

    [Header("Live Stats")]
    public int currentHealth;
    
    private EnemyStats stats;
    private DamageFlash damageFlash;

    private void Awake()
    {
        // Lấy component ngay khi đối tượng được tạo
        stats = GetComponent<EnemyStats>();
        damageFlash = GetComponent<DamageFlash>();
    }

    private void Start()
    {
        // ÉP máu hiện tại phải bằng máu tối đa khi bắt đầu game
        if (stats != null) 
        {
            currentHealth = stats.maxHealth;
        }
        else 
        {
            currentHealth = 100; // Mặc định 100 máu nếu thiếu Stats
            Debug.LogWarning("EnemyStats thiếu trên " + gameObject.name + ". Đã tự đặt 100 máu.");
        }
    }

    // --- HÀM NHẬN SÁT THƯƠNG CHÍNH ---
    public void TakeDamage(int amount, DamageType damageType)
    {
        // Đảm bảo amount luôn là số dương để trừ máu đúng cách
        int damageToApply = Mathf.Abs(amount);
        if (damageToApply <= 0) return;

        ApplyDamageLogic(damageToApply);
        ShowDamagePopup(damageToApply);

        // Chỉ chết khi máu thực sự về 0
        if (currentHealth <= 0)
        {
            if (damageType == DamageType.NormalAttack) GiveEnergy();
            Die();
        }
    }

    // --- HÀM DÙNG CHO SKILL HOẶC HỒI MÁU ---
    public void ChangeHealth(int amount)
    {
        if (amount < 0) // Gây sát thương
        {
            int damage = Mathf.Abs(amount);
            ApplyDamageLogic(damage);
            ShowDamagePopup(damage);
        }
        else // Hồi máu
        {
            currentHealth += amount;
            if (stats != null) currentHealth = Mathf.Min(currentHealth, stats.maxHealth);
        }

        if (currentHealth <= 0)
        {
            GiveEnergy();
            Die();
        }
    }

    private void ApplyDamageLogic(int amount)
    {
        currentHealth -= amount; // Thực hiện trừ máu
        currentHealth = Mathf.Max(currentHealth, 0); // Đảm bảo không bị máu âm
    }

    void ShowDamagePopup(int amount)
    {
        if (damageTextPrefab != null)
        {
            // Ép trục Z = -1 để số luôn hiện phía trước
            Vector3 spawnPos = transform.position + Vector3.up;
            spawnPos.z = -1f; 

            GameObject textInstance = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            
            DamagePopup popupScript = textInstance.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                // Truyền số dương và màu vàng cho quái
                popupScript.Setup(amount, Color.yellow); 
            }
        }

        if (damageFlash != null) damageFlash.Flash();
    }

    void GiveEnergy()
    {
        Player_Energy energy = FindObjectOfType<Player_Energy>();
        if (energy != null) energy.AddEnergy(energyReward);
    }

    void Die()
    {
        if (WaveManager.Instance != null) WaveManager.Instance.OnEnemyKilled();
        Destroy(gameObject); // Xóa quái
    }
}