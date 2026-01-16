using UnityEngine;
using System.Collections;

public class Skill_BiteDash : MonoBehaviour
{
    [Header("Cài đặt")]
    public BossController boss;
    public BossSkillData skillData;

    // Các thành phần được tạo tự động
    private LineRenderer lineRend;
    private GameObject hitboxObj;
    private BoxCollider2D hitboxCol;

    private float currentCooldown = 0f;

    private void Awake()
    {
        // Tự động tạo LineRenderer và Hitbox khi game bắt đầu
        CreateLineRenderer();
        CreateHitbox();
    }

    private void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
        else 
        {
            if (boss.CanAttack())
            {
                ActivateSkill();
            }
        }
    }

    // --- PHẦN TẠO VISUAL (LINE RENDERER) ---
    void CreateLineRenderer()
    {
        GameObject lineObj = new GameObject("TelegraphLine");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;

        lineRend = lineObj.AddComponent<LineRenderer>();
        lineRend.useWorldSpace = true;
        lineRend.positionCount = 2;
        lineRend.sortingOrder = -1;
        
        if (skillData.lineMaterial != null)
            lineRend.material = skillData.lineMaterial;
        else
            lineRend.material = new Material(Shader.Find("Sprites/Default"));

        lineRend.enabled = false;
    }

    // --- PHẦN TẠO HITBOX ---
    void CreateHitbox()
    {
        // 1. Tạo GameObject con
        hitboxObj = new GameObject("DashHitbox");
        hitboxObj.transform.SetParent(transform);
        hitboxObj.transform.localPosition = Vector3.zero;

        // 2. Thêm BoxCollider2D (Trigger)
        hitboxCol = hitboxObj.AddComponent<BoxCollider2D>();
        hitboxCol.isTrigger = true; // Quan trọng: Trigger để đi xuyên qua nhưng vẫn detect
        
        // 3. Thêm script xử lý va chạm
        BossAttackHitbox handler = hitboxObj.AddComponent<BossAttackHitbox>();
        handler.Setup(this); // Gửi tham chiếu script này sang để nó biết đường báo tin

        hitboxObj.SetActive(false); // Mặc định tắt
    }

    // Hàm nhận tin báo từ Hitbox (Script BossAttackHitbox gọi cái này)
    public void OnHitPlayer(GameObject playerObj)
    {
        Debug.Log("CHÉM TRÚNG PLAYER! Gây sát thương: " + skillData.damage);
        // playerObj.GetComponent<PlayerHealth>().TakeDamage(skillData.damage);
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

        // 1. KHÓA HƯỚNG
        Vector2 lockedDirection = (boss.player.position - transform.position).normalized;
        boss.FacePlayer(); 
        boss.PlayAnim("biteCast");

        // 2. CHẠY TELEGRAPH
        yield return StartCoroutine(ShowLineTelegraph(lockedDirection));

        // 3. TẤN CÔNG (DASH)
        lineRend.enabled = false; // Tắt vạch đỏ
        
        // --- ĐỒNG BỘ HITBOX VỚI TELEGRAPH ---
        SyncHitboxToTelegraph(lockedDirection); 
        hitboxObj.SetActive(true); // Bật hitbox lên

        boss.PlayAnim("biteAttack");
        boss.rb.velocity = lockedDirection * skillData.dashSpeed; 

        yield return new WaitForSeconds(skillData.dashDuration);

        // 4. KẾT THÚC
        boss.rb.velocity = Vector2.zero;
        hitboxObj.SetActive(false); // Tắt hitbox đi

        boss.ChangeState(BossState.Recovering);
        boss.PlayAnim("Idle");
        
        yield return new WaitForSeconds(skillData.recoverTime);

        boss.ChangeState(BossState.Idle);
        currentCooldown = skillData.autoCooldown;
    }

    // Hàm xoay và chỉnh kích thước Hitbox
    void SyncHitboxToTelegraph(Vector2 direction)
    {
        // 1. Xoay Hitbox theo hướng dash (Giống hệt cách xoay LineRenderer/Telegraph cũ)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        hitboxObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 2. Chỉnh kích thước Hitbox
        // Size X: Độ dài của hitbox (bạn có thể cho nó to hơn body boss 1 chút, ví dụ 1.5f)
        // Size Y: Độ rộng -> LẤY TỪ SKILL DATA (Bằng đúng độ rộng vạch đỏ)
        hitboxCol.size = new Vector2(1.5f, skillData.width);

        // 3. Offset (Dời tâm)
        // Mặc định tâm hitbox trùng tâm boss. Nếu muốn hitbox nhô ra phía trước đầu boss, chỉnh offset X
        hitboxCol.offset = new Vector2(0.5f, 0); 
    }

    private IEnumerator ShowLineTelegraph(Vector2 direction)
    {
        lineRend.enabled = true;
        lineRend.startWidth = skillData.width;
        lineRend.endWidth = skillData.width;
        lineRend.startColor = skillData.telegraphColor;
        lineRend.endColor = skillData.telegraphColor;

        Vector3 startPos = transform.position; 
        startPos.z = 0; 

        float timer = 0f;
        while (timer < skillData.prepareTime)
        {
            timer += Time.deltaTime;
            float growthProgress = Mathf.Clamp01(timer / skillData.lineGrowthTime);
            float currentLength = Mathf.Lerp(0, skillData.maxLength, growthProgress);

            Vector3 endPos = startPos + (Vector3)(direction * currentLength);
            endPos.z = 0;

            lineRend.SetPosition(0, transform.position); 
            lineRend.SetPosition(1, endPos);

            yield return null;
        }
    }
}