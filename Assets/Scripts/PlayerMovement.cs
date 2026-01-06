using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    public Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private bool isDashing = false;
    private float dashTimeLeft;
    private float lastDash = -10f;

    public Player_Combat player_Combat;

    // THÊM DÒNG NÀY
    private Player_SweepSkill sweepSkill;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // LẤY THAM CHIẾU SKILL
        sweepSkill = GetComponent<Player_SweepSkill>();
    }

    void Update()
    {
        // Attack input
        if (Input.GetMouseButtonDown(0))
        {
            player_Combat.Attack();
        }

        // Movement input (nếu không dash & không recoil)
        if (!isDashing && (sweepSkill == null || !sweepSkill.isRecoiling))
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput.Normalize();

            // Flip sprite
            if (moveInput.x < 0) spriteRenderer.flipX = false;
            if (moveInput.x > 0) spriteRenderer.flipX = true;

            // Update animator
            anim.SetFloat("horizontal", Mathf.Abs(moveInput.x));
            anim.SetFloat("vertical", Mathf.Abs(moveInput.y));
        }

        // Dash (Space)
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDash + dashCooldown)
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            lastDash = Time.time;
        }
    }

    void FixedUpdate()
    {
        // NẾU ĐANG RECOIL → KHÔNG ĐƯỢC GHI ĐÈ VELOCITY
        if (sweepSkill != null && sweepSkill.isRecoiling)
        {
            return;
        }

        if (isDashing)
        {
            rb.velocity = moveInput * dashSpeed;

            dashTimeLeft -= Time.fixedDeltaTime;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            rb.velocity = moveInput * moveSpeed;
        }
    }
}
