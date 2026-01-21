using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillCardUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Button cardButton;

    private SkillData _data;

    public void Setup(SkillData data)
    {
        _data = data;
        
        titleText.text = data.skillName;
        descText.text = data.description;

        // Reset the button to ensure it's clean
        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(OnCardSelected);
    }

    private void OnCardSelected()
    {
        Debug.Log("1. Card Button physically clicked!");

        if (_data == null) 
        {
            Debug.LogError("Data is null on this card!");
            return;
        }

        // Apply stats
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ApplyUpgrade(_data.upgradeType, _data.valueAmount);
            Debug.Log("2. PlayerStats applied.");
        }

        // Trigger the Manager
        if (UpgradeManager.Instance != null)
        {
            Debug.Log("3. Calling UpgradeManager.SelectUpgrade...");
            UpgradeManager.Instance.SelectUpgrade(this.transform);
        }
        else
        {
            Debug.LogError("UpgradeManager.Instance is MISSING!");
        }
    }
}