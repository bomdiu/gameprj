using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Boss/Skill Data")]
public class BossSkillData : ScriptableObject
{
    [Header("Cấu hình chung")]
    public string skillName = "Bite Dash";
    public float damage = 10f;
    public float autoCooldown = 3.0f;

    [Header("Giai đoạn 1: Chuẩn bị (Telegraph Line)")]
    public float prepareTime = 1.0f;        // Tổng thời gian cảnh báo
    public float lineGrowthTime = 0.3f;     // Thời gian để vạch đỏ chạy từ chân boss đến max độ dài
    public float maxLength = 6.0f;          // Độ dài tối đa
    public float width = 0.5f;              // Độ rộng của đường kẻ
    public Color telegraphColor = Color.red; // Màu sắc
    public Material lineMaterial;           // Material cho Line (Để None sẽ tự lấy Default)

    [Header("Giai đoạn 2: Tấn công (Dash)")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.4f;

    [Header("Giai đoạn 3: Hồi phục (Recover)")]
    public float recoverTime = 1.5f;
}