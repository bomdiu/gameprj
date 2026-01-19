using UnityEngine;
using System.Collections.Generic;

public class BossAI : MonoBehaviour
{
    [Header("References")]
    public BossController boss;
    public BossHealth health;

    [Header("Basic Skill")]
    public Skill_BiteDash skillBite;

    [Header("Phase 1 Skills")]
    public Skill_Summon skillSummon;

    [Header("Phase 2 Skills")]
    public Skill_Explosion skillExplode;
    public Skill_Shockwave skillShockwave;

    [Header("AI Rhythm Config")] // --- CẤU HÌNH NHỊP ĐIỆU ---
    [Tooltip("Thời gian nghỉ bình thường sau khi dùng Skill đặc biệt")]
    public float standardDelay = 1.5f; 
    
    [Tooltip("Thời gian nghỉ ngắn giữa các cú cắn liên tiếp (Spam)")]
    public float biteSequenceDelay = 0.3f; // <-- BIẾN MỚI: Chỉnh tốc độ spam cắn ở đây

    [Header("Debug - Read Only")]
    public string activePhase = "Phase 1";
    public int biteCounter = 0;
    public int targetBites = 3;

    private float decisionTimer = 0f;
    private float currentDelayThreshold = 3.0f; // Biến nội bộ để lưu thời gian chờ hiện tại

    private void Start()
    {
        targetBites = Random.Range(3, 5);
        currentDelayThreshold = standardDelay; // Bắt đầu bằng delay chuẩn
    }

    private void Update()
    {
        // 1. Chỉ suy nghĩ khi Boss đang Rảnh (Idle)
        if (boss.currentState != BossState.Idle) return;

        // 2. Đếm giờ dựa trên ngưỡng (Threshold) động
        decisionTimer += Time.deltaTime;
        
        // So sánh với currentDelayThreshold thay vì biến cố định
        if (decisionTimer >= currentDelayThreshold) 
        {
            MakeDecision();
            decisionTimer = 0f;
        }
    }

    private void MakeDecision()
    {
        bool isLowHealth = health.IsPhase2;
        activePhase = isLowHealth ? "PHASE 2" : "PHASE 1";

        // === LOGIC 1: CHUỖI CẮN (SPAM) ===
        if (biteCounter < targetBites)
        {
            if (skillBite.IsReady)
            {
                skillBite.ActivateSkill();
                biteCounter++;

                // Nếu đây là cú cắn CUỐI CÙNG trong chuỗi
                if (biteCounter >= targetBites) 
                {
                    // Nghỉ lâu hơn một chút để báo hiệu sắp đổi chiêu
                    // Ví dụ: Nghỉ 1.0 giây trước khi Summon
                    currentDelayThreshold = 1.0f; 
                }

                // QUAN TRỌNG: Sau khi cắn, lần tới chỉ cần chờ một chút thôi
                else
                {
                currentDelayThreshold = biteSequenceDelay; 
                }
            }
            return;
        }

        // === LOGIC 2: SKILL ĐẶC BIỆT ===
        List<MonoBehaviour> allowedSpecials = new List<MonoBehaviour>();

        if (skillSummon.IsReady) allowedSpecials.Add(skillSummon);

        if (isLowHealth)
        {
            if (skillExplode.IsReady) allowedSpecials.Add(skillExplode);
            if (skillShockwave.IsReady) allowedSpecials.Add(skillShockwave);
        }

        if (allowedSpecials.Count > 0)
        {
            int index = Random.Range(0, allowedSpecials.Count);
            TriggerSkill(allowedSpecials[index]);

            // Reset chuỗi cắn
            biteCounter = 0;
            targetBites = isLowHealth ? Random.Range(2, 4) : Random.Range(3, 5);

            // QUAN TRỌNG: Sau khi dùng skill to, Boss cần nghỉ ngơi lâu hơn
            currentDelayThreshold = standardDelay;
        }
        else
        {
            // Fallback: Nếu không có skill to nào hồi kịp thì cắn đỡ
            if (skillBite.IsReady) 
            {
                skillBite.ActivateSkill();
                currentDelayThreshold = biteSequenceDelay; // Cắn thì vẫn delay ngắn
            }
        }
    }

    private void TriggerSkill(MonoBehaviour skill)
    {
        if (skill == skillSummon) skillSummon.ActivateSkill();
        else if (skill == skillExplode) skillExplode.ActivateSkill();
        else if (skill == skillShockwave) skillShockwave.ActivateSkill();
        
        Debug.Log($"[AI] Triggered: {skill.GetType().Name}");
    }
}