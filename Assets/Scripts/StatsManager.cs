using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    [Header("Persistent Movement Stats")]
    public float speedMultiplier = 1f;
    public float dashCooldown = 1f;

    [Header("Persistent Health Stats")]
    public int maxHealth = 100;
    public int healthRegen = 0;

    // --- NEW: Persistent Energy Stats ---
    [Header("Persistent Energy Stats")]
    public int maxEnergy = 100;
    public int currentEnergy = 0;

    [Header("Persistent Combat Stats")]
    public int damageBonus = 0; // Adds to damage1, 2, and 3
    public float critChance = 0.01f;
    public float lifestealChance = 0.01f;
    public int skillDamageBonus = 0;

    void Awake()
    {
        // Persistent Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Resets all persistent stats to their default values.
    /// Call this when the player dies or returns to the Main Menu.
    /// </summary>
    public void ResetStats()
    {
        speedMultiplier = 1f;
        dashCooldown = 1f;
        
        maxHealth = 100;
        healthRegen = 0;
        
        // --- Reset Energy ---
        maxEnergy = 100;
        currentEnergy = 0; 

        damageBonus = 0;
        critChance = 0.01f;
        lifestealChance = 0.01f;
        skillDamageBonus = 0;

        Debug.Log("<color=red>StatsManager:</color> All stats have been reset to default.");
    }

    /// <summary>
    /// This is called by UpgradeManager whenever a skill is picked.
    /// It updates the "Master" values that survive scene changes.
    /// </summary>
    public void UpdatePersistentStats(SkillData skill)
    {
        switch (skill.upgradeType)
        {
            case SkillData.SkillType.HealthUp:
                maxHealth += (int)skill.valueAmount; break;
            case SkillData.SkillType.DamageUp:
                damageBonus += (int)skill.valueAmount; break;
            case SkillData.SkillType.DashCooldown:
                dashCooldown = Mathf.Max(0.1f, dashCooldown - skill.valueAmount); break;
            case SkillData.SkillType.SpeedUp:
                speedMultiplier += skill.valueAmount; break;
            case SkillData.SkillType.LifeSteal:
                lifestealChance += skill.secondValue; break;
            case SkillData.SkillType.SkillDamage:
                skillDamageBonus += (int)skill.valueAmount; break;
            case SkillData.SkillType.HealthRegen:
                healthRegen += (int)skill.valueAmount; break;
            case SkillData.SkillType.CritChance:
                critChance += skill.valueAmount; break;
        }
        
        Debug.Log("<color=green>StatsManager Updated: </color> Persistent data saved.");
    }

    /// <summary>
    /// This is called by the Player when a new scene loads.
    /// </summary>
    public void ApplyStatsToPlayer(PlayerMovement move, PlayerHealth health, PlayerCombat combat)
    {
        if (move != null)
        {
            move.speedMultiplier = speedMultiplier;
            move.dashCooldown = dashCooldown;
        }

        if (health != null)
        {
            health.maxHealth = maxHealth;
            health.healthRegen = healthRegen;
            // Optionally reset current health to max when entering a new scene
            health.currentHealth = maxHealth; 

            // Tell the Health Script to update the UI Bar
            health.SyncHealthUI();
        }

        if (combat != null)
        {
            combat.damage1 += damageBonus;
            combat.damage2 += damageBonus;
            combat.damage3 += damageBonus;
            combat.critChance = critChance;
            combat.lifestealChance = lifestealChance;
            combat.skillDamageBonus = skillDamageBonus;
        }
        
        Debug.Log("<color=yellow>StatsManager:</color> Applied persistent stats and refreshed UI.");
    }
}