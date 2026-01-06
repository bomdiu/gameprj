using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName; // Tên hiển thị
    [TextArea] public string description; // Mô tả
    
    // Ví dụ về chỉ số sẽ tăng
    public enum SkillType {
        AttackUp, 
        SpeedUp, 
        HealthUp
        // Thêm các loại khác ở đây (ví dụ: Cooldown, Armor...)
    }

    public SkillType upgradeType;
    public float valueAmount; // Ví dụ: tăng 10 damage thì điền 10
}