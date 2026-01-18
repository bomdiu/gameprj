using UnityEngine;
using System.Collections.Generic;

public class BossAI : MonoBehaviour
{
    [Header("References")]
    public BossController boss;
    public BossHealth health;

    [Header("Basic Skill (Always)")]
    public Skill_BiteDash skillBite;

    [Header("Phase 1 - Special Skills")]
    public Skill_Summon skillSummon;

    [Header("Phase 2 - Special Skills (Unlocked < 50% HP)")]
    public Skill_Explosion skillExplode;
    public Skill_Shockwave skillShockwave;

    [Header("AI Config")]
    public float decisionDelay = 0.5f;
    
    [Header("Debug - Read Only")]
    public string activePhase = "Phase 1";
    public int biteCounter = 0;
    public int targetBites = 3;

    private float decisionTimer = 0f;

    private void Start()
    {
        // Randomize the first set of bites
        targetBites = Random.Range(3, 5);
    }

    private void Update()
    {
        // 1. GUARD: Only think if the boss is currently Idle
        if (boss.currentState != BossState.Idle) return;

        // 2. TIMER: Delay between attacks
        decisionTimer += Time.deltaTime;
        if (decisionTimer >= decisionDelay)
        {
            MakeDecision();
            decisionTimer = 0f;
        }
    }

    private void MakeDecision()
    {
        // Check health script for the 50% threshold
        bool isLowHealth = health.IsPhase2;
        activePhase = isLowHealth ? "PHASE 2" : "PHASE 1";

        // === LOGIC 1: THE BITE RHYTHM ===
        // Boss bites several times before using a "Special" skill
        if (biteCounter < targetBites)
        {
            if (skillBite.IsReady)
            {
                skillBite.ActivateSkill();
                biteCounter++;
            }
            return;
        }

        // === LOGIC 2: SPECIAL SKILL SELECTION ===
        // We build a list of "Allowed" skills based on current Phase
        List<MonoBehaviour> allowedSpecials = new List<MonoBehaviour>();

        // Phase 1: Always includes Summon
        if (skillSummon.IsReady) allowedSpecials.Add(skillSummon);

        // Phase 2: ONLY adds these if health is < 50%
        if (isLowHealth)
        {
            if (skillExplode.IsReady) allowedSpecials.Add(skillExplode);
            if (skillShockwave.IsReady) allowedSpecials.Add(skillShockwave);
        }

        // EXECUTION: Pick one randomly from the allowed list
        if (allowedSpecials.Count > 0)
        {
            int index = Random.Range(0, allowedSpecials.Count);
            TriggerSkill(allowedSpecials[index]);

            // Reset the bite cycle
            biteCounter = 0;
            targetBites = isLowHealth ? Random.Range(2, 4) : Random.Range(3, 5);
        }
        else
        {
            // Fallback: If no specials are ready, just bite again
            if (skillBite.IsReady) skillBite.ActivateSkill();
        }
    }

    // Helper: Correctly triggers the specific script
    private void TriggerSkill(MonoBehaviour skill)
    {
        if (skill == skillSummon) skillSummon.ActivateSkill();
        else if (skill == skillExplode) skillExplode.ActivateSkill();
        else if (skill == skillShockwave) skillShockwave.ActivateSkill();
        
        Debug.Log($"[AI] {activePhase} - Triggered: {skill.GetType().Name}");
    }
}