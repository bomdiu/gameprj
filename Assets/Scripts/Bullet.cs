using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 20f;
    public float lifeTime = 2f;
    public Vector2 moveDir;

    [Header("Combat Settings")]
    public int damage = 3;
    public LayerMask enemyLayer;

    [Header("Visual Settings")]
    public string sortingLayerName = "Projectiles"; // Change this in the Inspector
    public int sortingOrder = 10;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Init(Vector2 dir)
    {
        moveDir = dir.normalized;

        // Apply Sorting Layer settings
        if (sr != null)
        {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
        }

        // Rotate bullet to face movement direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(moveDir * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            // Trigger the damage logic on the enemy
            other.GetComponent<Enemy_Health>()?.ChangeHealth(-damage);

            Destroy(gameObject);
        }
    }
}