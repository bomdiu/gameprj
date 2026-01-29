using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PlayerCombat : MonoBehaviour
{
    [System.Serializable]
    public class AttackData {
        public string animName;
        public int damage;
        public float lungeDistance;
        public AnimationCurve lungeCurve; 
        public GameObject vfxPrefab;
        public float vfxOffset = 1.5f;
        public float vfxRotation = 0f;
        public float cooldownToNext = 0.15f;
    }

    [Header("Combo Configuration")]
    [SerializeField] private List<AttackData> attacks = new List<AttackData>();
    
    // --- COMPATIBILITY PROPERTIES ---
    [HideInInspector] public int damage1 { get => attacks.Count > 0 ? attacks[0].damage : 0; set { if(attacks.Count > 0) attacks[0].damage = value; } }
    [HideInInspector] public int damage2 { get => attacks.Count > 1 ? attacks[1].damage : 0; set { if(attacks.Count > 1) attacks[1].damage = value; } }
    [HideInInspector] public int damage3 { get => attacks.Count > 2 ? attacks[2].damage : 0; set { if(attacks.Count > 2) attacks[2].damage = value; } }
    
    public float lastAttackEndTime; 

    [Header("Rare Upgrade Stats")]
    public float critChance = 0.1f; 
    public float lifestealChance = 0.1f;
    public float lifestealPercent = 0.1f; 
    public int skillDamageBonus = 0;

    [Header("Settings")]
    [SerializeField] private float comboWindow = 0.8f;
    [SerializeField] private float bufferWindow = 0.25f;
    [SerializeField] private float lungeDuration = 0.12f;

    [Header("References")]
    [SerializeField] public Animator anim;
    [SerializeField] private Transform visualsTransform;
    [SerializeField] private Transform weaponPivot; 
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] slashSounds;

    private PlayerHealth playerHealth; 
    private Rigidbody2D rb;
    private Camera cam;

    private int comboStep = 0;
    private float lastBufferedTime;
    private bool hasBufferedInput = false;
    public bool isAttacking = false;

    private Coroutine attackCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>(); 
        cam = Camera.main;

        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        DisableAllHitboxes();
    }

    public void StartAttackMove() { }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PauseMenu.GameIsPaused) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            HandleInput();
        }

        if (!isAttacking)
        {
            TryExecuteBufferedAttack();
            if (comboStep > 0 && Time.time - lastAttackEndTime > comboWindow) comboStep = 0;
        }
    }

    private void HandleInput()
    {
        hasBufferedInput = true;
        lastBufferedTime = Time.time;
        if (!isAttacking) TryExecuteBufferedAttack();
    }

    private void TryExecuteBufferedAttack()
    {
        if (!hasBufferedInput) return;
        if (Time.time - lastBufferedTime > bufferWindow) { hasBufferedInput = false; return; }

        float currentCooldown = comboStep > 0 ? attacks[Mathf.Clamp(comboStep - 1, 0, attacks.Count-1)].cooldownToNext : 0;
        if (Time.time - lastAttackEndTime >= currentCooldown) PrepareAttack();
    }

    private void PrepareAttack()
    {
        if (attacks.Count == 0) return;

        hasBufferedInput = false;
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        bool inComboChain = (Time.time - lastAttackEndTime <= comboWindow);
        comboStep = (inComboChain && comboStep < attacks.Count) ? comboStep + 1 : 1;
        
        attackCoroutine = StartCoroutine(ExecuteAttackRoutine(attacks[comboStep - 1]));
    }

    private IEnumerator ExecuteAttackRoutine(AttackData attack)
    {
        isAttacking = true;
        if (movement != null) movement.canMove = false;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 dir = (mousePos - transform.position).normalized;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (weaponPivot != null) weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
        FlipCharacter(dir.x);

        PlaySlashSFX();

        if (anim != null) anim.Play(attack.animName, 0, 0f);

        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + (dir * attack.lungeDistance);
        float timer = 0;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation; 
        while (timer < lungeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / lungeDuration;
            float curveT = attack.lungeCurve != null ? attack.lungeCurve.Evaluate(t) : t;
            
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, curveT));
            yield return null;
        }

        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        
        yield return new WaitForSeconds(0.15f);
        if (isAttacking) EndAttackMove();
    }

    public void EndAttackMove()
    {
        if (!isAttacking) return;
        isAttacking = false;
        lastAttackEndTime = Time.time;
        DisableAllHitboxes();
        
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (movement != null) movement.canMove = true;
    }

    public void TriggerHitbox()
    {
        if (comboStep == 0 || attacks.Count < comboStep) return;
        AttackData currentAttack = attacks[comboStep - 1];

        if (currentAttack.vfxPrefab != null && weaponPivot != null)
        {
            Vector3 spawnPos = weaponPivot.position + (weaponPivot.right * currentAttack.vfxOffset);
            Quaternion spawnRot = weaponPivot.rotation * Quaternion.Euler(0, 0, currentAttack.vfxRotation);
            GameObject vfx = Instantiate(currentAttack.vfxPrefab, spawnPos, spawnRot);
            if (comboStep == 3) vfx.transform.localScale *= 1.5f;
        }

        // Logic updated to select the specific anchor for the current combo step
        if (weaponPivot != null)
        {
            int hitboxIndex = comboStep - 1; // 1st attack uses index 0, 2nd uses index 1, etc.
            if (weaponPivot.childCount > hitboxIndex)
            {
                GameObject currentHitbox = weaponPivot.GetChild(hitboxIndex).gameObject;
                currentHitbox.SetActive(true);
                
                DamageDealer dealer = currentHitbox.GetComponent<DamageDealer>();
                if (dealer != null) dealer.ResetHitList();
            }
        }
    }

    private void FlipCharacter(float xDir)
    {
        if (visualsTransform == null) return;
        if (xDir > 0.1f) visualsTransform.localScale = new Vector3(-1, 1, 1);
        else if (xDir < -0.1f) visualsTransform.localScale = new Vector3(1, 1, 1);
    }

    public int GetCurrentDamage(out bool isCrit)
    {
        if (attacks.Count < comboStep || comboStep == 0) { isCrit = false; return 0; }
        float totalDamage = attacks[comboStep - 1].damage;
        isCrit = false;
        if (Random.value < critChance) { totalDamage *= 1.5f; isCrit = true; }
        return Mathf.RoundToInt(totalDamage);
    }

    public int GetCurrentDamage() => GetCurrentDamage(out _);

    public void ApplyLifesteal(int damageDealt)
    {
        if (lifestealChance > 0 && Random.value < lifestealChance)
        {
            int healAmount = Mathf.RoundToInt(damageDealt * lifestealPercent);
            if (playerHealth != null) playerHealth.ChangeHealth(healAmount); 
        }
    }

    private void PlaySlashSFX()
    {
        if (audioSource != null && slashSounds != null && slashSounds.Length >= comboStep)
            audioSource.PlayOneShot(slashSounds[comboStep - 1]);
    }

    public void DisableAllHitboxes()
    {
        if (weaponPivot == null) return;

        // Logic updated to turn off ALL children anchors
        foreach (Transform child in weaponPivot)
        {
            child.gameObject.SetActive(false);
        }
    }
}