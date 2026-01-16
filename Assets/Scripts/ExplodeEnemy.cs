using UnityEngine;
using System.Collections;

public class CreeperAI : MonoBehaviour
{
    private enum CreeperState { Idle, Chasing, Priming, Recovering }
    [SerializeField] private CreeperState currentState = CreeperState.Idle;

    [Header("Detection & Chase")]
    public float detectionRange = 10f;
    public float fuseRange = 2f;
    public float chaseSpeed = 3.5f;

    [Header("Explosion Settings")]
    public float fuseTime = 1.5f;
    public float explosionRadius = 4f;
    public float explosionDamage = 40f;
    public float explosionKnockbackForce = 15f; 
    public Vector2 visualExplosionOffset; 
    public LayerMask playerLayer;
    public LayerMask enemyLayer; 
    public GameObject explosionEffect;
    public string explosionSortingLayer = "Visuals"; 

    [Header("Expansion Settings")]
    public float maxScaleMultiplier = 1.4f; 
    public float pulseSpeed = 15f;          
    public float recoverySpeed = 4f; 

    [Header("Visuals")]
    public GameObject exclamationMark;
    public Vector3 markOffset = new Vector3(0, 1.5f, 0); 
    public Color fuseColor = Color.red;
    [Range(5f, 30f)] public float blinkSpeed = 15f; 

    private EnemyPathfinding motor;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private DamageFlash flashController; 
    private Enemy_Health health; // Reference to your health script
    private Vector3 originalScale;
    private Vector3 markOriginalWorldScale; 
    private bool hasExploded = false;

    private void Start()
    {
        motor = GetComponent<EnemyPathfinding>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        flashController = GetComponent<DamageFlash>(); 
        health = GetComponent<Enemy_Health>(); // Initialize health reference
        originalScale = transform.localScale;

        if (exclamationMark != null) 
        {
            markOriginalWorldScale = exclamationMark.transform.lossyScale;
            exclamationMark.SetActive(false);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null || hasExploded || currentState == CreeperState.Recovering) return;

        // --- CHECK FOR KNOCKBACK ---
        // Uses your existing IsKnockedBack() method to pause AI
        if (health != null && health.IsKnockedBack()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case CreeperState.Idle:
                if (distanceToPlayer <= detectionRange)
                    currentState = CreeperState.Chasing;
                break;
            case CreeperState.Chasing:
                HandleChasing(distanceToPlayer);
                break;
        }
    }

    private void HandleChasing(float distance)
    {
        if (distance <= fuseRange)
        {
            StartCoroutine(PrimingSequence());
            return;
        }

        if (distance > detectionRange)
        {
            motor.StopMoving();
            currentState = CreeperState.Idle;
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        motor.SetMoveSpeed(chaseSpeed);
        motor.SetMoveDir(direction);
    }

    private IEnumerator PrimingSequence()
    {
        currentState = CreeperState.Priming;
        motor.StopMoving();

        if (exclamationMark != null)
        {
            exclamationMark.transform.SetParent(null); 
            exclamationMark.transform.localScale = markOriginalWorldScale;
            exclamationMark.SetActive(true);
        }

        float timer = 0;
        Color originalColor = spriteRenderer.color;

        while (timer < fuseTime)
        {
            // Pause the fuse timer if the enemy is knocked back
            if (health != null && health.IsKnockedBack())
            {
                yield return null;
                continue;
            }

            timer += Time.deltaTime;
            float progress = timer / fuseTime;

            if (flashController == null || !flashController.IsFlashing)
            {
                float flashValue = Mathf.PingPong(timer * 10f, 1f);
                spriteRenderer.color = Color.Lerp(originalColor, fuseColor, flashValue);
            }

            float growth = Mathf.Lerp(1f, maxScaleMultiplier, progress);
            float jitter = 1f + (Mathf.Sin(Time.time * pulseSpeed) * 0.05f * progress);
            transform.localScale = originalScale * growth * jitter;

            if (exclamationMark != null)
            {
                exclamationMark.transform.position = transform.position + markOffset;
                bool isVisible = Mathf.Sin(timer * blinkSpeed) > 0;
                exclamationMark.SetActive(isVisible);
            }

            if (Vector2.Distance(transform.position, player.position) > fuseRange * 1.5f)
            {
                StartCoroutine(RecoverSequence(originalColor));
                yield break;
            }

            yield return null;
        }

        Explode();
    }

    private IEnumerator RecoverSequence(Color targetColor)
    {
        currentState = CreeperState.Recovering;
        
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
            exclamationMark.transform.SetParent(transform);
            exclamationMark.transform.localScale = Vector3.one; 
        }

        Vector3 currentScale = transform.localScale;
        Color currentColor = spriteRenderer.color;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * recoverySpeed;
            transform.localScale = Vector3.Lerp(currentScale, originalScale, t);
            
            if (flashController == null || !flashController.IsFlashing)
                spriteRenderer.color = Color.Lerp(currentColor, targetColor, t);
            
            yield return null;
        }

        transform.localScale = originalScale;
        spriteRenderer.color = targetColor;
        currentState = CreeperState.Chasing;
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Vector3 physicsCenter = transform.position;
        Vector3 visualCenter = transform.position + (Vector3)visualExplosionOffset;

        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, visualCenter, Quaternion.identity);
            SpriteRenderer effectSR = effect.GetComponent<SpriteRenderer>();
            if (effectSR != null) effectSR.sortingLayerName = explosionSortingLayer;
        }

        // 1. Player Damage
        Collider2D hitPlayer = Physics2D.OverlapCircle(physicsCenter, explosionRadius, playerLayer);
        if (hitPlayer != null)
        {
            float distance = Vector2.Distance(physicsCenter, hitPlayer.transform.position);
            float damagePercent = 1f - (distance / explosionRadius);
            float finalDamage = explosionDamage * Mathf.Clamp01(damagePercent);
            hitPlayer.GetComponent<PlayerStats>()?.TakeDamage(finalDamage);
        }

        // 2. Push Enemies (Using Direct Rigidbody Access since Health script is unchanged)
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(physicsCenter, explosionRadius, enemyLayer);
        foreach (Collider2D enemy in nearbyEnemies)
        {
            if (enemy.gameObject == gameObject) continue;

            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 pushDirection = (enemy.transform.position - physicsCenter).normalized;
                enemyRb.velocity = Vector2.zero; // Reset to ensure consistent push
                enemyRb.AddForce(pushDirection * explosionKnockbackForce, ForceMode2D.Impulse);
            }
        }

        if (exclamationMark != null) Destroy(exclamationMark);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}