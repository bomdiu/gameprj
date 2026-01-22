using UnityEngine;
using System.Collections.Generic;

public class DamageDealer : MonoBehaviour
{
    private PlayerCombat combat;
    private List<GameObject> enemiesHit = new List<GameObject>();

    void Awake()
    {
        // Finds the combat script you provided on the parent object
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
        if (collision.CompareTag("Enemy"))
        {
            if (!enemiesHit.Contains(collision.gameObject))
            {
                Enemy_Health enemy = collision.GetComponent<Enemy_Health>();
                if (enemy != null && combat != null)
                {
                    // 1. GET DAMAGE & CRIT FLAG: Call the method with the 'out' parameter
                    // This triggers the "CRIT!" log in your Combat script
                    int dmg = combat.GetCurrentDamage(out bool isCrit);

                    // 2. APPLY DAMAGE: Pass the calculated dmg and the exact isCrit flag
                    // This ensures the enemy shows yellow text correctly
                    enemy.TakeDamage(dmg, DamageType.NormalAttack, isCrit);

                    // 3. TRIGGER LIFESTEAL: Uses the playerCombat logic
                    // If lifestealChance is 1.0f, this will heal the player every hit
                    combat.ApplyLifesteal(dmg);

                    enemiesHit.Add(collision.gameObject);
                }
            }
        }
    }
}