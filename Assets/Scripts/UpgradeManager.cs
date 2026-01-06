using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Cần thêm thư viện này để xử lý Layout

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance; // Singleton để dễ gọi

    [Header("Settings")]
    public GameObject upgradePanel; // Kéo object UpgradePanel vào đây
    public Transform cardsContainer; // Kéo object chứa layout group vào đây
    public GameObject cardPrefab; // Kéo Prefab SkillCard vào đây
    
    [Header("Data")]
    public List<SkillData> allSkills; // Kéo tất cả SkillData bạn tạo ở Bước 2 vào đây

    private void Awake()
    {
        Instance = this;
        // Đảm bảo ẩn panel khi bắt đầu game
        if(upgradePanel != null) upgradePanel.SetActive(false); 
    }

    // Hàm này gọi khi mở rương hoặc lên cấp
    public void ShowUpgradeOptions()
    {
        // 1. Pause game
        Time.timeScale = 0; 
        upgradePanel.SetActive(true);

        // 2. Xóa các thẻ bài cũ (nếu có) để tránh bị nhân đôi
        foreach (Transform child in cardsContainer)
        {
            Destroy(child.gameObject);
        }

        // 3. Chọn 3 skill ngẫu nhiên
        List<SkillData> randomSkills = GetRandomSkills(3);

        // 4. Tạo thẻ bài mới
        foreach (SkillData skill in randomSkills)
        {
            // --- SỬA LỖI: PHẢI TẠO RA OBJECT TRƯỚC ---
            GameObject newCard = Instantiate(cardPrefab, cardsContainer);
            
            // --- FIX LỖI SCALE VÀ VỊ TRÍ (QUAN TRỌNG) ---
            // Đặt lại tỉ lệ về chuẩn (1, 1, 1) để không bị méo hay biến mất
            newCard.transform.localScale = Vector3.one; 
            
            // Đặt lại vị trí Z về 0 để không bị chìm xuống dưới background
            Vector3 pos = newCard.transform.localPosition;
            pos.z = 0;
            newCard.transform.localPosition = pos;

            // 5. Setup dữ liệu vào thẻ
            SkillCardUI cardUI = newCard.GetComponent<SkillCardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(skill);
            }
            else
            {
                Debug.LogError("Lỗi: Prefab thẻ bài chưa gắn script SkillCardUI!");
            }
        }

        // --- FIX LỖI LAYOUT DÍNH CHÙM ---
        // Tắt đi bật lại LayoutGroup để Unity tính toán lại vị trí ngay lập tức
        LayoutGroup group = cardsContainer.GetComponent<LayoutGroup>();
        if (group != null)
        {
            group.enabled = false;
            group.enabled = true;
        }
    }

    public void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1; // Tiếp tục game
    }

    // Thuật toán chọn random không trùng lặp
    List<SkillData> GetRandomSkills(int count)
    {
        List<SkillData> tempList = new List<SkillData>(allSkills);
        List<SkillData> result = new List<SkillData>();

        // Nếu số lượng skill yêu cầu lớn hơn số skill đang có thì lấy hết
        if (count > tempList.Count) count = tempList.Count;

        for (int i = 0; i < count; i++)
        {
            if (tempList.Count == 0) break;
            int randomIndex = Random.Range(0, tempList.Count);
            result.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex); // Xóa để không chọn lại
        }
        return result;
    }
}