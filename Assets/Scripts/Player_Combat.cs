using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combo Damage")]
    [SerializeField] public int damage1 = 10;
    [SerializeField] public int damage2 = 12;
    [SerializeField] public int damage3 = 25;

    [Header("Combo Timing")]
    [SerializeField] private float cooldown1to2 = 0.12f;
    [SerializeField] private float cooldown2to3 = 0.15f;
    [SerializeField] private float cooldown3to1 = 0.4f;
    [SerializeField] private float comboWindow = 0.8f;

    [Header("Input Buffering")]
    [Tooltip("How close to the end of an animation can the player 'queue' the next hit?")]
    [SerializeField] private float bufferWindow = 0.25f;
    private bool hasBufferedInput = false;
    private float lastBufferedTime;

    [Header("Lunge Settings")]
    [SerializeField] private float lungeDistance = 3.0f;
    [SerializeField] private float finalLungeDistance = 5.0f;
    [SerializeField] private float lungeDuration = 0.12f;

    [Header("Hitbox Anchors")]
    [SerializeField] private GameObject hitbox1;
    [SerializeField] private GameObject hitbox2;
    [SerializeField] private GameObject hitbox3;

    [Header("VFX Prefabs (Right Facing)")]
    [SerializeField] private GameObject splashRight1;
    [SerializeField] private GameObject splashRight2;
    [SerializeField] private GameObject splashRight3;

    [Header("VFX Prefabs (Left Facing)")]
    [SerializeField] private GameObject splashLeft1;
    [SerializeField] private GameObject splashLeft2;
    [SerializeField] private GameObject splashLeft3;

    [Header("VFX Offset (Right Facing)")]
    [SerializeField] private float offsetRight1 = 1.5f;
    [SerializeField] private float offsetRight2 = 1.5f;
    [SerializeField] private float offsetRight3 = 1.5f;

    [Header("VFX Offset (Left Facing)")]
    [SerializeField] private float offsetLeft1 = 1.5f;
    [SerializeField] private float offsetLeft2 = 1.5f;
    [SerializeField] private float offsetLeft3 = 1.5f;

    [Header("VFX Rotation (Right Facing)")]
    [SerializeField] private float rotationRight1 = 0f;
    [SerializeField] private float rotationRight2 = 0f;
    [SerializeField] private float rotationRight3 = 0f;

    [Header("VFX Rotation (Left Facing)")]
    [SerializeField] private float rotationLeft1 = 0f;
    [SerializeField] private float rotationLeft2 = 0f;
    [SerializeField] private float rotationLeft3 = 0f;

    [Header("References")]
    [SerializeField] public Animator anim;
    [SerializeField] private Transform visualsTransform;
    [SerializeField] private PlayerMovement movement;
    private Rigidbody2D rb;
    private Camera cam;

    private int comboStep = 0;
    private float lastAttackStartTime;
    public float lastAttackEndTime;
    public bool isAttacking = false;
    private bool isFacingRight = true;
    private Coroutine lungeCoroutine;
    private Coroutine recoveryCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        transform.localScale = Vector3.one;

        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (movement == null) movement = GetComponent<PlayerMovement>();

        DisableAllHitboxes();
    }

    public void StartAttackMove() { }

    public int GetCurrentDamage()
    {
        if (comboStep == 1) return damage1;
        if (comboStep == 2) return damage2;
        return damage3;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PauseMenu.GameIsPaused) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            HandleInput();
        }

        if (isAttacking && Time.time - lastAttackStartTime > 0.05f)
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Idle") || state.IsName("Walk")) EndAttackMove();
        }

        if (!isAttacking)
        {
            TryExecuteBufferedAttack();

            if (comboStep > 0 && Time.time - lastAttackEndTime > comboWindow)
            {
                comboStep = 0;
            }
        }
    }

    private void HandleInput()
    {
        hasBufferedInput = true;
        lastBufferedTime = Time.time;

        if (!isAttacking)
        {
            TryExecuteBufferedAttack();
        }
    }

    private void TryExecuteBufferedAttack()
    {
        if (!hasBufferedInput) return;

        if (Time.time - lastBufferedTime > bufferWindow)
        {
            hasBufferedInput = false;
            return;
        }

        float currentCooldown = GetCurrentCooldown();
        if (Time.time - lastAttackEndTime >= currentCooldown)
        {
            PrepareAttack();
        }
    }

    private float GetCurrentCooldown()
    {
        if (comboStep == 1) return cooldown1to2;
        if (comboStep == 2) return cooldown2to3;
        return cooldown3to1;
    }

    private void PrepareAttack()
    {
        hasBufferedInput = false;

        if (lungeCoroutine != null) StopCoroutine(lungeCoroutine);
        if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine);

        bool inComboChain = (Time.time - lastAttackEndTime <= comboWindow);
        comboStep = (inComboChain && comboStep < 3) ? comboStep + 1 : 1;

        ExecuteAttack();
    }

    private void ExecuteAttack()
    {
        isAttacking = true;
        lastAttackStartTime = Time.time;
        if (movement != null) movement.canMove = false;

        rb.velocity = Vector2.zero;
        
        // --- FIX START: Lock Position ---
        // Freezing All ensures the physics engine cannot push the player
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        // --- FIX END ---

        lungeCoroutine = StartCoroutine(PerformLunge());
    }

    private IEnumerator PerformLunge()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 dir = (mousePos - transform.position).normalized;

        if (comboStep == 1)
        {
            FlipCharacter(dir.x);
        }

        RotateCurrentHitbox(dir);

        anim.Play("Attack" + comboStep, 0, 0f);

        float dist = (comboStep == 3) ? finalLungeDistance : lungeDistance;
        float speed = dist / lungeDuration;

        float timer = 0;
        
        // Note: Even though constraints are frozen, we can still set velocity manually
        // However, to ensure the lunge works while constraints are technically 'Frozen' for collision
        // we might need to rely on the fact that 'velocity' assignment overrides constraints momentarily in some versions,
        // BUT simpler: Unfreeze X/Y for the lunge, then refreeze.
        
        // Let's modify the Freeze logic slightly to allow the lunge:
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Allow moving for the lunge itself

        while (timer < lungeDuration)
        {
            rb.velocity = dir * speed;
            timer += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        
        // --- RE-FREEZE after lunge is done but before recovery is over ---
        // This prevents pushing during the recovery animation
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        
        lungeCoroutine = null;
    }

    private void RotateCurrentHitbox(Vector2 dir)
    {
        GameObject current = GetCurrentHitboxAnchor();
        if (current == null) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        current.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private GameObject GetCurrentHitboxAnchor()
    {
        if (comboStep == 1) return hitbox1;
        if (comboStep == 2) return hitbox2;
        if (comboStep == 3) return hitbox3;
        return null;
    }

    private void FlipCharacter(float xDir)
    {
        if (visualsTransform == null) return;

        if (xDir > 0.1f)
        {
            visualsTransform.localScale = new Vector3(-1, 1, 1);
            isFacingRight = true;
        }
        else if (xDir < -0.1f)
        {
            visualsTransform.localScale = new Vector3(1, 1, 1);
            isFacingRight = false;
        }
    }

    public void EndAttackMove()
    {
        if (!isAttacking) return;
        isAttacking = false;
        lastAttackEndTime = Time.time;

        if (lungeCoroutine != null) StopCoroutine(lungeCoroutine);
        rb.velocity = Vector2.zero;

        DisableAllHitboxes();

        if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine);
        recoveryCoroutine = StartCoroutine(RecoveryRoutine());
    }

    private IEnumerator RecoveryRoutine()
    {
        float waitTime = (movement != null) ? movement.directionLockTime : 0.1f;
        
        // Ensure we are still frozen during the wait
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        yield return new WaitForSeconds(waitTime);

        // --- FIX END: Unlock Position ---
        // Revert to just freezing rotation (allowing movement)
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        if (movement != null && !isAttacking) movement.canMove = true;
    }

    public void TriggerHitbox()
    {
        DisableAllHitboxes();
        GameObject current = GetCurrentHitboxAnchor();

        if (current != null)
        {
            bool aimingRight = isFacingRight;

            GameObject prefabToSpawn = aimingRight ? GetRightPrefab() : GetLeftPrefab();

            if (prefabToSpawn != null)
            {
                float offset = aimingRight ? GetRightOffset() : GetLeftOffset();
                float rotation = aimingRight ? GetRightRotation() : GetLeftRotation();

                Vector3 spawnPosition = current.transform.position + (current.transform.right * offset);
                Quaternion spawnRotation = current.transform.rotation * Quaternion.Euler(0, 0, rotation);

                GameObject vfx = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);

                if (comboStep == 3)
                {
                    vfx.transform.localScale *= 1.5f;
                }
            }

            if (current.transform.childCount > 0)
            {
                GameObject child = current.transform.GetChild(0).gameObject;
                child.SetActive(true);

                DamageDealer dealer = child.GetComponent<DamageDealer>();
                if (dealer != null) dealer.ResetHitList();
            }
        }
    }

    private GameObject GetRightPrefab()
    {
        if (comboStep == 1) return splashRight1;
        if (comboStep == 2) return splashRight2;
        return splashRight3;
    }

    private GameObject GetLeftPrefab()
    {
        if (comboStep == 1) return splashLeft1;
        if (comboStep == 2) return splashLeft2;
        return splashLeft3;
    }

    private float GetRightOffset()
    {
        if (comboStep == 1) return offsetRight1;
        if (comboStep == 2) return offsetRight2;
        return offsetRight3;
    }

    private float GetLeftOffset()
    {
        if (comboStep == 1) return offsetLeft1;
        if (comboStep == 2) return offsetLeft2;
        return offsetLeft3;
    }

    private float GetRightRotation()
    {
        if (comboStep == 1) return rotationRight1;
        if (comboStep == 2) return rotationRight2;
        return rotationRight3;
    }

    private float GetLeftRotation()
    {
        if (comboStep == 1) return rotationLeft1;
        if (comboStep == 2) return rotationLeft2;
        return rotationLeft3;
    }

    public void DisableAllHitboxes()
    {
        if (hitbox1 != null && hitbox1.transform.childCount > 0) hitbox1.transform.GetChild(0).gameObject.SetActive(false);
        if (hitbox2 != null && hitbox2.transform.childCount > 0) hitbox2.transform.GetChild(0).gameObject.SetActive(false);
        if (hitbox3 != null && hitbox3.transform.childCount > 0) hitbox3.transform.GetChild(0).gameObject.SetActive(false);
    }
}