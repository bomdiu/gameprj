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

    [Header("Static Obstacles (Trees/Walls)")]
    [SerializeField] private LayerMask staticLayer;
    [SerializeField] private float staticLookAhead = 3.0f; 
    [SerializeField] private float staticAvoidForce = 2.0f; 

    [Header("Swarm Logic (Other Enemies)")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float swarmLookAhead = 1.0f;  
    [SerializeField] private float swarmAvoidForce = 1.2f; 

    [Header("General Settings")]
    [SerializeField] private float detectionRadius = 0.4f; 
    [SerializeField] private float turnSmoothing = 0.15f; 

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>(); 
        health = GetComponent<Enemy_Health>();
    }

  private void FixedUpdate() 
{
    // 1. THE LOCK
    if (health != null && health.IsKnockedBack()) 
    {
        // Slowly bleed off velocity so they don't snap to a stop
        rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.fixedDeltaTime * 3f);
        UpdateAnimation();
        return; 
    }

    // 2. IDLE / STOPPING LOGIC
    if (moveDir == Vector2.zero) {
        rb.velocity = Vector2.SmoothDamp(rb.velocity, Vector2.zero, ref currentVelocity, turnSmoothing);
        UpdateAnimation();
        // If we aren't moving, DON'T flip. This prevents the "last frame" flip.
        return;
    }

    // 3. MOVEMENT LOGIC
    Vector2 desiredVelocity = moveDir * moveSpeed;
    // ... (Your existing Raycast logic) ...

    desiredVelocity = desiredVelocity.normalized * moveSpeed;
    rb.velocity = Vector2.SmoothDamp(rb.velocity, desiredVelocity, ref currentVelocity, turnSmoothing);

    // 4. VISUALS
    UpdateAnimation();

    // ONLY flip if the moveDir is actually pointing somewhere.
    // This ignores the physics-driven velocity and only looks at AI intent.
    FlipSprite(moveDir.x); 
}

    private Vector2 CalculateAvoidance(RaycastHit2D hit, float lookDist, float force) {
        Vector2 avoidanceDirection = ((Vector2)transform.position - (Vector2)hit.collider.bounds.center).normalized;
        float distanceScale = 1.0f - (hit.distance / lookDist);
        return avoidanceDirection * force * moveSpeed * distanceScale;
    }

    private void UpdateAnimation() {
        if (anim != null) anim.SetFloat("Speed", rb.velocity.magnitude);
    }

    public void MoveTo(Vector2 targetPosition) {
        moveDir = (targetPosition - (Vector2)transform.position).normalized;
    }

    public void StopMoving() {
        moveDir = Vector2.zero;
        if (anim != null) anim.SetFloat("Speed", 0f);
    }

    public void SetMoveSpeed(float newSpeed) { moveSpeed = newSpeed; }

    private void FlipSprite(float xVelocity) {
        // Higher threshold to prevent flickering when stopping
        if (Mathf.Abs(xVelocity) < 0.2f) return;
        
        spriteRenderer.flipX = isFacingRightByDefault ? (xVelocity < 0) : (xVelocity > 0);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green; 
        Gizmos.DrawRay(transform.position, moveDir * staticLookAhead);
        Gizmos.color = Color.yellow; 
        Gizmos.DrawRay(transform.position, moveDir * swarmLookAhead);
    }
}