using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;
    public HealthBarUI healthBarUI; 
    private bool isDead = false;

    [Header("Rare Upgrades")]
    public int healthRegen = 0; 

    [Header("Visual Effects")]
    [SerializeField] private GameObject healParticlePrefab; 

    void Start()
    {
        currentHealth = maxHealth; 
        if (healthBarUI != null)
        {
            healthBarUI.SetMaxHealth(maxHealth);
            healthBarUI.SetHealth(currentHealth);
        }
        StartCoroutine(RegenRoutine()); 
    }

    private IEnumerator RegenRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(3f);
            if (healthRegen > 0 && currentHealth < maxHealth)
            {
                Heal(healthRegen);
            }
        }
    }

  public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (healthBarUI != null) healthBarUI.SetHealth(currentHealth);

        if (healParticlePrefab != null)
        {
            // 1. Instantiate as usual
            GameObject effect = Instantiate(healParticlePrefab, transform.position, Quaternion.identity, transform);
            
            // 2. Start a small Coroutine to handle the "Fade and Cleanup"
            StartCoroutine(FadeAndDestroyParticles(effect, 1.5f));
        }
    }

    private IEnumerator FadeAndDestroyParticles(GameObject effect, float duration)
    {
        yield return new WaitForSeconds(duration);

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // 3. Stop emitting new particles
            ps.Stop();
            
            // 4. Wait for the existing particles to live out their remaining time (fade)
            // We wait for the 'startLifetime' so they can finish their Color Over Lifetime fade
            yield return new WaitForSeconds(ps.main.startLifetime.constantMax);
        }

        // 5. Finally, destroy the object
        Destroy(effect);
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
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
        if (healthBarUI != null) healthBarUI.SetHealth(currentHealth); 

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        // 1. SEND SIGNAL IMMEDIATELY
        // This must happen BEFORE hiding visuals or disabling scripts.
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerDeath(transform);
        }
        else
        {
            // If you see this, the GameOverManager is NOT in your Hierarchy!
            Debug.LogError("CRITICAL: GameOverManager Instance is null in Die()");
        }

        // 2. DISABLE MOVEMENT
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.canMove = false; 
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) 
            {
                rb.velocity = Vector2.zero;
                rb.simulated = false; // Freeze physics instantly
            }
        }

        // 4. DISABLE COLLISION
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log("Player Death Sequence complete.");
    }
}