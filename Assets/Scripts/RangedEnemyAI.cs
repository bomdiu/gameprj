using UnityEngine;

public class RangedEnemyAI : MonoBehaviour
{
    [Header("Ranged Settings")]
    [SerializeField] private float stopDistance = 6f;      
    [SerializeField] private float retreatDistance = 3f;  
    [SerializeField] private float attackCooldown = 2f;
    
    [Header("Movement Settings")]
    [Range(0.1f, 10f)] 
    [SerializeField] private float generalMoveSpeed = 3.5f; 

    [Header("Ranged Attack Settings")]
    [SerializeField] private GameObject bulletPrefab; 
    [SerializeField] private Transform firePoint;     
    [SerializeField] private LayerMask obstacleLayer; // Should be set to 'Default' or your Wall layer

    private EnemyPathfinding motor;
    private Transform player;
    private Animator anim;
    private float lastAttackTime;

    private void Start() {
        motor = GetComponent<EnemyPathfinding>();
        anim = GetComponent<Animator>();

        if (motor != null) motor.SetMoveSpeed(generalMoveSpeed);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update() {
        if (player == null || motor == null) return;

        motor.SetMoveSpeed(generalMoveSpeed);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool hasLineOfSight = CheckLineOfSight();
        Vector2 directionToMove = Vector2.zero;

        // 1. DECIDE DIRECTION
        if (distanceToPlayer < retreatDistance) {
            // Priority 1: If player is too close, always retreat even if blocked
            directionToMove = (transform.position - player.position).normalized;
        } 
        else if (distanceToPlayer > stopDistance || !hasLineOfSight) {
            // Priority 2: If too far OR the shot is blocked, move closer to find an angle
            directionToMove = (player.position - transform.position).normalized;
        } 
        else {
            // Priority 3: Within range AND clear shot, stay put
            directionToMove = Vector2.zero;
        }

        motor.SetMoveDir(directionToMove);
        motor.FlipToTarget(player.position.x);

        // 2. ATTACK LOGIC (Only shoot if range is correct AND LoS is clear)
        if (distanceToPlayer <= stopDistance && hasLineOfSight && Time.time >= lastAttackTime + attackCooldown) {
            AttackTrigger();
        }
    }

    private bool CheckLineOfSight() {
        if (player == null || firePoint == null) return false;

        Vector2 direction = (player.position - firePoint.position).normalized;
        float distance = Vector2.Distance(firePoint.position, player.position);

        // Raycast toward the player to see if we hit a wall first
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, distance, obstacleLayer);

        // If the raycast hits nothing, the path is clear.
        // If it hits something, it means an obstacle is in the way.
        return hit.collider == null;
    }

    private void AttackTrigger() {
        lastAttackTime = Time.time;
        if (anim != null) anim.SetTrigger("Attack");
    }

    public void LaunchProjectile() {
        if (player == null) return;

        if (bulletPrefab != null && firePoint != null) {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Vector2 fireDir = (player.position - firePoint.position).normalized;
            
            EnemyProjectile projectileScript = bullet.GetComponent<EnemyProjectile>();
            if (projectileScript != null) {
                projectileScript.Setup(fireDir);
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);

        // Visualizing the Line of Sight in Scene View
        if (player != null && firePoint != null) {
            bool clear = CheckLineOfSight();
            Gizmos.color = clear ? Color.green : Color.red;
            Gizmos.DrawLine(firePoint.position, player.position);
        }
    }
}