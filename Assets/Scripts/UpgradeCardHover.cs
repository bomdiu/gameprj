using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationSpeed = 10f;
    
    private Vector3 originalScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;

    // Added the missing variable to store the skill reference
    private SkillData thisSkillData;

    // Added a method to receive the data from your SkillCardUI script
    public void SetData(SkillData data)
    {
        thisSkillData = data;
    }

    private void Update()
    {
        // Use unscaledDeltaTime because game is paused
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // FIXED: Now passes both SkillData and the Transform to satisfy the UpgradeManager
        if (thisSkillData != null && UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.SelectUpgrade(thisSkillData, this.transform);
        }
    }
}