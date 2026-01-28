using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float speedMultiplier = 1f; 
    
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Ghost Trail & VFX")]
    public GameObject dashEffectPrefab;
    public Vector3 vfxOffset;
    public float vfxDestroyTime = 0.5f;
    public GameObject ghostPrefab;
    public float ghostDelay = 0.03f;
    public float ghostFadeTime = 0.3f;
    public Color ghostColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
    public Material ghostMaterial;

    [Header("Audio & Components")]
    public AudioSource audioSource;
    public AudioClip dashSFX;
    public Animator anim;
    [SerializeField] private Transform visualsTransform;
    
    private Rigidbody2D rb;
    private Camera cam;
    private SpriteRenderer playerSR;
    private PlayerCombat combat; 

    private Vector2 moveInput;
    private bool isDashing = false;
    private float dashTimeLeft;
    private float lastDash = -10f;
    private Vector2 dashDir;
    private float ghostTimer;
    
    [HideInInspector] public bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        combat = GetComponent<PlayerCombat>();
        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (visualsTransform != null) playerSR = visualsTransform.GetComponent<SpriteRenderer>();

        // Link to your stats manager
        if (StatsManager.Instance != null)
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            StatsManager.Instance.ApplyStatsToPlayer(this, health, combat);
        }
    }

    void Update()
    {
        // We always capture raw input to know where to dash even if canMove is false
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 rawInput = new Vector2(h, v).normalized;

        if (canMove && !isDashing) 
        {
            moveInput = rawInput;
            FaceCursor(); 
        }
        else if (!isDashing) 
        {
            moveInput = Vector2.zero;
        }

        // --- DASH & DASH CANCEL LOGIC ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool offCooldown = Time.time >= lastDash + dashCooldown;
            bool hasInput = rawInput.sqrMagnitude > 0;

            if (offCooldown && hasInput)
            {
                // If we are currently attacking, cancel it to dash immediately
                if (combat != null && combat.isAttacking) 
                {
                    CancelAttack();
                }
                
                dashDir = rawInput;
                StartDash();
            }
        }

        if (anim != null)
        {
            anim.SetFloat("horizontal", Mathf.Abs(moveInput.x));
            anim.SetFloat("vertical", Mathf.Abs(moveInput.y));
            UpdateBackwardAnimation();
        }
    }

    private void CancelAttack()
    {
        // This stops the combat coroutines and resets state
        combat.StopAllCoroutines(); 
        combat.EndAttackMove();
    }

    private void StartDash()
    {
        isDashing = true;
        canMove = false;
        dashTimeLeft = dashDuration;
        lastDash = Time.time;
        ghostTimer = 0;

        if (audioSource != null && dashSFX != null) audioSource.PlayOneShot(dashSFX);
        
        // Ignore enemy collisions during the dash
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, true);

        SpawnDashVFX();
    }

    void FixedUpdate()
    {
        if (isDashing) ApplyDashMovement();
        else if (canMove) rb.velocity = moveInput * (moveSpeed * speedMultiplier);
    }

    private void ApplyDashMovement()
    {
        rb.velocity = dashDir * (dashDistance / dashDuration);
        
        ghostTimer -= Time.fixedDeltaTime;
        if (ghostTimer <= 0) { SpawnGhost(); ghostTimer = ghostDelay; }
        
        dashTimeLeft -= Time.fixedDeltaTime;
        if (dashTimeLeft <= 0) EndDash();
    }

    private void EndDash()
    {
        isDashing = false;
        canMove = true;
        rb.velocity = Vector2.zero;
        
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, false);
    }

    private void FaceCursor()
    {
        if (visualsTransform == null) return;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        visualsTransform.localScale = new Vector3(mousePos.x > transform.position.x ? -1 : 1, 1, 1);
    }

    private void UpdateBackwardAnimation()
    {
        if (moveInput.magnitude < 0.1f) { anim.SetBool("isBackward", false); return; }
        bool facingRight = (visualsTransform.localScale.x < 0);
        bool backward = (facingRight && moveInput.x < -0.1f) || (!facingRight && moveInput.x > 0.1f);
        anim.SetBool("isBackward", backward);
    }

    private void SpawnGhost()
    {
        if (ghostPrefab == null || playerSR == null) return;
        GameObject ghostObj = Instantiate(ghostPrefab, visualsTransform.position, visualsTransform.rotation);
        DashGhost ghostScript = ghostObj.GetComponent<DashGhost>();
        if (ghostScript != null)
            ghostScript.Init(playerSR.sprite, visualsTransform.localScale, ghostColor, ghostFadeTime, ghostMaterial);
    }

    private void SpawnDashVFX()
    {
        if (dashEffectPrefab == null) return;
        float flipX = (visualsTransform.localScale.x < 0) ? -1 : 1;
        Vector3 spawnPos = transform.position + new Vector3(vfxOffset.x * flipX, vfxOffset.y, vfxOffset.z);
        GameObject vfx = Instantiate(dashEffectPrefab, spawnPos, Quaternion.identity);
        vfx.transform.localScale = new Vector3(vfx.transform.localScale.x * flipX, vfx.transform.localScale.y, 1);
        Destroy(vfx, vfxDestroyTime);
    }
}