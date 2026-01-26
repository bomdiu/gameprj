using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class ChargeEnemyAI : MonoBehaviour
{
    private enum State { Chasing, Telegraphing, Charging, Cooldown }
    [SerializeField] private State currentState = State.Chasing;

    [Header("Visuals")]
    public GameObject exclamationMark; 

    [Header("Detection Settings")]
    public float chargeRange = 5f;
    public LayerMask obstacleLayer;
    public string enemyLayerName = "Enemy"; 

    [Header("Damage Settings")]
    public float damage = 15f;

    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float chargeSpeed = 12f;
    public float chargeDuration = 0.8f;
    public float postChargeWait = 1f;

    [Header("Bounce Settings")]
    public float bounceForce = 5f; 
    public float bounceDuration = 0.1f; 

    [Header("Telegraph Line Settings")]
    public Color lineColor = Color.red;
    [Range(0, 1)] public float lineOpacity = 0.5f;
    public float lineExtendSpeed = 2f; 
    public float lineWidth = 0.05f;
    public float maxLineLength = 10f;
    public Vector2 lineOffset; 
    public float preChargeWait = 0.8f; 
    public float rotationSpeed = 5f;   
    public string sortingLayerName = "Shaow"; 

    [Header("Audio Settings")] // MỚI: Quản lý âm thanh Charge
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip telegraphSFX;   // Âm thanh khi bắt đầu báo hiệu
    [SerializeField] private AudioClip chargeDashSFX;  // Âm thanh khi lao đi

    private EnemyPathfinding motor;
    private Rigidbody2D rb; 
    private Animator anim; 
    private Transform player;
    private LineRenderer lineRenderer;
    private Enemy_Health health; 
    private Vector2 chargeDirection;
    private float stateTimer;

    private void Start() {
        motor = GetComponent<EnemyPathfinding>();
        rb = GetComponent<Rigidbody2D>(); 
        anim = GetComponent<Animator>(); 
        health = GetComponent<Enemy_Health>(); 
        
        // Tự động tìm AudioSource
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (rb != null) {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        lineRenderer = GetComponent<LineRenderer>();
        SetupLine();

        if (exclamationMark != null) exclamationMark.SetActive(false);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void SetupLine() {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.sortingLayerName = "Shadow";
        lineRenderer.sortingOrder = 0;
    }

    private void FixedUpdate() {
        if (health != null && health.IsKnockedBack()) return;

        if (currentState == State.Charging && rb != null && stateTimer > 0) {
            rb.velocity = chargeDirection * chargeSpeed;
        }
    }

    private void Update() {
        if (player == null || motor == null) return;
        if (health != null && health.IsKnockedBack()) return;

        switch (currentState) {
            case State.Chasing:
                HandleChasing();
                break;
            case State.Charging:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) {
                    StartCoroutine(HandleCleanStop()); 
                }
                break;
        }
    }

    private void HandleChasing() {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= chargeRange && CheckLineOfSight()) {
            StartCoroutine(TelegraphSequence());
        } else {
            Vector2 dir = (player.position - transform.position).normalized;
            motor.SetMoveDir(dir);
            motor.SetMoveSpeed(walkSpeed);
        }
    }

    private IEnumerator TelegraphSequence() {
        currentState = State.Telegraphing;
        motor.SetMoveDir(Vector2.zero); 
        
        if (exclamationMark != null) exclamationMark.SetActive(true);

        // MỚI: Phát âm thanh báo hiệu tụ lực
        if (audioSource != null && telegraphSFX != null) {
            audioSource.PlayOneShot(telegraphSFX);
        }

        lineRenderer.enabled = true;
        float progress = 0;
        float lockWindow = 0.1f; 
        float trackingDuration = Mathf.Max(0, preChargeWait - lockWindow); 

        Vector3 startPos = GetLineStartPos();
        chargeDirection = (player.position - startPos).normalized;

        while (progress < 1f) {
            if (health != null && health.IsKnockedBack()) { yield return null; continue; }
            progress += Time.deltaTime * lineExtendSpeed;
            UpdateLineVisuals(progress, lineOpacity);
            yield return null;
        }

        float trackingTimer = 0;
        while (trackingTimer < trackingDuration) {
            if (health != null && health.IsKnockedBack()) { yield return null; continue; }
            trackingTimer += Time.deltaTime;
            UpdateLineVisuals(1f, lineOpacity); 
            yield return null;
        }

        float fadeTimer = 0;
        while (fadeTimer < lockWindow) {
            fadeTimer += Time.deltaTime;
            float fadeAlpha = Mathf.Lerp(lineOpacity, 0, fadeTimer / lockWindow);
            UpdateLineVisuals(1f, fadeAlpha, true); 
            yield return null;
        }

        // 4. CHARGE
        if (exclamationMark != null) exclamationMark.SetActive(false);
        lineRenderer.enabled = false;
        if (anim != null) anim.SetTrigger("Charge"); 

        // MỚI: Phát âm thanh khi bắt đầu lao đi
        if (audioSource != null && chargeDashSFX != null) {
            audioSource.PlayOneShot(chargeDashSFX);
        }
        
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer(enemyLayerName), true);
        
        currentState = State.Charging;
        stateTimer = chargeDuration;
    }

    private void UpdateLineVisuals(float lengthProgress, float alpha, bool isLocked = false) {
        Vector3 startPos = GetLineStartPos();
        
        if (!isLocked) {
            motor.FlipToTarget(player.position.x);
            Vector2 targetDir = (player.position - startPos).normalized;
            chargeDirection = Vector3.Slerp(chargeDirection, targetDir, Time.deltaTime * rotationSpeed);
        }

        Vector3 targetPos = startPos + (Vector3)(chargeDirection * maxLineLength);
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, Vector3.Lerp(startPos, targetPos, lengthProgress));

        Color c = lineColor;
        c.a = alpha;
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
    }

    private Vector3 GetLineStartPos() {
        float flipMultiplier = (GetComponent<SpriteRenderer>().flipX) ? -1 : 1;
        return transform.position + new Vector3(lineOffset.x * flipMultiplier, lineOffset.y, 0);
    }

    private IEnumerator HandleCleanStop() {
        if (currentState == State.Cooldown) yield break;
        currentState = State.Cooldown;
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer(enemyLayerName), false);
        if (rb != null) rb.velocity = Vector2.zero; 
        motor.SetMoveSpeed(0); 
        yield return new WaitForSeconds(postChargeWait);
        motor.SetMoveSpeed(walkSpeed); 
        currentState = State.Chasing;
    }

    private IEnumerator HandleBounceStop(Vector2 collisionPoint) {
        if (currentState == State.Cooldown) yield break;
        currentState = State.Cooldown;
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer(enemyLayerName), false);
        if (rb != null) {
            Vector2 recoilDir = ((Vector2)transform.position - collisionPoint).normalized;
            rb.velocity = recoilDir * bounceForce;
            yield return new WaitForSeconds(bounceDuration);
            rb.velocity = Vector2.zero;
        }
        yield return new WaitForSeconds(postChargeWait);
        motor.SetMoveSpeed(walkSpeed);
        currentState = State.Chasing;
    }

    private bool CheckLineOfSight() {
        Vector2 dir = (player.position - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, player.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, obstacleLayer);
        return hit.collider == null;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (currentState != State.Charging) return;
        Vector2 hitPoint = collision.contacts[0].point;
        if (collision.gameObject.CompareTag("Player")) {
            PlayerStats stats = collision.gameObject.GetComponent<PlayerStats>();
            if (stats != null) stats.TakeDamage(damage);
            StartCoroutine(HandleBounceStop(hitPoint));
        } else if (((1 << collision.gameObject.layer) & obstacleLayer) != 0) {
            StartCoroutine(HandleBounceStop(hitPoint));
        }
    }

    private void OnDisable() {
        if (lineRenderer != null) lineRenderer.enabled = false;
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer(enemyLayerName), false);
    }
}