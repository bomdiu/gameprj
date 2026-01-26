using UnityEngine;
using System.Collections;

public class Skill2 : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject boomerangPrefab;
    public int energyCost = 2;
    public float cooldown = 4f;
    public float releaseDelay = 0.2f; 
    public float totalCastTime = 0.5f;

    [Header("Cast Effects")]
    public ParticleSystem gatheringParticles; 
    public Vector3 handOffset = new Vector3(0.5f, 0.2f, 0f); 

    [Header("Chain Stats")]
    public float flySpeed = 22f;
    public int damage = 15;
    public int maxChainHits = 5;
    public LayerMask enemyLayer;

    private Player_Energy energy;
    private PlayerMovement movement;
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
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    void Start() { if (anim != null) visualsTransform = anim.transform; }

    void Update()
    {
        // Unlock Check: Skill 2 unlocks after Samplescene 1
        if (SkillUnlockManager.Instance != null && !SkillUnlockManager.Instance.skill2Unlocked) return;

        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= nextFireTime && !isCasting)
        {
            if (energy != null && energy.UseEnergy(energyCost)) {
                StartCoroutine(CastRoutine());
                SkillUIController.Instance.StartCooldown(1, cooldown);
        }
    }
    }
    private IEnumerator CastRoutine()
    {
        isCasting = true; 
        nextFireTime = Time.time + cooldown;
        if (movement != null) movement.canMove = false;
        if (rb != null) rb.velocity = Vector2.zero; 

        FaceMouseCorrectly(); 

        if (gatheringParticles != null)
        {
            gatheringParticles.transform.position = GetHandWorldPosition();
            gatheringParticles.Play();
        }

        if (anim != null) anim.SetTrigger("CastFireball");

        yield return new WaitForSeconds(releaseDelay);
        
        if (gatheringParticles != null) gatheringParticles.Stop();

        ThrowChainBoomerang();

        yield return new WaitForSeconds(totalCastTime - releaseDelay);
        if (movement != null) movement.canMove = true;
        isCasting = false; 
    }

    private void ThrowChainBoomerang()
    {
        if (boomerangPrefab == null) return;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        GameObject boomerang = Instantiate(boomerangPrefab, transform.position, Quaternion.identity);
        
        BoomerangChain script = boomerang.GetComponent<BoomerangChain>();
        if (script != null)
        {
            script.Setup(flySpeed, transform, damage, enemyLayer, maxChainHits, mousePos);
        }
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
}