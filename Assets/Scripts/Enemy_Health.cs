using UnityEngine;
using System.Collections;

public class Enemy_Health : MonoBehaviour
{
    [Header("Settings")]
    public GameObject damageTextPrefab; 
    public int energyReward = 10;

    [Header("Juice Settings")]
    [SerializeField] private float hitstopDuration = 0.06f; 
    [SerializeField] private float knockbackForce = 7f;
    [SerializeField] private float knockbackStunTime = 0.15f;

    [Header("Morph (Squash & Stretch)")]
    [Tooltip("Tỷ lệ bẹt (X rộng ra, Y lùn đi)")]
    [SerializeField] private Vector3 squashScale = new Vector3(1.3f, 0.7f, 1f);
    [Tooltip("Thời gian giữ trạng thái bẹt (giây)")]
    [SerializeField] private float squashHoldDuration = 0.05f; 
    [Tooltip("Tốc độ nảy lại hình dạng ban đầu")]
    [SerializeField] private float morphRestoreSpeed = 8f;

    [Header("Death Juice")]
    [SerializeField] private GameObject deathParticlePrefab;
    [SerializeField] private float deathHitstop = 0.25f; 
    [SerializeField] private float deathDestroyDelay = 0.2f; 
    [SerializeField] private Vector3 deathSquashScale = new Vector3(1.8f, 0.3f, 1f);
    
    [Header("Death Timing")]
    [SerializeField] private float deathStartVelocity = 35f; // Vận tốc bùng nổ ban đầu
    [SerializeField] private float deathKnockbackDuration = 0.1f; // Thời gian bay lùi (nên dài hơn một chút để thấy rõ độ giảm tốc)
    [SerializeField] private float deathVerticalPop = 0.5f; 

    [Header("Visual Orientation")]
    [SerializeField] private bool isFacingRightByDefault = true; 

    [Header("Live Stats")]
    public int currentHealth;
    
    private EnemyStats stats;
    private DamageFlash damageFlash;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float knockbackTimer;

    private Vector3 originalScale;
    private Coroutine morphCoroutine;
    private bool isDead = false; 

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        damageFlash = GetComponentInChildren<DamageFlash>(); 
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        if (stats != null) currentHealth = stats.maxHealth;
        else currentHealth = 100;
    }

    private void Update()
    {
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int amount, DamageType damageType)
    {
        if (isDead) return;

        int damageToApply = Mathf.Abs(amount);
        if (damageToApply <= 0) return;

        ApplyDamageLogic(damageToApply);
        ShowDamagePopup(damageToApply);

        if (currentHealth <= 0)
        {
            if (damageType == DamageType.NormalAttack) GiveEnergy();
            Die();
            return;
        }

        if (damageFlash != null) damageFlash.Flash();
        if (HitStopManager.Instance != null) HitStopManager.Instance.HitStop(hitstopDuration);

        ApplyMorphEffect();
        ApplyAutoKnockback();
    }

    private void ApplyMorphEffect()
    {
        if (morphCoroutine != null) StopCoroutine(morphCoroutine);
        morphCoroutine = StartCoroutine(MorphRoutine());
    }

    private IEnumerator MorphRoutine()
    {
        transform.localScale = Vector3.Scale(originalScale, squashScale);
        yield return new WaitForSecondsRealtime(squashHoldDuration);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime * morphRestoreSpeed;
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, elapsed);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Shutdown Brain
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            if (s != this) s.enabled = false;
        }

        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        // 1. INITIAL EXPLOSIVE VELOCITY
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && rb != null)
        {
            Vector2 direction = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
            direction.y += deathVerticalPop;
            rb.velocity = direction.normalized * deathStartVelocity;
        }

        if (damageFlash != null) damageFlash.FlashIndefinitely();

        // 2. DECREASE VELOCITY OVER TIME (The Smooth Slowdown)
        float elapsed = 0f;
        Vector2 initialVelocity = rb.velocity;

        while (elapsed < deathKnockbackDuration)
        {
            elapsed += Time.deltaTime;
            // Giảm dần vận tốc về 0 dựa trên thời gian duration
            if (rb != null)
            {
                rb.velocity = Vector2.Lerp(initialVelocity, Vector2.zero, elapsed / deathKnockbackDuration);
            }
            yield return null;
        }

        // 3. COMPLETE STOP & FREEZE
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false; 
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (morphCoroutine != null) StopCoroutine(morphCoroutine);
        transform.localScale = Vector3.Scale(originalScale, deathSquashScale);

        // 4. LINGER
        yield return new WaitForSeconds(deathDestroyDelay);

        // 5. DISAPPEAR
        if (deathParticlePrefab != null) Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        if (HitStopManager.Instance != null) HitStopManager.Instance.HitStop(deathHitstop);

        if (WaveManager.Instance != null) WaveManager.Instance.OnEnemyKilled();
        Destroy(gameObject); 
    }

    // --- REMAINDER OF FUNCTIONS UNCHANGED ---
    public void ChangeHealth(int amount)
    {
        if (amount < 0) 
        {
            int damage = Mathf.Abs(amount);
            ApplyDamageLogic(damage);
            ShowDamagePopup(damage);
            if (damageFlash != null) damageFlash.Flash();
            if (HitStopManager.Instance != null) HitStopManager.Instance.HitStop(hitstopDuration);
            ApplyMorphEffect(); 
            ApplyAutoKnockback();
        }
        else 
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

    private void ApplyAutoKnockback()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && rb != null)
        {
            knockbackTimer = knockbackStunTime; 
            if (sr != null)
            {
                bool playerIsToTheLeft = player.transform.position.x < transform.position.x;
                if (isFacingRightByDefault) sr.flipX = !playerIsToTheLeft; 
                else sr.flipX = playerIsToTheLeft;
            }
            Vector2 direction = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
            rb.velocity = Vector2.zero; 
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public bool IsKnockedBack() => knockbackTimer > 0;
    private void ApplyDamageLogic(int amount) { currentHealth = Mathf.Max(currentHealth - amount, 0); }
    void ShowDamagePopup(int amount)
    {
        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up;
            spawnPos.z = -1f; 
            GameObject textInstance = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            DamagePopup popupScript = textInstance.GetComponent<DamagePopup>();
            if (popupScript != null) popupScript.Setup(amount, Color.yellow); 
        }
    }
    void GiveEnergy()
    {
        Player_Energy energy = FindObjectOfType<Player_Energy>();
        if (energy != null) energy.AddEnergy(energyReward);
    }
}