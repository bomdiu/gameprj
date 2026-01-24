using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float damageAmount = 10f; 
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Visual Settings")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 0;

    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>(); 
    }

    private void Start() {
        Destroy(gameObject, lifeTime); 
    }

    public void Setup(Vector2 dir) {
        moveDirection = dir;
        
        if (sr != null) {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void FixedUpdate() {
        rb.velocity = moveDirection * speed;
    }

   private void OnTriggerEnter2D(Collider2D collision) {
    // --- IGNORE OTHER PROJECTILES ---
    if (collision.GetComponent<EnemyProjectile>() != null) {
        return;
    }

    // Hit Player
    if (collision.CompareTag("Player")) {
        PlayerStats playerStats = collision.GetComponentInParent<PlayerStats>();
        if (playerStats != null) {
            playerStats.TakeDamage(damageAmount);
        }
        Destroy(gameObject);
    }
    
    // Hit Obstacles (Walls/Trees)
    if (((1 << collision.gameObject.layer) & obstacleLayer) != 0) {
        // --- THE FIX: Only destroy if it's a solid obstacle (NOT a trigger) ---
        if (!collision.isTrigger) {
            Destroy(gameObject);
        }
    }
}
}