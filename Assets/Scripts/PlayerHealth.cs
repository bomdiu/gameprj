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

    [Header("Audio Settings")] // MỚI: Quản lý âm thanh nhận sát thương và chết
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hurtSFX;
    [SerializeField] private AudioClip deathSFX;

    void Start()
    {
        currentHealth = maxHealth; 
        
        // Tự động tìm AudioSource nếu chưa gán
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        SyncHealthUI(); 
        StartCoroutine(RegenRoutine()); 
    }

    public void SyncHealthUI()
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetMaxHealth(maxHealth);
            healthBarUI.SetHealth(currentHealth);
        }
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
            GameObject effect = Instantiate(healParticlePrefab, transform.position, Quaternion.identity, transform);
            StartCoroutine(FadeAndDestroyParticles(effect, 1.5f));
        }
    }

    private IEnumerator FadeAndDestroyParticles(GameObject effect, float duration)
    {
        yield return new WaitForSeconds(duration);

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop();
            yield return new WaitForSeconds(ps.main.startLifetime.constantMax);
        }

        Destroy(effect);
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        SyncHealthUI(); 
    }

    public void ChangeHealth(int amount)
    {
        if (isDead) return;

        // MỚI: Nếu amount < 0 tức là nhận sát thương -> Phát âm thanh hurt
        if (amount < 0 && audioSource != null && hurtSFX != null)
        {
            audioSource.PlayOneShot(hurtSFX);
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (healthBarUI != null) healthBarUI.SetHealth(currentHealth); 

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // MỚI: Phát âm thanh khi chết
        if (audioSource != null && deathSFX != null)
        {
            audioSource.PlayOneShot(deathSFX);
        }
        
        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.ResetStats();
        }

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerDeath(transform);
        }
        else
        {
            Debug.LogError("CRITICAL: GameOverManager Instance is null in Die()");
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.canMove = false; 
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) 
            {
                rb.velocity = Vector2.zero;
                rb.simulated = false; 
            }
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log("Player Death Sequence complete.");
    }
}