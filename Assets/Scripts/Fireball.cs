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

    void Start()
    {
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
            // 1. Orphan from Fireball
            explosionParticles.transform.SetParent(null);

            // 2. FIX SCALE: Force it back to normal (1,1,1)
            explosionParticles.transform.localScale = Vector3.one;

            // 3. FIX POSITION: Force Z to 0 so it's not hidden
            Vector3 finalPos = explosionParticles.transform.position;
            finalPos.z = 0;
            explosionParticles.transform.position = finalPos;

            // 4. Play your burst
            explosionParticles.Play();

            // 5. Delete particles after 5 seconds
            Destroy(explosionParticles.gameObject, 5f);
        }

        // Damage calculation
        ApplyExplosionDamage(directHitTarget);

        // Destroy the fireball logic immediately
        Destroy(gameObject);
    }

    private void ApplyExplosionDamage(GameObject directHitTarget)
    {
        if (directHitTarget != null && directHitTarget.CompareTag("Enemy"))
        {
            directHitTarget.GetComponent<Enemy_Health>()?.TakeDamage(directHitDamage, DamageType.NormalAttack, true);
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            float damageMultiplier = 1f - Mathf.Clamp01(distance / explosionRadius);
            int finalDamage = Mathf.RoundToInt(maxExplosionDamage * damageMultiplier);
            enemy.GetComponent<Enemy_Health>()?.TakeDamage(finalDamage, DamageType.NormalAttack, false);

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