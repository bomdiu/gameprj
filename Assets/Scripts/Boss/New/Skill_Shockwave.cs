using UnityEngine;
using System.Collections;

public class Skill_Shockwave : MonoBehaviour
{
    [Header("Cài đặt")]
    public BossController boss;
    public ShockwaveSkillData skillData;
    public GameObject shockwavePrefab; 

    private LineRenderer telegraphLine; 
    public float currentCooldown = 0f;
    // Thêm dòng này vào bất kỳ chỗ nào trong class skill
    public bool IsReady => currentCooldown <= 0;

    private void Awake()
    {
        CreateTelegraphLine();
    }

    private void Update()
    {
        if (currentCooldown > 0)
            currentCooldown -= Time.deltaTime;
        else if (boss.CanAttack())
            ActivateSkill();
    }

    // (Giữ nguyên hàm CreateTelegraphLine...)
    void CreateTelegraphLine()
    {
        GameObject tObj = new GameObject("ShockwaveTelegraph");
        tObj.transform.SetParent(transform);
        tObj.transform.localPosition = Vector3.zero;

        telegraphLine = tObj.AddComponent<LineRenderer>();
        telegraphLine.useWorldSpace = false;
        telegraphLine.loop = true;
        telegraphLine.positionCount = 60;
        telegraphLine.startWidth = 0.05f;
        telegraphLine.endWidth = 0.05f;
        telegraphLine.material = new Material(Shader.Find("Sprites/Default"));
        telegraphLine.startColor = new Color(1, 1, 1, 0.3f); 
        telegraphLine.endColor = new Color(1, 1, 1, 0.3f);
        telegraphLine.enabled = false; 
    }

    public void ActivateSkill()
    {
        StartCoroutine(ExecuteShockwave());
    }

    private IEnumerator ExecuteShockwave()
    {
        // === GIAI ĐOẠN 1: CAST (TÍCH TỤ) ===
        boss.ChangeState(BossState.Attacking);
        boss.rb.velocity = Vector2.zero;
        
        boss.PlayAnim("ScreamCast"); 
        ShowTelegraph(true);

        // [RUNG 1] Rung nhẹ khi gồng
        StartCoroutine(ShakeSprite(skillData.castTime, skillData.castShakeIntensity));

        yield return new WaitForSeconds(skillData.castTime);


        // === GIAI ĐOẠN 2: RELEASE (HÉT) ===
        ShowTelegraph(false); 
        boss.PlayAnim("ScreamRelease"); 

        // Tính toán thời gian thực tế Boss sẽ hét
        // Nếu user set duration = 0 thì tính theo số wave * interval, ngược lại lấy duration user set
        float actualScreamTime = (skillData.screamDuration > 0) 
            ? skillData.screamDuration 
            : (skillData.waveCount * skillData.spawnInterval) + 0.5f; // +0.5s dư ra cho chắc

        // [RUNG 2] Rung mạnh khi hét (theo thời gian thực tế đã tính)
        StartCoroutine(ShakeSprite(actualScreamTime, skillData.releaseShakeIntensity));

        float attackStartTime = Time.time;

        // Vòng lặp bắn sóng
        for (int i = 0; i < skillData.waveCount; i++)
        {
            GameObject waveObj = Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
            ShockwaveEffect waveScript = waveObj.GetComponent<ShockwaveEffect>();
            if (waveScript != null) waveScript.Setup(skillData);

            if (i < skillData.waveCount - 1)
                yield return new WaitForSeconds(skillData.spawnInterval);
        }

        // Đợi nốt thời gian còn lại của Scream Duration
        float timeElapsed = Time.time - attackStartTime; 
        float remainingTime = skillData.screamDuration - timeElapsed;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);


        // === GIAI ĐOẠN 3: RECOVER ===
        boss.ChangeState(BossState.Recovering);
        boss.PlayAnim("Idle"); 
        
        yield return new WaitForSeconds(skillData.recoverTime);

        boss.ChangeState(BossState.Idle);
        currentCooldown = skillData.autoCooldown;
    }

    // (Giữ nguyên hàm ShakeSprite)
    IEnumerator ShakeSprite(float duration, float intensity)
    {
        Transform spriteTransform = boss.spriteRenderer.transform;
        Vector3 originalLocalPos = spriteTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            spriteTransform.localPosition = originalLocalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null; 
        }
        spriteTransform.localPosition = originalLocalPos;
    }

    void ShowTelegraph(bool show)
    {
        telegraphLine.enabled = show;
        if (show)
        {
            float radius = 1.5f; 
            float angleStep = 360f / 60;
            for (int i = 0; i < 60; i++)
            {
                float angle = Mathf.Deg2Rad * i * angleStep;
                float x = Mathf.Sin(angle) * radius;
                float y = Mathf.Cos(angle) * radius;
                telegraphLine.SetPosition(i, new Vector3(x, y, 0));
            }
        }
    }
}