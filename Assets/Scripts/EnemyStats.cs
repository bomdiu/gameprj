using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Combat Stats")]
    public int baseDamage = 10; // Chỉ số sát thương cơ bản
    public float attackSpeed = 1.0f; // Tốc độ đánh (số lần đánh/giây)

    [Header("Health Stats")]
    public int maxHealth = 50;
    // Thêm các chỉ số khác nếu cần (Ví dụ: public float defense)
}