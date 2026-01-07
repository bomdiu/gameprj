using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    private EnemyStats stats; // Tham chiếu đến script EnemyStats
    
    [Header("Attack Settings")]
    public Transform attackPoint;   // Điểm tâm của đòn đánh (thường là tay hoặc vũ khí)
    public float weaponRange = 0.5f; // Tầm xa của đòn đánh
    public LayerMask playerLayer;    // Lớp Layer của Player

    void Start()
    {
        // Lấy component EnemyStats khi bắt đầu
        stats = GetComponent<EnemyStats>();
        
        if (stats == null)
        {
            Debug.LogError("LỖI: Thiếu script EnemyStats trên " + gameObject.name);
        }

        // Kiểm tra nếu quên chưa gán attackPoint
        if (attackPoint == null)
        {
            attackPoint = this.transform;
            Debug.LogWarning("Cảnh báo: Chưa gán Attack Point cho " + gameObject.name + ". Đang tự lấy vị trí của quái.");
        }
    }

    // Hàm này được gọi từ Animation Event hoặc logic AI của bạn
    public void Attack()
    {
        if (stats == null) return;

        // Lấy sát thương từ EnemyStats
        int currentDamage = stats.baseDamage;

        // Quét tìm Player trong vùng đánh 
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, playerLayer);

        if (hits.Length > 0)
        {
            // Chỉ cần chạm trúng ít nhất 1 collider thuộc lớp Player
            // Chúng ta gọi thẳng vào Singleton của PlayerStats
            if (PlayerStats.Instance != null)
            {
                // Gọi hàm TakeDamage (hàm này sẽ tự trừ máu, nháy đỏ và cập nhật UI)
                PlayerStats.Instance.TakeDamage(currentDamage);
                Debug.Log("Quái đã đánh trúng Player! Sát thương: " + currentDamage);
            }
        }
    }

    // Vẽ vòng tròn tầm đánh để bạn dễ căn chỉnh trong Unity Editor
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
    }
}