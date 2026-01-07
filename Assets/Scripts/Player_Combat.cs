using System.Collections.Generic; // Cần thêm dòng này để dùng HashSet
using UnityEngine;

public class Player_Combat : MonoBehaviour
{
    public Transform attackPoint;
    public float weaponRange = 1;
    public LayerMask enemyLayer;

    public Animator anim;
    public float cooldown = 2;
    private float timer;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalAttackPos;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (attackPoint != null) originalAttackPos = attackPoint.localPosition;
    }

    private void Update()
    {
        if (timer > 0) timer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        HandleFlip();
    }

    public void Attack()
    {
        if (timer <= 0)
        {
            anim.SetBool("isAttacking", true);
            timer = cooldown;
        }
    }

    // --- HÀM GÂY SÁT THƯƠNG ĐÃ ĐƯỢC FIX LỖI "CHẾT LUÔN" ---
    public void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, enemyLayer);

        // Dùng HashSet để chắc chắn mỗi con quái chỉ nhận sát thương 1 lần mỗi nhát chém
        HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

        int realDamage = 1;
        if (PlayerStats.Instance != null)
        {
            realDamage = Mathf.RoundToInt(PlayerStats.Instance.attackDamage);
        }

        foreach (Collider2D enemyCollider in enemies)
        {
            // Lấy GameObject chính của con quái
            GameObject enemyObj = enemyCollider.gameObject;

            if (!hitEnemies.Contains(enemyObj))
            {
                Enemy_Health health = enemyObj.GetComponent<Enemy_Health>();
                if (health != null)
                {
                    // Truyền số dương. Enemy_Health sẽ tự trừ máu.
                    health.TakeDamage(realDamage, DamageType.NormalAttack);
                    
                    // HIỆN THÔNG BÁO: Kiểm tra sát thương có bị quá lớn không
                    Debug.Log("<color=cyan>Player gây: </color>" + realDamage + " sát thương lên " + enemyObj.name);
                    
                    hitEnemies.Add(enemyObj);
                }
            }
        }
    }

    public void FinishAttacking()
    {
        anim.SetBool("isAttacking", false);
    }

    private void HandleFlip()
    {
        if (spriteRenderer == null || attackPoint == null) return;

        if (spriteRenderer.flipX)
        {
            attackPoint.localPosition = new Vector3(Mathf.Abs(originalAttackPos.x), originalAttackPos.y, 0);
        }
        else
        {
            attackPoint.localPosition = new Vector3(-Mathf.Abs(originalAttackPos.x), originalAttackPos.y, 0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
    }
}