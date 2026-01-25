using UnityEngine;
using System.Collections;

public class Skill1 : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject fireballPrefab;
    public int energyCost = 1;
    public float cooldown = 1.5f;
    public float releaseDelay = 0.2f; 
    public float totalCastTime = 0.5f;

    [Header("Fire Build-Up Settings")]
    public GameObject fireBuildUpPrefab;
    public ParticleSystem gatheringParticles; 
    public Vector3 handOffset = new Vector3(0.5f, 0.2f, 0f); 
    private GameObject activeBuildUp;

    [Header("Melee Interruption")]
    public float postAttackDelay = 0.3f; 

    [Header("References")]
    private Player_Energy energy;
    private PlayerMovement movement;
    private PlayerCombat combat; 
    private Rigidbody2D rb;
    private Camera cam;
    private Animator anim;
    private Transform visualsTransform; 
    
    private float nextFireTime;
    private bool isCasting = false;

    void Awake()
    {
        energy = GetComponent<Player_Energy>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    void Start() { if (anim != null) visualsTransform = anim.transform; }

    void Update()
    {
        // Check if the skill is unlocked (Samplescene 2 completion)
        if (SkillUnlockManager.Instance != null && !SkillUnlockManager.Instance.skill1Unlocked) return;

        // CHANGED: KeyCode changed to E
        if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextFireTime && !isCasting)
        {
            if (combat != null && combat.isAttacking) return;
            if (combat != null && (Time.time - combat.lastAttackEndTime < postAttackDelay)) return;

            if (energy != null && energy.UseEnergy(energyCost)) StartCoroutine(CastRoutine());
        }
    }

    private IEnumerator CastRoutine()
    {
        isCasting = true; 
        nextFireTime = Time.time + cooldown;
        if (movement != null) movement.canMove = false;
        if (rb != null) rb.velocity = Vector2.zero; 

        FaceMouseCorrectly(); 
        Vector3 spawnPos = GetHandWorldPosition();

        // 1. START CHARGE EFFECTS
        if (fireBuildUpPrefab != null)
        {
            activeBuildUp = Instantiate(fireBuildUpPrefab, spawnPos, Quaternion.identity);
            activeBuildUp.transform.SetParent(visualsTransform);
        }

        // 2. PLAY GATHERING PARTICLES
        if (gatheringParticles != null)
        {
            gatheringParticles.transform.position = spawnPos;
            gatheringParticles.Play();
        }
        
        if (anim != null) anim.SetTrigger("CastFireball");

        yield return new WaitForSeconds(releaseDelay);
        
        // 3. STOP CHARGE EFFECTS
        if (activeBuildUp != null) Destroy(activeBuildUp);
        if (gatheringParticles != null) gatheringParticles.Stop();

        SpawnProjectile();

        yield return new WaitForSeconds(totalCastTime - releaseDelay);
        if (movement != null) movement.canMove = true;
        isCasting = false; 
    }

    private Vector3 GetHandWorldPosition()
    {
        if (visualsTransform == null) return transform.position;
        Vector3 offset = handOffset;
        offset.x *= -visualsTransform.localScale.x; 
        return transform.position + offset;
    }

    private void FaceMouseCorrectly()
    {
        if (visualsTransform == null) return;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        float flipX = mousePos.x > transform.position.x ? -1f : 1f; 
        visualsTransform.localScale = new Vector3(flipX, 1, 1);
    }

    private void SpawnProjectile()
    {
        if (fireballPrefab == null) return;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 dir = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Instantiate(fireballPrefab, GetHandWorldPosition(), Quaternion.Euler(0, 0, angle));
    }
}