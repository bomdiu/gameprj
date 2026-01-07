using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance; 

    [Header("Settings")]
    public GameObject upgradePanel; 
    public Transform cardsContainer; 
    public GameObject cardPrefab; 
    
    // --- THÊM BIẾN PORTAL TẠI ĐÂY ---
    [Header("Portal Settings")]
    public GameObject portalObject; // Kéo object Portal (cái cổng) vào đây

    [Header("Data")]
    public List<SkillData> allSkills; 

    private void Awake()
    {
        Instance = this;
        if(upgradePanel != null) upgradePanel.SetActive(false); 
        
        // Đảm bảo cổng luôn ẩn khi bắt đầu màn chơi
        if(portalObject != null) portalObject.SetActive(false); 
    }

    public void ShowUpgradeOptions()
    {
        Time.timeScale = 0; // Dừng thời gian game
        upgradePanel.SetActive(true);

        foreach (Transform child in cardsContainer)
        {
            Destroy(child.gameObject);
        }

        List<SkillData> randomSkills = GetRandomSkills(3);

        foreach (SkillData skill in randomSkills)
        {
            GameObject newCard = Instantiate(cardPrefab, cardsContainer);
            newCard.transform.localScale = Vector3.one; 
            
            Vector3 pos = newCard.transform.localPosition;
            pos.z = 0;
            newCard.transform.localPosition = pos;

            SkillCardUI cardUI = newCard.GetComponent<SkillCardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(skill);
            }
        }

        LayoutGroup group = cardsContainer.GetComponent<LayoutGroup>();
        if (group != null)
        {
            group.enabled = false;
            group.enabled = true;
        }
    }

    // Hàm này được gọi khi người chơi nhấn chọn một thẻ kỹ thuật số
    public void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1; // Cho game chạy tiếp

        // --- KÍCH HOẠT CỔNG SAU KHI CHỌN XONG ---
        if (portalObject != null)
        {
            portalObject.SetActive(true); // Hiện cổng để người chơi đi tới
            Debug.Log("Portal has appeared!");
        }
    }

    List<SkillData> GetRandomSkills(int count)
    {
        List<SkillData> tempList = new List<SkillData>(allSkills);
        List<SkillData> result = new List<SkillData>();

        if (count > tempList.Count) count = tempList.Count;

        for (int i = 0; i < count; i++)
        {
            if (tempList.Count == 0) break;
            int randomIndex = Random.Range(0, tempList.Count);
            result.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex); 
        }
        return result;
    }
}