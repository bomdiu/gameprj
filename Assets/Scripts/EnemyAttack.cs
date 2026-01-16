using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Stats")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private string attackTriggerName = "Attack";

    private float nextAttackTime;
    private Animator anim;
    private Transform currentTarget;

    private void Awake() {
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    public void TryToAttack(Transform playerTransform)
    {
        if (Time.time >= nextAttackTime)
        {
            // Store the player reference so the Animation Event knows who to hit
            currentTarget = playerTransform;

            if (anim != null) {
                anim.ResetTrigger(attackTriggerName); 
                anim.SetTrigger(attackTriggerName);
            }

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    // This function is called by your Animation Event
    public void DealDamageEvent() 
    {
        if (currentTarget != null) {
            // Find your new PlayerStats script
            PlayerStats stats = currentTarget.GetComponent<PlayerStats>();
            
            if (stats != null) {
                // This triggers the damage, the popup, and the flash all at once!
                stats.TakeDamage(damage);
            }
        }
    }
}