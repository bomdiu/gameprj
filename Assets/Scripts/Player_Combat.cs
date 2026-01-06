using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Combat : MonoBehaviour
{
    public Transform attackPoint;
    public float weaponRange = 1;
    public LayerMask enemyLayer;
    
    // --- BỎ DÒNG NÀY ĐI HOẶC ẨN NÓ ĐI ---
    // public int damage = 1; // Biến này không dùng nữa vì nó không đồng bộ
    // ------------------------------------

    public Animator anim;
    public float cooldown = 2;
    private float timer;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalAttackPos;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalAttackPos = attackPoint.localPosition;
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        // Lật vị trí đánh theo hướng nhân vật
        if (spriteRenderer.flipX)
        {
            attackPoint.localPosition = new Vector3(Mathf.Abs(originalAttackPos.x), originalAttackPos.y, 0);
        }
        else
        {
            attackPoint.localPosition = new Vector3(-Mathf.Abs(originalAttackPos.x), originalAttackPos.y, 0);
        }
    }

    public void Attack()
    {
        if (timer <= 0)
        {
            anim.SetBool("isAttacking", true);
            timer = cooldown;
        }
    }

    // --- SỬA CHÍNH Ở ĐÂY ---
    public void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            weaponRange,
            enemyLayer
        );

        if (enemies.Length > 0)
        {
            // 1. Lấy chỉ số Damage mới nhất từ PlayerStats
            // Lưu ý: Phải ép kiểu (int) vì PlayerStats dùng float còn hàm TakeDamage của bạn có vẻ dùng int
            int realDamage = 1; // Mặc định là 1 phòng trường hợp lỗi

            if (PlayerStats.Instance != null)
            {
                realDamage = (int)PlayerStats.Instance.attackDamage;
            }

            // 2. Gây sát thương
            Enemy_Health enemy = enemies[0].GetComponent<Enemy_Health>();
            if (enemy != null)
            {
                // Truyền realDamage vào thay vì biến damage cũ
                enemy.TakeDamage(-realDamage, DamageType.NormalAttack);
                
                // Debug để kiểm tra xem damage đã tăng chưa
                Debug.Log("Đã chém: " + realDamage + " sát thương!"); 
            }
        }
    }
    // -----------------------

    public void FinishAttacking()
    {
        anim.SetBool("isAttacking", false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
    }
}