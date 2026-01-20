using UnityEngine;

[CreateAssetMenu(fileName = "NewExplosionData", menuName = "Boss/Explosion Data")]
public class ExplosionSkillData : ScriptableObject
{
    [Header("Visual Resources")]
    public GameObject explosionVFX; // Optional: If you spawn an explosion effect later

    [Header("Visual Settings (Indicator)")]
    [Tooltip("Size of the red warning box. X = Width, Y = Height.")]
    public Vector2 areaSize = new Vector2(3f, 3f); 
    
    public float borderWidth = 0.1f; 
    public Color borderColor = new Color(1, 0, 0, 1);
    public Color coreColor = new Color(1, 0, 0, 0.5f);
    public float vfxScale = 1.0f;

    [Header("Logic Settings (Hitbox)")]
    [Tooltip("IMPORTANT: This controls the actual damage range. Should be roughly half of Area Size X.")]
    public float explosionRadius = 1.5f; // Used in HandleExplosionDamage (Physics2D.OverlapCircle)

    [Header("Gameplay Stats")]
    public float damage = 15f;          // Used in HandleExplosionDamage
    public int explosionCount = 5;      // Used in loop
    public float spawnInterval = 0.3f;  // Used in loop delay
    
    [Tooltip("Time to wait before dealing damage (matches the visual growth time).")]
    public float explosionDelay = 1.5f; // Used in HandleExplosionDamage (WaitForSeconds)
    
    public float bossRecoverTime = 2.0f; 
    public float autoCooldown = 8.0f;
}