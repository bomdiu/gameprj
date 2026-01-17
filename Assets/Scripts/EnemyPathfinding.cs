using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    [SerializeField] private bool isFacingRightByDefault = true;
    
    private float moveSpeed; 
    private Rigidbody2D rb;
    private Vector2 moveDir; 
    private Vector2 currentVelocity; 
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Enemy_Health health;
    private Transform playerTransform;

    [Header("Static Obstacles (Trees/Walls)")]
    [SerializeField] private LayerMask staticLayer;
    [SerializeField] private float staticLookAhead = 3.0f; 
    [SerializeField] private float staticAvoidForce = 2.0f; 
    [SerializeField] private float staticDetectionRadius = 0.5f;

    [Header("Swarm Logic (Other Enemies)")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float swarmLookAhead = 1.0f;  
    [SerializeField] private float swarmAvoidForce = 1.2f; 
    [SerializeField] private float swarmDetectionRadius = 0.4f;

    [Header("General Settings")]
    [SerializeField] private float turnSmoothing = 0.15f; 

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>(); 
        health = GetComponent<Enemy_Health>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    private void FixedUpdate() 
    {
        // NEW: Stop processing physics if speed is 0 or less (prevents sliding/bouncing)
        if (moveSpeed <= 0 && (health == null || !health.IsKnockedBack())) 
        {
            return; 
        }

        if (health != null && health.IsKnockedBack()) 
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.fixedDeltaTime * 3f);
            UpdateAnimation();
            if (playerTransform != null) {
                float xToPlayer = playerTransform.position.x - transform.position.x;
                FlipSprite(xToPlayer);
            }
            return; 
        }

        if (moveDir == Vector2.zero) {
            rb.velocity = Vector2.SmoothDamp(rb.velocity, Vector2.zero, ref currentVelocity, turnSmoothing);
            UpdateAnimation();
            return;
        }

        Vector2 desiredVelocity = moveDir * moveSpeed;

        RaycastHit2D staticHit = Physics2D.CircleCast(transform.position, staticDetectionRadius, moveDir, staticLookAhead, staticLayer);
        if (staticHit.collider != null) {
            desiredVelocity += CalculateAvoidance(staticHit, staticLookAhead, staticAvoidForce);
        }

        RaycastHit2D swarmHit = Physics2D.CircleCast(transform.position, swarmDetectionRadius, moveDir, swarmLookAhead, enemyLayer);
        if (swarmHit.collider != null && swarmHit.collider.gameObject != gameObject) {
            desiredVelocity += CalculateAvoidance(swarmHit, swarmLookAhead, swarmAvoidForce);
        }

        desiredVelocity = desiredVelocity.normalized * moveSpeed;
        rb.velocity = Vector2.SmoothDamp(rb.velocity, desiredVelocity, ref currentVelocity, turnSmoothing);

        UpdateAnimation();
        FlipSprite(moveDir.x); 
    }

    public void SetMoveDir(Vector2 newDir) { 
        moveDir = newDir; 
    }

    public void FlipToTarget(float targetX) {
        float xDiff = targetX - transform.position.x;
        FlipSprite(xDiff);
    }

    private Vector2 CalculateAvoidance(RaycastHit2D hit, float lookDist, float force) {
        Vector2 avoidanceDirection = ((Vector2)transform.position - (Vector2)hit.collider.bounds.center).normalized;
        float distanceScale = 1.0f - (hit.distance / lookDist);
        return avoidanceDirection * force * moveSpeed * distanceScale;
    }

    private void UpdateAnimation() {
        if (anim == null) return;
        if (HasParameter("Speed", anim)) {
            anim.SetFloat("Speed", rb.velocity.magnitude);
        }
    }

    private bool HasParameter(string paramName, Animator animator) {
        foreach (AnimatorControllerParameter param in animator.parameters) {
            if (param.name == paramName) return true;
        }
        return false;
    }

    public void MoveTo(Vector2 targetPosition) {
        moveDir = (targetPosition - (Vector2)transform.position).normalized;
    }

    public void StopMoving() {
        moveDir = Vector2.zero;
        if (anim != null && HasParameter("Speed", anim)) anim.SetFloat("Speed", 0f);
    }

    public void SetMoveSpeed(float newSpeed) { moveSpeed = newSpeed; }

    private void FlipSprite(float xInput) {
        if (Mathf.Abs(xInput) < 0.1f) return;
        spriteRenderer.flipX = isFacingRightByDefault ? (xInput < 0) : (xInput > 0);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = new Color(1, 0, 0, 0.3f); 
        Gizmos.DrawWireSphere(transform.position + (Vector3)moveDir * staticLookAhead, staticDetectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)moveDir * staticLookAhead);

        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.3f); 
        Gizmos.DrawWireSphere(transform.position + (Vector3)moveDir * swarmLookAhead, swarmDetectionRadius);
    }
}