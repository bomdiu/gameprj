using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SkillUIController : MonoBehaviour
{
    public static SkillUIController Instance;

    [Header("Fade Settings")]
    public CanvasGroup popupCanvasGroup; 
    public float fadeSpeed = 2f;
    public float displayTime = 3f;

    [Header("Card Content")]
    public TextMeshProUGUI cardName; 
    public TextMeshProUGUI cardDesc;
    public TextMeshProUGUI cardTypeText; 

    [Header("Corner UI")]
    public GameObject cornerIcon1; 
    public GameObject cornerIcon2; 

    [Header("Cooldown Overlays")]
    public Image cooldownOverlay1; // Drag Skill1_CooldownOverlay here
    public Image cooldownOverlay2; // Drag Skill2_CooldownOverlay here

    void Awake()
    {
        // Ensures the Player scripts always talk to the UI in the CURRENT scene
        Instance = this; 
    }

   void Start()
{
    // Force the cooldown overlays to be empty when the scene starts
    if (cooldownOverlay1 != null) cooldownOverlay1.fillAmount = 0; 
    if (cooldownOverlay2 != null) cooldownOverlay2.fillAmount = 0; 

    RefreshUI(); //
}

    public void RefreshUI()
    {
        if (SkillUnlockManager.Instance == null) return; //

        // Activates corner icons based on the Manager's unlock status
        if (SkillUnlockManager.Instance.skill1Unlocked) cornerIcon1.SetActive(true);
        if (SkillUnlockManager.Instance.skill2Unlocked) cornerIcon2.SetActive(true);
    }

    // --- COOLDOWN LOGIC ---
 public void StartCooldown(int skillID, float duration)
{
    Image targetOverlay = (skillID == 1) ? cooldownOverlay1 : cooldownOverlay2;

    if (targetOverlay == null)
    {
        Debug.LogError($"UI ERROR: Skill {skillID} overlay is NOT assigned in the Inspector for this scene!");
        return;
    }

    Debug.Log($"UI: Starting cooldown for Skill {skillID} on object {targetOverlay.gameObject.name}");
    StartCoroutine(CooldownRoutine(targetOverlay, duration));
}

   private IEnumerator CooldownRoutine(Image overlay, float duration)
{
    if (overlay == null || duration <= 0) yield break;

    float timer = duration;
    overlay.fillAmount = 1; //

    while (timer > 0)
    {
        // Reduce timer by the real time passed
        timer -= Time.deltaTime; 
        
        // Update the fill amount based on remaining time
        overlay.fillAmount = Mathf.Clamp01(timer / duration); 
        
        yield return null; // Wait for the next frame
    }
    
    overlay.fillAmount = 0; // Final cleanup
}
    // --- SKILL UNLOCK ANIMATION ---
    public void TriggerSkillUnlock(string sName, string sDesc, int skillID)
    {
        // 1. Instantly sync the icon and the card
        RefreshUI(); 

        // 2. Clear any existing card animations to prevent flickering
        StopAllCoroutines(); 
        StartCoroutine(SkillFadeRoutine(sName, sDesc));
    }

    private IEnumerator SkillFadeRoutine(string sName, string sDesc)
    {
        // Setup card visual state
        popupCanvasGroup.gameObject.SetActive(true); //
        popupCanvasGroup.alpha = 0; 
        cardName.text = sName;
        cardDesc.text = sDesc;
        cardTypeText.text = "SKILL";

        // Fade In
        while (popupCanvasGroup.alpha < 1)
        {
            popupCanvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        // Wait (stays visible)
        yield return new WaitForSeconds(displayTime);

        // Fade Out
        while (popupCanvasGroup.alpha > 0)
        {
            popupCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }
}