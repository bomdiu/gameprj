using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 14f;
    public float lifetime = 2.5f;

    [Header("Explosion Settings")]
    public float explosionRadius = 3.5f;
    public int directHitDamage = 60;
    public int maxExplosionDamage = 40;
    public float knockbackForce = 18f;
    public LayerMask enemyLayer;
    public LayerMask obstacleLayer;

    [Header("Visual Effects")]
    public ParticleSystem explosionParticles; 

    private bool hasExploded = false;
    private PlayerCombat playerCombat; // Reference to find the bonus

    void Start()
    {
        // Find the player and their combat script to get the skillDamageBonus
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerCombat = player.GetComponent<PlayerCombat>();
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (hasExploded) return;
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded || collision.CompareTag("Player")) return;

        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            Explode(collision.gameObject);
        }
        else if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            if (!collision.isTrigger) Explode(null);
        }
    }

    private void Explode(GameObject directHitTarget)
    {
        hasExploded = true;

        if (explosionParticles != null)
        {
            explosionParticles.transform.SetParent(null);
            explosionParticles.transform.localScale = Vector3.one;
            Vector3 finalPos = explosionParticles.transform.position;
            finalPos.z = 0;
            explosionParticles.transform.position = finalPos;
            explosionParticles.Play();
            Destroy(explosionParticles.gameObject, 5f);
        }

        ApplyExplosionDamage(directHitTarget);
        Destroy(gameObject);
    }

   private void ApplyExplosionDamage(GameObject directHitTarget)
    {
        // Get the flat bonus from PlayerCombat (kept as skillDamageBonus)
        int bonus = (playerCombat != null) ? playerCombat.skillDamageBonus : 0;

        // --- 1. HANDLE DIRECT HIT ---
        if (directHitTarget != null)
        {
            // Add the flat bonus to the base direct hit damage
            int finalDirectDamage = directHitDamage + bonus;

            directHitTarget.GetComponent<Enemy_Health>()?.TakeDamage(finalDirectDamage, DamageType.NormalAttack, true);
            directHitTarget.GetComponent<BossHealth>()?.TakeDamage(finalDirectDamage);
        }

        // --- 2. HANDLE AREA OF EFFECT (AOE) ---
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.gameObject == directHitTarget) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            float damageMultiplier = 1f - Mathf.Clamp01(distance / explosionRadius);
            
            // Add the flat bonus to the AOE damage as well
            int finalAOEDamage = Mathf.RoundToInt((maxExplosionDamage + bonus) * damageMultiplier);

            enemy.GetComponent<Enemy_Health>()?.TakeDamage(finalAOEDamage, DamageType.NormalAttack, false);
            enemy.GetComponent<BossHealth>()?.TakeDamage(finalAOEDamage);

            // --- 3. KNOCKBACK ---
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (enemy.transform.position - transform.position).normalized;
                rb.velocity = Vector2.zero;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}