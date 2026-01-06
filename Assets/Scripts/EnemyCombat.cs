using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    private EnemyStats stats; // Tham chiếu đến script EnemyStats
    public Transform attackPoint;
    public float weaponRange;
    public LayerMask playerLayer;

    // private void OnCollisionEnter2D (Collision2D collision)
    // {
    //     if (collision.gameObject.tag == "Player")
    //     {
    //         collision.gameObject.GetComponent<PlayerHealth>().ChangeHealth (-damage);
    //     }
    // }
    void Start()
    {
        // Lấy component EnemyStats khi bắt đầu
        stats = GetComponent<EnemyStats>();
        if (stats == null)
        {
            Debug.LogError("EnemyStats component missing on " + gameObject.name);
        }
    }

    public void Attack()
    {
        if (stats == null) return;

        // Sử dụng chỉ số baseDamage từ EnemyStats
        int currentDamage = stats.baseDamage;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, playerLayer);

        if(hits.Length > 0)
        {
            // --- SỬA Ở ĐÂY ---
            // Gọi thẳng vào PlayerStats thông qua Instance
            if (PlayerStats.Instance != null)
            {
                // Truyền currentDamage (số dương), hàm TakeDamage sẽ tự trừ
                PlayerStats.Instance.TakeDamage(currentDamage);
            }
        }
    }
}