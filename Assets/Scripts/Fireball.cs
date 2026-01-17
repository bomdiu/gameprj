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
    public GameObject explosionVFX;

    private bool hasExploded = false;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded || collision.CompareTag("Player")) return;
        Explode(collision.gameObject);
    }

    private void Explode(GameObject directHitTarget)
    {
        hasExploded = true;

        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        if (directHitTarget.CompareTag("Enemy"))
        {
            directHitTarget.GetComponent<Enemy_Health>()?.TakeDamage(directHitDamage, DamageType.NormalAttack);
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            float damageMultiplier = 1f - Mathf.Clamp01(distance / explosionRadius);
            int finalDamage = Mathf.RoundToInt(maxExplosionDamage * damageMultiplier);

            enemy.GetComponent<Enemy_Health>()?.TakeDamage(finalDamage, DamageType.NormalAttack);

            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (enemy.transform.position - transform.position).normalized;
                rb.velocity = Vector2.zero;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // FIX: Using manual color values to prevent definition errors
        Gizmos.color = new Color(1.0f, 0.64f, 0.0f); 
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}