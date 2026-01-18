using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Required for Coroutines

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance; 

    [Header("Settings")]
    public GameObject upgradePanel; 
    public Transform cardsContainer; 
    public GameObject cardPrefab; 
    
    [Header("Visual Effects")]
    [SerializeField] private CanvasGroup panelCanvasGroup; // Add a CanvasGroup to your panel
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);

    [Header("Portal Settings")]
    public GameObject portalObject; 

    [Header("Data")]
    public List<SkillData> allSkills; 

    private void Awake()
    {
        Instance = this;
        if(upgradePanel != null) 
        {
            upgradePanel.SetActive(false); 
            // Initialize transparency if CanvasGroup exists
            if (panelCanvasGroup != null) panelCanvasGroup.alpha = 0;
        }
        
        if(portalObject != null) portalObject.SetActive(false); 
    }

    public void ShowUpgradeOptions()
    {
        // Stop time and show panel
        Time.timeScale = 0; 
        upgradePanel.SetActive(true);

        // Clear and refill cards
        foreach (Transform child in cardsContainer)
        {
            Destroy(child.gameObject);
        }

        List<SkillData> randomSkills = GetRandomSkills(3);

        foreach (SkillData skill in randomSkills)
        {
            GameObject newCard = Instantiate(cardPrefab, cardsContainer);
            newCard.transform.localScale = Vector3.one; 
            
            SkillCardUI cardUI = newCard.GetComponent<SkillCardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(skill);
            }
        }

        // Trigger the visual entrance animation
        StartCoroutine(AnimateEntrance());
    }

    private IEnumerator AnimateEntrance()
    {
        float timer = 0;
        
        // Reset scale and alpha before starting
        cardsContainer.localScale = startScale;
        if (panelCanvasGroup != null) panelCanvasGroup.alpha = 0;

        while (timer < animationDuration)
        {
            // MUST use unscaledDeltaTime because timeScale is 0
            timer += Time.unscaledDeltaTime; 
            float progress = timer / animationDuration;

            // Ease out cubic for a smoother feel
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3);

            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = easedProgress;

            cardsContainer.localScale = Vector3.Lerp(startScale, Vector3.one, easedProgress);
            
            yield return null;
        }

        // Ensure final values are set
        if (panelCanvasGroup != null) panelCanvasGroup.alpha = 1;
        cardsContainer.localScale = Vector3.one;

        // Force layout update
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
        Time.timeScale = 1; // Resume game

        if (portalObject != null)
        {
            portalObject.SetActive(true); 
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