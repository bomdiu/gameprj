using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillCardUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private Image cardFrame;

    [Header("Rarity Sprites")]
    [SerializeField] private Sprite commonCardSprite; 
    [SerializeField] private Sprite rareCardSprite;   

    private SkillData thisSkillData;

    public void Setup(SkillData skill)
    {
        thisSkillData = skill;

        // 1. Basic Info
        if (titleText != null) titleText.text = skill.skillName;
        if (rarityText != null) rarityText.text = skill.skillRarity.ToString();

        // 2. Dynamic Description Calculation
        if (descriptionText != null)
        {
            descriptionText.text = GetDynamicDescription(skill);
        }

        // 3. Rarity Visuals
        if (cardFrame != null)
        {
            cardFrame.sprite = (skill.skillRarity == SkillData.Rarity.Rare) ? rareCardSprite : commonCardSprite;
        }

        // 4. Hover Data
        if (TryGetComponent(out UpgradeCardHover hover))
        {
            hover.SetData(skill);
        }
    }

    private string GetDynamicDescription(SkillData skill)
    {
        var mgr = UpgradeManager.Instance;
        if (mgr == null) return skill.description;

        string statsText = "";

        switch (skill.upgradeType)
        {
            case SkillData.SkillType.HealthUp:
                int curHP = mgr.playerHealth.maxHealth;
                int nextHP = curHP + (int)skill.valueAmount;
                statsText = $"{curHP} HP <color=#00FF00>-> {nextHP} HP</color>";
                break;

            case SkillData.SkillType.DamageUp:
                int curDmg = mgr.playerCombat.damage1;
                int nextDmg = curDmg + (int)skill.valueAmount;
                statsText = $"{curDmg} DMG <color=#00FF00>-> {nextDmg} DMG</color>";
                break;

            case SkillData.SkillType.SpeedUp:
                float curSpeed = mgr.playerMovement.speedMultiplier;
                float nextSpeed = curSpeed + skill.valueAmount;
                statsText = $"{curSpeed:0.##}x Speed <color=#00FF00>-> {nextSpeed:0.##}x</color>";
                break;

            case SkillData.SkillType.HealthRegen:
                int curRegen = mgr.playerHealth.healthRegen;
                int nextRegen = curRegen + (int)skill.valueAmount;
                statsText = $"{curRegen} Regen <color=#00FF00>-> {nextRegen} Regen</color>";
                break;

            case SkillData.SkillType.LifeSteal:
                float curLSChance = mgr.playerCombat.lifestealChance * 100f;
                float nextLSChance = curLSChance + (skill.secondValue * 100f);
                statsText = $"{curLSChance:0.#}% Chance <color=#00FF00>-> {nextLSChance:0.#}%</color>";
                break;

            case SkillData.SkillType.CritChance:
                float curCrit = mgr.playerCombat.critChance * 100f;
                float nextCrit = curCrit + (skill.valueAmount * 100f);
                statsText = $"{curCrit:0.#}% Crit <color=#00FF00>-> {nextCrit:0.#}%</color>";
                break;

            default:
                return skill.description;
        }

        return $"{skill.description}\n{statsText}";
    }

    public void OnClickSelect()
    {
        if (UpgradeManager.Instance != null && thisSkillData != null)
        {
            UpgradeManager.Instance.SelectUpgrade(thisSkillData, this.transform);
        }
    }
}