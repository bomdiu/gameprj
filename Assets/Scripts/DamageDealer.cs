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

// Inside DamageDealer.cs
private void OnDrawGizmos()
{
    BoxCollider2D collider = GetComponent<BoxCollider2D>();
    if (collider != null)
    {
        // This will now draw even if the GameObject is inactive in some Unity versions,
        // but for it to be reliable, the script component itself must be enabled.
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(collider.offset, collider.size);
    }
}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (!enemiesHit.Contains(collision.gameObject))
            {
                Enemy_Health enemy = collision.GetComponent<Enemy_Health>();
                if (enemy != null)
                {
                    int dmg = combat.GetCurrentDamage();
                    enemy.TakeDamage(dmg, DamageType.NormalAttack);
                    enemiesHit.Add(collision.gameObject);
                }
            }
        }
    }
}