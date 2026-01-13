using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Components")]
    public Animator anim;
    [SerializeField] private Transform visualsTransform; 
    private Rigidbody2D rb;
    private Camera cam; // Reference to the main camera

    private Vector2 moveInput;
    private bool isDashing = false;
    private float dashTimeLeft;
    private float lastDash = -10f;

    [HideInInspector] public bool canMove = true; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main; // Initialize the camera
        
        transform.localScale = Vector3.one;

        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (visualsTransform == null && anim != null) visualsTransform = anim.transform;
    }

    void Update()
    {
        // 1. Always face the cursor regardless of movement
        FaceCursor();

        if (canMove && !isDashing)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput.Normalize();
        }
        else if (!isDashing)
        {
            moveInput = Vector2.zero;
        }

        if (anim != null)
        {
            anim.SetFloat("horizontal", Mathf.Abs(moveInput.x));
            anim.SetFloat("vertical", Mathf.Abs(moveInput.y));
        }

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDash + dashCooldown && canMove)
        {
            StartDash();
        }
    }

    private void FaceCursor()
    {
        if (visualsTransform == null) return;

        // Convert mouse position to world space
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // --- LOGIC FOR LEFT-FACING DEFAULT SPRITE ---
        // If mouse is to the RIGHT of the player, flip scale to -1
        if (mousePos.x > transform.position.x)
        {
            visualsTransform.localScale = new Vector3(-1, 1, 1);
        }
        // If mouse is to the LEFT of the player, reset scale to 1
        else
        {
            visualsTransform.localScale = new Vector3(1, 1, 1);
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            ApplyDashMovement();
        }
        else if (canMove) 
        {
            rb.velocity = moveInput * moveSpeed;
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDash = Time.time;
        // FaceCursor in Update will handle the flip during the dash
    }

    private void ApplyDashMovement()
    {
        rb.velocity = moveInput * dashSpeed;
        dashTimeLeft -= Time.fixedDeltaTime;
        if (dashTimeLeft <= 0)
        {
            isDashing = false;
            rb.velocity = Vector2.zero;
        }
    }
}