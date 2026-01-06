using UnityEngine;

public class RewardItem : MonoBehaviour
{
    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;
            Debug.Log("Nhặt được rương thần!");

            // Gọi cái bảng nâng cấp mà chúng ta đã làm ở các bước trước
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.ShowUpgradeOptions();
            }

            // Hủy vật phẩm sau khi nhặt
            Destroy(gameObject);
        }
    }
}