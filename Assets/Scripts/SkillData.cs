using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Display Settings")]
    public string skillName; 
    [TextArea] public string description; 
    public Sprite icon; 

    [Header("Rarity Settings")]
    public Rarity skillRarity;
    public enum Rarity 
    { 
        Common, 
        Rare 
    }

    [Header("Stat Logic")]
    public SkillType upgradeType;
    
    public enum SkillType 
    {
        // COMMON (Map 1 available)
        HealthUp,      // Flat int
        DamageUp,      // Flat int
        DashCooldown,  // Flat int reduction
        SpeedUp,       // Percentage (e.g., 0.1 for +10%)
        
        // RARE
        LifeSteal,     // Uses valueAmount (%) and secondValue (Chance)
        SkillDamage,   // Map 1 restricted
        HealthRegen,   // Flat int/s
        CritChance     // Percentage chance for fixed 1.5x damage
    }

    [Tooltip("Primary value for the upgrade (e.g., Damage amount or Speed percentage).")]
    public float valueAmount; 

    [Tooltip("Used only for Life Steal to define the trigger CHANCE (0.0 to 1.0).")]
    public float secondValue; 
}