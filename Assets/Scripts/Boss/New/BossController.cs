using UnityEngine;

public enum BossState 
{ 
    Idle,       
    Attacking,  
    Recovering,
    Intro
}

public class BossController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator anim;
    public Transform player;
    public SpriteRenderer spriteRenderer;

    [Header("State Info")]
    public BossState currentState = BossState.Idle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (player == null) 
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        // Luôn quay mặt về phía player nếu KHÔNG PHẢI đang tấn công (đang Dash thì không quay lung tung)
        if (currentState != BossState.Attacking)
        {
            FacePlayer();
        }
    }

    public void ChangeState(BossState newState)
    {
        currentState = newState;
    }

    public bool CanAttack()
    {
        return currentState == BossState.Idle;
    }

    // Hàm xử lý Flip (Lật trái phải)
    public void FacePlayer()
    {
        if (player == null) return;

        // Nếu player ở bên trái -> flipX = true (nhìn trái)
        // Nếu player ở bên phải -> flipX = false (nhìn phải)
        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true; // Lật hình sang trái
        }
        else
        {
            spriteRenderer.flipX = false; // Hình gốc nhìn sang phải
        }
    }

    public void PlayAnim(string triggerName)
    {
        anim.SetTrigger(triggerName);
    }
}