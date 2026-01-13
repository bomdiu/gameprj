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
    public GameObject damageTextPrefab; 
    public Vector3 textOffset = new Vector3(0, 1.5f, 0); 

    private bool isDead = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Auto-link logic for the health bar
        if (healthBarScript == null)
        {
            GameObject foundObj = GameObject.Find("healthbar");
            if (foundObj != null)
            {
                healthBarScript = foundObj.GetComponent<HealthBarUI>();
            }
        }

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

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SpawnDamageText(Mathf.RoundToInt(damage), Color.red);

        if (damageFlash != null) damageFlash.Flash();

        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    public void SpawnDamageText(int amount, Color color)
    {
        if (damageTextPrefab != null)
        {
            GameObject popup = Instantiate(damageTextPrefab, transform.position + textOffset, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null) popupScript.Setup(amount, color);
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
        Debug.Log("Player has died!");
        gameObject.SetActive(false); 
    }
}