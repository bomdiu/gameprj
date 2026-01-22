using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("References")]
    private PlayerHealth healthScript; 
    private DamageFlash damageFlash; 

    [Header("Damage Text Settings")]
    public GameObject damageTextPrefab; 
    public Vector3 textOffset = new Vector3(0, 1.5f, 0); 

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        healthScript = GetComponent<PlayerHealth>();
        damageFlash = GetComponent<DamageFlash>();
    }

    // This is the main entry point for enemies to hurt the player
    public void TakeDamage(float damage)
    {
        if (healthScript == null) return;

        // 1. Pass the damage to the Health Script logic
        healthScript.ChangeHealth(-(int)damage);

        // 2. Visual feedback: Damage Text
        SpawnDamageText(Mathf.RoundToInt(damage), Color.red);

        // 3. Visual feedback: Flash
        if (damageFlash != null) damageFlash.Flash();
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
}