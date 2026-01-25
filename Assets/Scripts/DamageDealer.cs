using UnityEngine;
using System.Collections.Generic;

public class DamageDealer : MonoBehaviour
{
    private PlayerCombat combat;
    private List<GameObject> enemiesHit = new List<GameObject>();

    void Awake()
    {
        combat = GetComponentInParent<PlayerCombat>();
    }

    void OnDisable()
    {
        enemiesHit.Clear();
    }

    public void ResetHitList()
    {
        enemiesHit.Clear();
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(collider.offset, collider.size);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ensure the Boss is tagged "Enemy" in the Inspector
        if (collision.CompareTag("Enemy"))
        {
            if (!enemiesHit.Contains(collision.gameObject))
            {
                // Try to find regular enemy OR the Boss script
                Enemy_Health regularEnemy = collision.GetComponent<Enemy_Health>();
                BossHealth bossEnemy = collision.GetComponent<BossHealth>();

                if (combat != null)
                {
                    int dmg = combat.GetCurrentDamage(out bool isCrit);

                    if (regularEnemy != null)
                    {
                        regularEnemy.TakeDamage(dmg, DamageType.NormalAttack, isCrit);
                        ApplyHitLogic(dmg, collision.gameObject);
                    }
                    else if (bossEnemy != null)
                    {
                        // Pass damage to the Boss
                        bossEnemy.TakeDamage(dmg); 
                        ApplyHitLogic(dmg, collision.gameObject);
                    }
                }
            }
        }
    }

    private void ApplyHitLogic(int dmg, GameObject target)
    {
        combat.ApplyLifesteal(dmg);
        enemiesHit.Add(target);
    }
}