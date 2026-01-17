using UnityEngine;
using System.Collections.Generic;

public class ShockwaveEffect : MonoBehaviour
{
    private ShockwaveSkillData data;
    private float currentRadius = 0f;
    private bool isExpanding = false;

    // Components
    private LineRenderer lineRend;
    private CircleCollider2D col;
    
    // Danh sách để đảm bảo mỗi lần sóng quét qua chỉ đánh trúng player 1 lần
    private List<GameObject> hitTargets = new List<GameObject>();

    public void Setup(ShockwaveSkillData skillData)
    {
        data = skillData;
        
        // 1. Setup LineRenderer
        lineRend = gameObject.AddComponent<LineRenderer>();
        lineRend.useWorldSpace = true; // Vẽ theo tọa độ thế giới nhưng cập nhật vị trí theo tâm
        lineRend.loop = true;
        lineRend.positionCount = data.segments;
        lineRend.startColor = data.waveColor;
        lineRend.endColor = new Color(data.waveColor.r, data.waveColor.g, data.waveColor.b, 0f); // Mờ dần ở viền ngoài
        lineRend.material = new Material(Shader.Find("Sprites/Default"));
        lineRend.startWidth = data.startWidth;
        lineRend.endWidth = data.startWidth;

        // 2. Setup Collider
        col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.1f; // Bắt đầu nhỏ xíu

        isExpanding = true;
    }

    void Update()
    {
        if (!isExpanding) return;

        // --- LOGIC MỞ RỘNG ---
        // Tăng bán kính theo tốc độ
        currentRadius += data.expansionSpeed * Time.deltaTime;

        // Cập nhật Collider
        col.radius = currentRadius;

        // Cập nhật độ dày nét vẽ (Lerp từ dày sang mỏng/to dần)
        float progress = currentRadius / data.maxRadius;
        float currentWidth = Mathf.Lerp(data.startWidth, data.endWidth, progress);
        lineRend.startWidth = currentWidth;
        lineRend.endWidth = currentWidth;

        // Cập nhật Visual (Vẽ vòng tròn)
        DrawCircle(currentRadius);

        // --- KIỂM TRA KẾT THÚC ---
        if (currentRadius >= data.maxRadius)
        {
            Destroy(gameObject); // Sóng tan biến
        }
    }

    void DrawCircle(float radius)
    {
        float angleStep = 360f / data.segments;
        for (int i = 0; i < data.segments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float x = Mathf.Sin(angle) * radius;
            float y = Mathf.Cos(angle) * radius;
            
            // Vẽ xung quanh tâm của object này (tức là tâm Boss)
            lineRend.SetPosition(i, transform.position + new Vector3(x, y, 0));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chỉ xử lý Player và chưa bị đánh trúng lần nào
        if (collision.CompareTag("Player") && !hitTargets.Contains(collision.gameObject))
        {
            hitTargets.Add(collision.gameObject);
            
            // 1. Gây Damage
            Debug.Log("Shockwave hit Player! Damage: " + data.damage);
            // collision.GetComponent<PlayerHealth>().TakeDamage(data.damage);

            // 2. XỬ LÝ KNOCKBACK (ĐẨY LÙI)
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Tính hướng đẩy: Từ tâm Boss -> Hướng về phía Player
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                
                // Reset vận tốc cũ để lực đẩy có tác dụng tức thì (tránh trường hợp player đang lao tới làm giảm lực đẩy)
                playerRb.velocity = Vector2.zero;
                
                // Thêm lực đẩy (Impulse = Lực tức thời)
                playerRb.AddForce(knockbackDir * data.knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}