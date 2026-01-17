using UnityEngine;
using System.Collections;

public class Enemy_Health : MonoBehaviour
{
    [Header("Stats Configuration")]
    public int maxHealth = 100;

    [Header("Visual Settings")]
    [Tooltip("The sorting layer the enemy moves to when dying (e.g., 'Foreground')")]
    public string deathSortingLayer = "Foreground"; 

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
    [SerializeField] private float deathStartVelocity = 35f; 
    [SerializeField] private float deathKnockbackDuration = 0.1f; 
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
        currentHealth = maxHealth;
        if (stats != null) 
        {
            maxHealth = stats.maxHealth;
            currentHealth = maxHealth;
        }
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

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. UPDATE SORTING LAYER IMMEDIATELY
        if (sr != null && !string.IsNullOrEmpty(deathSortingLayer))
        {
            sr.sortingLayerName = deathSortingLayer;
        }

        // 2. CONFIGURE COLLISION FOR DEATH
        // We keep the collider active to hit Obstacles but ignore Player/Enemies
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy"); 
        
        if (playerLayer != -1) Physics2D.IgnoreLayerCollision(gameObject.layer, playerLayer, true);
        if (enemyLayer != -1) Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, true);

        // Shutdown Brain/AI
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            if (s != this) s.enabled = false;
        }

        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        // 3. INITIAL EXPLOSIVE VELOCITY
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && rb != null)
        {
            Vector2 direction = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
            direction.y += deathVerticalPop;
            rb.velocity = direction.normalized * deathStartVelocity;
        }

        if (damageFlash != null) damageFlash.FlashIndefinitely();

        // 4. DECREASE VELOCITY OVER TIME
        float elapsed = 0f;
        while (elapsed < deathKnockbackDuration)
        {
            elapsed += Time.deltaTime;
            // Velocity is lerped, but rb.simulated remains true so it hits walls
            if (rb != null)
            {
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * (1f / deathKnockbackDuration));
            }
            yield return null;
        }

        // 5. STOP MOVEMENT
        // Simulation stays on so the enemy doesn't clip through obstacles if it hit one
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (morphCoroutine != null) StopCoroutine(morphCoroutine);
        transform.localScale = Vector3.Scale(originalScale, deathSquashScale);

        // 6. LINGER & DESTROY
        yield return new WaitForSeconds(deathDestroyDelay);

        if (deathParticlePrefab != null) Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        if (HitStopManager.Instance != null) HitStopManager.Instance.HitStop(deathHitstop);

        if (WaveManager.Instance != null) WaveManager.Instance.OnEnemyKilled();
        
        // Reset layer collisions before destruction to maintain global physics integrity
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer != -1) Physics2D.IgnoreLayerCollision(gameObject.layer, playerLayer, false);
        if (enemyLayer != -1) Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, false);

        Destroy(gameObject); 
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
            currentHealth = Mathf.Min(currentHealth, maxHealth);
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