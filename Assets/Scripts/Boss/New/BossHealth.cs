using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Stats Configuration")]
    public int maxHealth = 1000;

    [Header("Visual Settings")]
    [Tooltip("The sorting layer the boss moves to when dying (e.g., 'Foreground')")]
    public string deathSortingLayer = "Foreground"; 

    [Header("Settings")]
    public GameObject damageTextPrefab; 

    [Header("Juice Settings")]
    [SerializeField] private float hitstopDuration = 0.08f; // Slightly longer for boss impact
    [SerializeField] private float knockbackForce = 0f;     // Bosses typically don't get knocked back far
    [SerializeField] private float knockbackStunTime = 0.0f; // Bosses might not get stunned easily

    [Header("Morph (Squash & Stretch)")]
    [Tooltip("Tỷ lệ bẹt (X rộng ra, Y lùn đi)")]
    [SerializeField] private Vector3 squashScale = new Vector3(1.1f, 0.9f, 1f); // Less squash for big boss
    [Tooltip("Thời gian giữ trạng thái bẹt (giây)")]
    [SerializeField] private float squashHoldDuration = 0.05f; 
    [Tooltip("Tốc độ nảy lại hình dạng ban đầu")]
    [SerializeField] private float morphRestoreSpeed = 5f;

    [Header("Death Juice")]
    [SerializeField] private GameObject deathParticlePrefab;
    [SerializeField] private float deathHitstop = 1.0f; // Epic slow motion on death
    [SerializeField] private float deathDestroyDelay = 3.0f; // Time for death animation to play
    [SerializeField] private Vector3 deathSquashScale = new Vector3(1.2f, 0.8f, 1f);
    
    [Header("Visual Orientation")]
    [SerializeField] private bool isFacingRightByDefault = true; 

    [Header("Live Stats")]
    public float currentHealth; // Boss health often uses float for precise phases
    
    // References
    private DamageFlash damageFlash;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BossHealthUI healthUI; 

    private Vector3 originalScale;
    private Coroutine morphCoroutine;
    private bool isDead = false; 

    // --- NEW: COLLISION SETTINGS ---
    [Header("Collision Settings")]
    [Tooltip("Tag của vũ khí/đạn của Player (VD: 'PlayerAttack')")]
    public string playerWeaponTag = "PlayerAttack"; 

    // Logic Phase
    public bool IsPhase2 
    {
        get 
        {
            if (currentHealth <= 0) return false;
            return currentHealth <= (maxHealth * 0.5f);
        }
    }

    private void Awake()
    {
        damageFlash = GetComponentInChildren<DamageFlash>(); 
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        healthUI = FindObjectOfType<BossHealthUI>(); // Or assigning in inspector

        // Store original scale from the VISUALS child if possible, otherwise self
        if (sr != null) originalScale = sr.transform.localScale;
        else originalScale = transform.localScale;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthUI != null) healthUI.UpdateHealth(currentHealth, maxHealth);
    }

    // --- MAIN DAMAGE FUNCTION ---
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        float damageToApply = Mathf.Abs(amount);
        if (damageToApply <= 0) return;

        // 1. Apply Logic
        currentHealth = Mathf.Max(currentHealth - damageToApply, 0);

        // 2. Update UI
        if (healthUI != null) healthUI.UpdateHealth(currentHealth, maxHealth);

        // 3. Visual Feedback (Juice)
        ShowDamagePopup(Mathf.RoundToInt(damageToApply));
        if (damageFlash != null) damageFlash.Flash();
        if (HitStopManager.Instance != null) HitStopManager.Instance.HitStop(hitstopDuration);
        ApplyMorphEffect();

        // 4. Check Death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // --- COLLISION DAMAGE (Auto-detect player attacks) ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag(playerWeaponTag))
        {
            float damageToTake = 10f; // Default damage
            
            // Try to get dynamic damage from the projectile/weapon
            // Example: DamageDealer script on the attack object
            /*
            DamageDealer dealer = collision.GetComponent<DamageDealer>();
            if (dealer != null) damageToTake = dealer.damage;
            */

            TakeDamage(damageToTake);
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("BOSS DEAD!");

        // 1. Disable Physics & Logic
        if (rb != null) rb.velocity = Vector2.zero;
        
        // Disable AI Scripts (assuming they are MonoBehaviours on the same object)
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            if (s != this) s.enabled = false;
        }

        // 2. Start Death Sequence
        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        // 1. Big Hitstop
        if (HitStopManager.Instance != null) HitStopManager.Instance.HitStop(deathHitstop);

        // 2. Flash
        if (damageFlash != null) damageFlash.FlashIndefinitely();

        // 3. Play Death Animation (optional, if you have Animator)
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger("Death");

        // 4. Wait for drama
        yield return new WaitForSeconds(deathDestroyDelay);

        // 5. Explosion FX
        if (deathParticlePrefab != null) Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);

        // 6. Destroy
        Destroy(gameObject); 
    }

    // --- VISUAL EFFECTS (Squash & Stretch) ---
    private void ApplyMorphEffect()
    {
        if (morphCoroutine != null) StopCoroutine(morphCoroutine);
        morphCoroutine = StartCoroutine(MorphRoutine());
    }

    private IEnumerator MorphRoutine()
    {
        Transform targetTransform = (sr != null) ? sr.transform : transform;

        // Squash
        targetTransform.localScale = Vector3.Scale(originalScale, squashScale);
        yield return new WaitForSecondsRealtime(squashHoldDuration);

        // Return to normal
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime * morphRestoreSpeed;
            targetTransform.localScale = Vector3.Lerp(targetTransform.localScale, originalScale, elapsed);
            yield return null;
        }

        targetTransform.localScale = originalScale;
    }

    void ShowDamagePopup(int amount)
    {
        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 2f; // Higher popup for boss
            spawnPos.z = -1f; 
            GameObject textInstance = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            
            // Assuming DamagePopup script exists as per your Enemy_Health
            DamagePopup popupScript = textInstance.GetComponent<DamagePopup>();
            if (popupScript != null) popupScript.Setup(amount, Color.red); // Boss hits usually red
        }
    }

    // --- DEV TOOLS ---
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(50);
        }
    }
}