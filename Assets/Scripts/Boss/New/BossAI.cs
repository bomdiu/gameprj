using UnityEngine;
using System.Collections.Generic;

public class BossAI : MonoBehaviour
{
    [Header("References")]
    public BossController boss;
    public BossHealth health;

    [Header("Always Available (Standard Attack)")]
    public Skill_BiteDash skillBite;

    [Header("Phase 1 - Special Skills")]
    public Skill_Summon skillSummon;

    [Header("Phase 2 - Special Skills (Unlocked < 50% HP)")]
    public Skill_Explosion skillExplode;
    public Skill_Shockwave skillShockwave;

    [Header("AI Configuration")]
    public float decisionDelay = 0.5f;
    [Range(0f, 1f)] public float phase2Threshold = 0.5f; // 0.5 = 50% health

    [Header("Debug Info")]
    public string currentPhaseName = "Phase 1";
    public int biteCounter = 0;

    private List<MonoBehaviour> specialSkillPool = new List<MonoBehaviour>();
    private int targetBites;
    private float decisionTimer;
    private bool isPhase2Active = false;

    private void Start()
    {
        // 1. Initial Pool: Only Phase 1 specials are allowed
        specialSkillPool.Clear();
        specialSkillPool.Add(skillSummon);
        
        targetBites = Random.Range(3, 5);
    }

    private void Update()
    {
        // GUARD: Only make decisions if the boss is Idle
        if (boss.currentState != BossState.Idle) return;

        // 2. Health Check: Handle Phase Transition
        CheckHealthThreshold();

        // 3. Decision Timer
        decisionTimer += Time.deltaTime;
        if (decisionTimer >= decisionDelay)
        {
            MakeDecision();
            decisionTimer = 0f;
        }
    }

    private void CheckHealthThreshold()
    {
        if (isPhase2Active) return;

        // Standard percentage calculation: $Current / Max$
        float hpPercent = (float)health.currentHealth / health.maxHealth;

        if (hpPercent <= phase2Threshold)
        {
            ActivatePhase2();
        }
    }

    private void ActivatePhase2()
    {
        isPhase2Active = true;
        currentPhaseName = "Phase 2 (Danger)";

        // UNLOCK: Add Phase 2 skills to the pool
        specialSkillPool.Add(skillExplode);
        specialSkillPool.Add(skillShockwave);

        Debug.Log("<color=red><b>Boss Phase 2 Triggered!</b></color> Explosion and Shockwave unlocked.");
    }

    private void MakeDecision()
    {
        // LOGIC: The "Bite-Bite-Special" Rhythm
        if (biteCounter < targetBites)
        {
            if (skillBite.IsReady)
            {
                skillBite.ActivateSkill();
                biteCounter++;
            }
        }
        else
        {
            ExecuteSpecialFromPool();
        }
    }

    private void ExecuteSpecialFromPool()
    {
        // Create a temporary list of skills that are currently off-cooldown
        List<MonoBehaviour> readySpecials = specialSkillPool.FindAll(s => IsSkillReady(s));

        if (readySpecials.Count > 0)
        {
            // Pick a random skill from the allowed/ready list
            MonoBehaviour chosen = readySpecials[Random.Range(0, readySpecials.Count)];
            
            TriggerSpecificSkill(chosen);

            // Reset the bite cycle
            biteCounter = 0;
            targetBites = isPhase2Active ? Random.Range(2, 4) : Random.Range(3, 5);
        }
    }

    // Helper: Map the generic MonoBehaviour back to the specific skill script
    private void TriggerSpecificSkill(MonoBehaviour s)
    {
        if (s == skillSummon) skillSummon.ActivateSkill();
        else if (s == skillExplode) skillExplode.ActivateSkill();
        else if (s == skillShockwave) skillShockwave.ActivateSkill();
    }

    // Helper: Check cooldowns for the pool
    private bool IsSkillReady(MonoBehaviour s)
    {
        if (s == skillSummon) return skillSummon.IsReady;
        if (s == skillExplode) return skillExplode.IsReady;
        if (s == skillShockwave) return skillShockwave.IsReady;
        return false;
    }
}
