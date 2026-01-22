using UnityEngine;
using System.Collections; // Required for Coroutines

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;
    public HealthBarUI healthBarUI; 
    private bool isDead = false;

    [Header("Rare Upgrades")]
    [Tooltip("Amount of HP restored every second.")]
    public int healthRegen = 0; // Rare: Health Regen (int/s)

    void Start()
    {
        currentHealth = maxHealth; 
        if (healthBarUI != null)
        {
            healthBarUI.SetMaxHealth(maxHealth);
            healthBarUI.SetHealth(currentHealth);
        }

        // Start the regeneration loop immediately
        StartCoroutine(RegenRoutine()); 
    }

    private IEnumerator RegenRoutine()
    {
        while (!isDead)
        {
            // Wait for 1 second
            yield return new WaitForSeconds(1f);

            // Only heal if the player is alive, has regen, and is below max health
            if (healthRegen > 0 && currentHealth < maxHealth)
            {
                // Heals the player and ensures it doesn't exceed max
                currentHealth = Mathf.Min(currentHealth + healthRegen, maxHealth);
                
                // Update UI after regen
                if (healthBarUI != null) healthBarUI.SetHealth(currentHealth);
            }
        }
    }

    // New method for the UpgradeManager to call
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        
        // Standard RPG rule: heal by the same amount max HP increased
        currentHealth += amount;

        // Update UI to reflect new maximum
        if (healthBarUI != null)
        {
            healthBarUI.SetMaxHealth(maxHealth);
            healthBarUI.SetHealth(currentHealth);
        }
    }

    public void ChangeHealth(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(currentHealth); 
        }

        if (currentHealth <= 0)
        {
            Die();
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