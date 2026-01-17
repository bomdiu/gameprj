using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dashDistance = 5f; 
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Dash VFX Settings")]
    public GameObject dashEffectPrefab; 
    public Vector3 vfxOffset;           
    public float vfxDestroyTime = 0.5f; 

    [Header("Ghost Trail Settings")]
    public GameObject ghostPrefab;      
    public float ghostDelay = 0.03f;    
    public float ghostFadeTime = 0.3f;  
    public Color ghostColor = new Color(0.8f, 0.8f, 0.8f, 0.6f); 
    public Material ghostMaterial;      

    [Header("Components")]
    public Animator anim;
    [SerializeField] private Transform visualsTransform; 
    private Rigidbody2D rb;
    private Camera cam; 
    private SpriteRenderer playerSR; 

    private Vector2 moveInput;
    private bool isDashing = false;
    private float dashTimeLeft;
    private float lastDash = -10f;
    private Vector2 dashDir; 
    private float ghostTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main; 
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (visualsTransform == null && anim != null) visualsTransform = anim.transform;
        if (visualsTransform != null) playerSR = visualsTransform.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        FaceCursor();

        if (canMove && !isDashing)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput.Normalize();
        }
        else if (!isDashing) moveInput = Vector2.zero;

        if (anim != null)
        {
            anim.SetFloat("horizontal", Mathf.Abs(moveInput.x));
            anim.SetFloat("vertical", Mathf.Abs(moveInput.y));
            UpdateBackwardAnimation();
        }

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDash + dashCooldown && moveInput.sqrMagnitude > 0)
        {
            StartDash();
        }
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

    void FixedUpdate()
    {
        if (isDashing) ApplyDashMovement();
        else if (canMove) rb.velocity = moveInput * moveSpeed;
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDash = Time.time;
        dashDir = moveInput;
        ghostTimer = 0; 
        
        // --- NEW: IGNORE ENEMY COLLISION ---
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, true);
        }

        SpawnDashVFX();
    }

    private void SpawnGhost()
    {
        if (ghostPrefab == null || playerSR == null) return;
        GameObject ghostObj = Instantiate(ghostPrefab, visualsTransform.position, visualsTransform.rotation);
        DashGhost ghostScript = ghostObj.GetComponent<DashGhost>();
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

    private void ApplyDashMovement()
    {
        rb.velocity = dashDir * (dashDistance / dashDuration);
        
        ghostTimer -= Time.fixedDeltaTime;
        if (ghostTimer <= 0) { SpawnGhost(); ghostTimer = ghostDelay; }

        dashTimeLeft -= Time.fixedDeltaTime;
        if (dashTimeLeft <= 0)
        {
            isDashing = false;
            rb.velocity = Vector2.zero;

            // --- NEW: RE-ENABLE ENEMY COLLISION ---
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer != -1)
            {
                Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, false);
            }
        }
    }

    [HideInInspector] public bool canMove = true; 
}