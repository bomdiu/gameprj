using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillCardUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private Image cardFrame; // The background image

    [Header("Rarity Sprites")]
    [SerializeField] private Sprite commonCardSprite; 
    [SerializeField] private Sprite rareCardSprite;   

    private SkillData thisSkillData;

    // Called by UpgradeManager when the card is spawned
    public void Setup(SkillData skill)
    {
        thisSkillData = skill;

        // Populate Text Fields
        if (titleText != null) titleText.text = skill.skillName;
        if (descriptionText != null) descriptionText.text = skill.description;
        if (rarityText != null) rarityText.text = skill.skillRarity.ToString();

        // SWAP SPRITE: Changes the background sprite based on rarity
        if (cardFrame != null)
        {
            cardFrame.sprite = (skill.skillRarity == SkillData.Rarity.Rare) ? rareCardSprite : commonCardSprite;
        }

        // Link the data to your hover script for effects
        if (TryGetComponent(out UpgradeCardHover hover))
        {
            hover.SetData(skill);
        }
    }

    public void OnClickSelect()
    {
        // Triggers the upgrade in the Manager
        if (UpgradeManager.Instance != null && thisSkillData != null)
        {
            UpgradeManager.Instance.SelectUpgrade(thisSkillData, this.transform);
        }
    }
}