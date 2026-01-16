using UnityEngine;
using System.Collections.Generic;

public class BossAttackHitbox : MonoBehaviour
{
    private Skill_BiteDash ownerSkill; // Tham chiếu về Skill chính
    private List<GameObject> hitList = new List<GameObject>(); // Danh sách những kẻ đã bị đánh (để tránh đánh 1 người 2 lần trong 1 lần lướt)

    public void Setup(Skill_BiteDash skill)
    {
        ownerSkill = skill;
    }

    // Mỗi lần bật hitbox lên thì xóa danh sách cũ đi
    private void OnEnable()
    {
        hitList.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu va chạm với Player VÀ Player này chưa bị đánh trong lần lướt này
        if (collision.CompareTag("Player") && !hitList.Contains(collision.gameObject))
        {
            // Thêm vào danh sách đã đánh
            hitList.Add(collision.gameObject);
            
            // Báo cho Boss biết để trừ máu
            ownerSkill.OnHitPlayer(collision.gameObject);
        }
    }
}