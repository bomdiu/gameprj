[System.Serializable]
public class PlayerData
{
    // Movement
    public float speedMultiplier = 1f;
    public float dashCooldown = 1f;

    // Health
    public int maxHealth = 100;
    public int healthRegen = 0;

    // Combat
    public float critChance = 0.01f;
    public float lifestealChance = 0.01f;
    public float lifestealPercent = 0.1f;
    public int skillDamageBonus = 0;
}