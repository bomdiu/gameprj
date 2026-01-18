using UnityEngine;
using System.Collections;

public class Skill1 : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject fireballPrefab;
    public int energyCost = 1;
    public float cooldown = 1.5f;
    public float releaseDelay = 0.2f;

    [Header("Melee Interruption Settings")]
    [Tooltip("How long must the player wait after an attack ends before casting?")]
    public float postAttackDelay = 0.3f; 

    [Header("References")]
    private Player_Energy energy;
    private PlayerMovement movement;
    private PlayerCombat combat; 
    private Rigidbody2D rb;
    private Camera cam;
    private float nextFireTime;

    void Awake()
    {
        energy = GetComponent<Player_Energy>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextFireTime)
        {
            // 1. Prevent casting if currently swinging
            if (combat != null && combat.isAttacking) return;

            // 2. NEW: Check if enough time has passed since the last attack ended
            if (combat != null)
            {
                float timeSinceLastAttack = Time.time - combat.lastAttackEndTime;
                if (timeSinceLastAttack < postAttackDelay)
                {
                    Debug.Log("Waiting for attack recovery: " + (postAttackDelay - timeSinceLastAttack).ToString("F2") + "s remaining.");
                    return;
                }
            }

            // 3. Check energy script before allowing the cast
            if (energy != null && energy.UseEnergy(energyCost))
            {
                StartCoroutine(CastRoutine());
            }
        }
    }

    private IEnumerator CastRoutine()
    {
        nextFireTime = Time.time + cooldown;

        // Locks player movement and stops physics momentum
        if (movement != null) movement.canMove = false;
        if (rb != null) rb.velocity = Vector2.zero; 
        
        if (combat != null && combat.anim != null) 
            combat.anim.SetTrigger("CastFireball");

        yield return new WaitForSeconds(releaseDelay);

        SpawnProjectile();

        yield return new WaitForSeconds(0.1f);
        if (movement != null) movement.canMove = true;
    }

    private void SpawnProjectile()
    {
        if (fireballPrefab == null) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 dir = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Instantiate(fireballPrefab, transform.position, Quaternion.Euler(0, 0, angle));
    }
}