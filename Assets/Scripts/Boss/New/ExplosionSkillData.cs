using UnityEngine;

[CreateAssetMenu(fileName = "NewExplosionData", menuName = "Boss/Explosion Data")]
public class ExplosionSkillData : ScriptableObject
{
    [Header("Visual Resources")]
    // Không cần Sprite nào nữa!
    public GameObject explosionVFX; 

    [Header("Cấu hình Kích thước (Vector2)")]
    [Tooltip("X: Chiều rộng, Y: Chiều cao (Chỉnh khác nhau để tạo hình Elip)")]
    public Vector2 areaSize = new Vector2(3f, 3f); 
    
    [Header("Cấu hình Viền (Border)")]
    public float borderWidth = 0.1f; // Độ dày nét vẽ
    public Color borderColor = new Color(1, 0, 0, 1);
    
    [Header("Cấu hình Lõi (Core)")]
    public Color coreColor = new Color(1, 0, 0, 0.5f);
    
    [Header("Hiệu ứng Nổ")]
    public float vfxScale = 1.0f;

    [Header("Gameplay")]
    public float damage = 15f;
    public int explosionCount = 5;
    public float spawnInterval = 0.3f;   
    public float growthTime = 1.5f;      
    public float bossRecoverTime = 2.0f; 
    public float autoCooldown = 8.0f;
}