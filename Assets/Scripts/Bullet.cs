using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;      // 🚀 tốc độ cao
    public int damage = 3;
    public float lifeTime = 2f;
    public LayerMask enemyLayer;

    private Vector2 moveDir;

    public void Init(Vector2 dir)
    {
        moveDir = dir.normalized;

        // Xoay đạn theo hướng bay (nếu có sprite)
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
            other.GetComponent<Enemy_Health>()
                 ?.ChangeHealth(-damage);

            Destroy(gameObject);
        }
    }
}
