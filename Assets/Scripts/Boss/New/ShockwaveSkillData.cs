using UnityEngine;

[CreateAssetMenu(fileName = "NewShockwaveData", menuName = "Boss/Shockwave Data")]
public class ShockwaveSkillData : ScriptableObject
{
    [Header("Cấu hình Đợt sóng")]
    public int waveCount = 3;            
    public float spawnInterval = 0.5f;   
    public float screamDuration = 2.0f;    

    [Header("Hiệu ứng Rung (Shake)")]
    [Tooltip("Độ rung khi đang gồng (Cast) - Nên để nhỏ (VD: 0.05)")]
    public float castShakeIntensity = 0.05f;    // <-- ĐỔI TÊN

    [Tooltip("Độ rung khi hét (Release) - Nên để to hơn (VD: 0.2)")]
    public float releaseShakeIntensity = 0.2f;  // <-- BIẾN MỚI
    
    // ... (Giữ nguyên các biến cũ bên dưới: maxRadius, damage, visual...)
    [Header("Cấu hình Sóng đơn lẻ")]
    public float maxRadius = 6.0f;       
    public float expansionSpeed = 10.0f; 
    public float startWidth = 0.5f;      
    public float endWidth = 2.0f;        
    
    [Header("Visual")]
    public Color waveColor = new Color(1, 1, 1, 0.8f); 
    public int segments = 60; 

    [Header("Tác động")]
    public float damage = 10f;
    public float knockbackForce = 15f; 

    [Header("Thời gian chung")]
    public float castTime = 1.0f;      
    public float recoverTime = 1.5f;   
    public float autoCooldown = 12.0f;
}