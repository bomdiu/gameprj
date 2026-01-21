using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Skill_BiteDash : MonoBehaviour
{
    [Header("Cài đặt")]
    public BossController boss;
    public BossSkillData skillData;

    [Header("Cấu hình Va chạm")]
    // Kéo Collider chính của Boss (cái nằm trên người Boss để nhận damage/va chạm tường) vào đây
    public Collider2D bossMainCollider; 

    // Các thành phần visual tự tạo
    private LineRenderer lineRend;
    private GameObject hitboxObj;
    private BoxCollider2D hitboxCol;

    public float currentCooldown = 0f;
    public bool IsReady => currentCooldown <= 0;

    // Danh sách lưu tạm những con quái đã bị tắt va chạm
    private List<Collider2D> ignoredMinions = new List<Collider2D>();

    private void Awake()
    {
        CreateLineRenderer();
        CreateHitbox();

        // Tự động tìm Collider trên người boss nếu quên kéo
        if (bossMainCollider == null)
            bossMainCollider = boss.GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    public void ActivateSkill()
    {
        StartCoroutine(ExecuteBiteDash());
    }

    private IEnumerator ExecuteBiteDash()
    {
        // === BẮT ĐẦU ===
        boss.ChangeState(BossState.Attacking);
        boss.rb.velocity = Vector2.zero;

        Vector2 lockedDirection = (boss.player.position - transform.position).normalized;
        boss.FacePlayer(); 
        boss.PlayAnim("biteCast");

        yield return StartCoroutine(ShowLineTelegraph(lockedDirection));

        // === GIAI ĐOẠN DASH (QUAN TRỌNG) ===
        lineRend.enabled = false;
        
        // 1. Tắt va chạm với Minions xung quanh
        IgnoreMinionCollision(true);

        // 2. Bật Hitbox gây damage
        SyncHitboxToTelegraph(lockedDirection); 
        hitboxObj.SetActive(true); 

        boss.PlayAnim("biteAttack");
        boss.rb.velocity = lockedDirection * skillData.dashSpeed; 

        yield return new WaitForSeconds(skillData.dashDuration);

        // === KẾT THÚC ===
        boss.rb.velocity = Vector2.zero;
        hitboxObj.SetActive(false); 

        // 3. Bật lại va chạm (tránh lỗi boss đi xuyên quái mãi mãi)
        IgnoreMinionCollision(false);

        boss.ChangeState(BossState.Recovering);
        boss.PlayAnim("Idle");
        
        yield return new WaitForSeconds(skillData.recoverTime);

        boss.ChangeState(BossState.Idle);
        currentCooldown = skillData.autoCooldown;
    }

    // --- HÀM XỬ LÝ ĐI XUYÊN QUÁI (MỚI) ---
    void IgnoreMinionCollision(bool ignore)
    {
        if (ignore)
        {
            // BƯỚC 1: Tìm tất cả collider xung quanh boss (bán kính quét rộng 1 chút, ví dụ 20 đơn vị)
            // Dùng LayerMask để chỉ quét layer Enemy cho nhẹ máy
            int enemyLayerMask = 1 << LayerMask.NameToLayer("Enemy");
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 20f, enemyLayerMask);

            ignoredMinions.Clear(); // Xóa danh sách cũ

            foreach (var col in hits)
            {
                // Chỉ tắt va chạm nếu đó KHÔNG PHẢI là boss (vì boss cũng là layer Enemy)
                if (col != bossMainCollider && !col.isTrigger) 
                {
                    // Tắt va chạm giữa Boss và con Minion này
                    Physics2D.IgnoreCollision(bossMainCollider, col, true);
                    
                    // Lưu vào danh sách để tí nữa bật lại
                    ignoredMinions.Add(col);
                }
            }
        }
        else
        {
            // BƯỚC 2: Bật lại va chạm cho những con đã lưu
            foreach (var col in ignoredMinions)
            {
                // Kiểm tra null (đề phòng con minion bị chết/destroy trong lúc boss đang dash)
                if (col != null)
                {
                    Physics2D.IgnoreCollision(bossMainCollider, col, false);
                }
            }
            ignoredMinions.Clear();
        }
    }

    // Đề phòng Boss chết hoặc bị tắt khi đang Dash -> Reset lại va chạm
    private void OnDisable()
    {
        IgnoreMinionCollision(false);
    }

    // --- CÁC HÀM VISUAL ---
    
    void CreateLineRenderer() {
        GameObject lineObj = new GameObject("TelegraphLine");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;
        lineRend = lineObj.AddComponent<LineRenderer>();
        lineRend.useWorldSpace = true; lineRend.positionCount = 2; lineRend.sortingOrder = -1;
        if (skillData.lineMaterial != null) lineRend.material = skillData.lineMaterial;
        else lineRend.material = new Material(Shader.Find("Sprites/Default"));
        lineRend.enabled = false;
    }

    void CreateHitbox() {
        hitboxObj = new GameObject("DashHitbox");
        hitboxObj.transform.SetParent(transform);
        hitboxObj.transform.localPosition = Vector3.zero;
        hitboxCol = hitboxObj.AddComponent<BoxCollider2D>();
        hitboxCol.isTrigger = true; 
        BossAttackHitbox handler = hitboxObj.AddComponent<BossAttackHitbox>();
        handler.Setup(this); 
        hitboxObj.SetActive(false); 
    }

    // --- NEW: DAMAGE LOGIC HERE ---
    public void OnHitPlayer(GameObject playerObj) {
        // Find the PlayerStats component on the player object and apply damage
        PlayerStats stats = playerObj.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(skillData.damage);
            // Optional: Apply knockback or other effects here if needed
        }
    }
    // ------------------------------

    void SyncHitboxToTelegraph(Vector2 direction) {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        hitboxObj.transform.rotation = Quaternion.Euler(0, 0, angle);
        hitboxCol.size = new Vector2(1.5f, skillData.width);
        hitboxCol.offset = new Vector2(0.5f, 0); 
    }

    private IEnumerator ShowLineTelegraph(Vector2 direction) {
        lineRend.enabled = true;
        lineRend.startWidth = skillData.width; lineRend.endWidth = skillData.width;
        lineRend.startColor = skillData.telegraphColor; lineRend.endColor = skillData.telegraphColor;
        Vector3 startPos = transform.position; startPos.z = 0; 
        float timer = 0f;
        while (timer < skillData.prepareTime) {
            timer += Time.deltaTime;
            float growthProgress = Mathf.Clamp01(timer / skillData.lineGrowthTime);
            float currentLength = Mathf.Lerp(0, skillData.maxLength, growthProgress);
            Vector3 endPos = startPos + (Vector3)(direction * currentLength); endPos.z = 0;
            lineRend.SetPosition(0, transform.position); lineRend.SetPosition(1, endPos);
            yield return null;
        }
    }
}