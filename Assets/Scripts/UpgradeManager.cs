using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance; 

    [Header("Settings")]
    public GameObject upgradePanel; 
    public Transform cardsContainer; 
    public GameObject cardPrefab; 
    
    [Header("Visual Effects")]
    [SerializeField] private CanvasGroup panelCanvasGroup; 
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
    [SerializeField] private Vector3 targetScale = new Vector3(3.83f, 3.83f, 1f);

    [Header("Selection Burst VFX")]
    [SerializeField] private ParticleSystem selectionBurstPrefab; 

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
            if (panelCanvasGroup != null) panelCanvasGroup.alpha = 0;
        }
        
        if(portalObject != null) portalObject.SetActive(false); 
    }

    public void ShowUpgradeOptions()
    {
        Time.timeScale = 0; 
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
            
            SkillCardUI cardUI = newCard.GetComponent<SkillCardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(skill);
            }
        }

        StartCoroutine(AnimateEntrance());
    }

    private IEnumerator AnimateEntrance()
    {
        float timer = 0;
        cardsContainer.localScale = startScale;
        if (panelCanvasGroup != null) panelCanvasGroup.alpha = 0;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime; 
            float progress = timer / animationDuration;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3);

            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = easedProgress;

            cardsContainer.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);
            yield return null;
        }

        if (panelCanvasGroup != null) panelCanvasGroup.alpha = 1;
        cardsContainer.localScale = targetScale;

        LayoutGroup group = cardsContainer.GetComponent<LayoutGroup>();
        if (group != null)
        {
            group.enabled = false;
            group.enabled = true;
        }
    }

    // UPDATED FUNCTION: Fixed for visibility and reliability
    public void SelectUpgrade(Transform selectedCardTransform)
    {
        Debug.Log("SelectUpgrade triggered for card: " + selectedCardTransform.name);

        if (selectionBurstPrefab == null)
        {
            Debug.LogError("CRITICAL: selectionBurstPrefab is NULL!");
            StartCoroutine(DelayedClose());
            return;
        }

        // 1. Spawn as child initially to get the correct world position
        ParticleSystem burst = Instantiate(selectionBurstPrefab, selectedCardTransform);
        
        if (burst != null)
        {
            // 2. Position it exactly on the card but pull it far toward the camera (Z = -500)
            burst.transform.localPosition = new Vector3(0, 0, -500f);
            burst.transform.localScale = Vector3.one; // Ensure it's not squashed
            burst.transform.localRotation = Quaternion.identity;

            // 3. Set to unscaled time so it plays while paused
            var main = burst.main;
            main.useUnscaledTime = true; 

            burst.Play();

            // 4. IMPORTANT: Move to Root so it doesn't get disabled when the panel closes
            burst.transform.SetParent(null);
            
            // 5. Cleanup
            Destroy(burst.gameObject, 2f); 
        }

        StartCoroutine(DelayedClose());
    }

    private IEnumerator DelayedClose()
    {
        // Increased delay slightly so the player sees the start of the burst
        yield return new WaitForSecondsRealtime(0.15f); 
        CloseUpgradePanel();
    }

    public void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1; 

        if (portalObject != null)
        {
            portalObject.SetActive(true); 
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