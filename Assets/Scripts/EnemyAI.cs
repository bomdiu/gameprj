using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float everywhereSpeed = 2f; 
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Detection Ranges (Capsules)")]
    [SerializeField] private float chaseRange = 5f; 
    [SerializeField] private Vector2 stopCapsuleSize = new Vector2(1.5f, 2.5f);
    [SerializeField] private Vector2 attackCapsuleSize = new Vector2(1.2f, 2.0f);
    
    [Tooltip("Moves the detection capsules relative to the enemy center.")]
    [SerializeField] private Vector2 capsuleOffset; // <--- NEW OFFSET VARIABLE

    [Header("Layer Setup")]
    [SerializeField] private LayerMask playerLayer;

    private Transform player;
    private EnemyPathfinding enemyPathfinding;
    private EnemyAttack enemyAttack;

    private void Awake() {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        enemyAttack = GetComponent<EnemyAttack>();
    }

    private void Start() {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }
private void FixedUpdate()
{
    // 1. Lấy tham chiếu đến script Health (Nếu bạn chưa lấy trong Awake)
    Enemy_Health health = GetComponent<Enemy_Health>();

    // 2. KIỂM TRA QUAN TRỌNG:
    // Nếu quái đang bị Knockback, chúng ta THOÁT hàm luôn (return).
    // Điều này để vật lý (AddForce) tự làm việc, AI không được chạm vào vận tốc (velocity).
    if (health != null && health.IsKnockedBack()) 
    {
        return; 
    }

    // --- Dưới đây là logic di chuyển AI bình thường của bạn ---
    // Ví dụ: rb.velocity = movementDirection * moveSpeed;
}
    private void Update() {
        if (player == null || enemyPathfinding == null) return;

        // Calculate the actual center position of the capsules including the offset
        Vector2 detectionCenter = (Vector2)transform.position + capsuleOffset;

        // DETECTION (Using the offset position)
        bool isPlayerInStopRange = Physics2D.OverlapCapsule(detectionCenter, stopCapsuleSize, CapsuleDirection2D.Vertical, 0, playerLayer);
        bool isPlayerInAttackRange = Physics2D.OverlapCapsule(detectionCenter, attackCapsuleSize, CapsuleDirection2D.Vertical, 0, playerLayer);
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 1. ATTACK
        if (isPlayerInAttackRange && enemyAttack != null) {
            enemyAttack.TryToAttack(player);
        }

        // 2. MOVEMENT
        if (isPlayerInStopRange) {
            enemyPathfinding.StopMoving();
        } 
        else {
            enemyPathfinding.SetMoveSpeed(distanceToPlayer <= chaseRange ? chaseSpeed : everywhereSpeed);
            enemyPathfinding.MoveTo(player.position);
        }
    }

    // --- Visualization Code for Scene View ---
    private void OnDrawGizmosSelected() {
        Vector3 detectionCenter = transform.position + (Vector3)capsuleOffset;

        // Chase Circle (Usually stays centered on enemy)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Stop Capsule (Blue)
        DrawWireCapsule(detectionCenter, stopCapsuleSize, Color.blue);

        // Attack Capsule (Red)
        DrawWireCapsule(detectionCenter, attackCapsuleSize, Color.red);
    }

    private void DrawWireCapsule(Vector3 pos, Vector2 size, Color color) {
        Gizmos.color = color;
        float radius = size.x / 2;
        float height = size.y - size.x;
        if (height < 0) height = 0;

        Vector3 top = pos + Vector3.up * (height / 2);
        Vector3 bottom = pos + Vector3.down * (height / 2);

        Gizmos.DrawLine(top + Vector3.left * radius, bottom + Vector3.left * radius);
        Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
        DrawGizmoCircleSegment(top, radius, 0, 180);
        DrawGizmoCircleSegment(bottom, radius, 180, 360);
    }

    private void DrawGizmoCircleSegment(Vector3 center, float radius, float start, float end) {
        float step = 10f; Vector3 last = Vector3.zero;
        for (float a = start; a <= end; a += step) {
            Vector3 next = center + new Vector3(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad), 0) * radius;
            if (a > start) Gizmos.DrawLine(last, next);
            last = next;
        }
    }
}