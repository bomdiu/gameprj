using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float moveSpeed = 5f;
    public float attackDamage = 10f;

    [Header("References")]
    public PlayerMovement movementScript; 
    public HealthBarUI healthBarScript; 
    private DamageFlash damageFlash; 

    [Header("Damage Text Settings")]
    public GameObject damageTextPrefab; // Kéo Prefab DamageText vào đây
    public Vector3 textOffset = new Vector3(0, 1.5f, 0); // Vị trí hiện số trên đầu

    private bool isDead = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        damageFlash = GetComponent<DamageFlash>();

        if (movementScript == null) 
            movementScript = GetComponent<PlayerMovement>();
            
        if (movementScript != null)
             movementScript.moveSpeed = moveSpeed; 
        
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // 1. Trừ máu và chặn không cho xuống dưới 0
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player bị đánh! Máu còn: " + currentHealth);

        // 2. HIỆN SỐ SÁT THƯƠNG (Màu đỏ cho Player bị đánh)
        SpawnDamageText(Mathf.RoundToInt(damage), Color.red);

        // 3. Hiệu ứng nháy đỏ
        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

        // 4. Cập nhật thanh máu UI
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Hàm tạo số sát thương nhảy lên
    public void SpawnDamageText(int amount, Color color)
    {
        if (damageTextPrefab != null)
        {
            // Tạo ra Prefab chữ tại vị trí Player + một khoảng lệch lên trên
            GameObject popup = Instantiate(damageTextPrefab, transform.position + textOffset, Quaternion.identity);
            
            // Gọi hàm Setup của script DamagePopup (Script t đưa m ở trên)
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.Setup(amount, color);
            }
        }
    }

    private void UpdateUI()
    {
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
                UpdateUI();
                break;
            case SkillData.SkillType.SpeedUp:
                moveSpeed += amount;
                if (movementScript != null) movementScript.moveSpeed = moveSpeed; 
                break;
        }
    }
    
    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("Player đã nghẻo!");
        gameObject.SetActive(false); 
    }
}