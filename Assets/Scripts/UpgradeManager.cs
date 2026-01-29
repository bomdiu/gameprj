using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance; 

    [Header("UI References")]
    public GameObject upgradePanel; 
    public Transform cardsContainer; 
    public GameObject cardPrefab; 
    
    [Header("Menu Animation")]
    [SerializeField] private CanvasGroup panelCanvasGroup; 
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
    [SerializeField] private Vector3 targetScale = Vector3.one; 

    [Header("Player References")]
    [SerializeField] public PlayerMovement playerMovement;
    [SerializeField] public PlayerHealth playerHealth;
    [SerializeField] public PlayerCombat playerCombat;
    public int currentMapIndex = 1; 

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem selectionBurstPrefab; 

    [Header("Firefly Absorption Settings")]
    [SerializeField] private ParticleSystem fireflySystem;   
    [SerializeField] private Transform playerTransform;      
    [SerializeField] private float transitionDelay = 1.2f;    
    [SerializeField] private float vortexPullStrength = -40f; 
    [SerializeField] [Range(0f, 1f)] private float fadeStartPercentage = 0.75f;

    [Header("Light Glow Settings")]
    [SerializeField] private Light2D playerGlowLight; 
    [SerializeField] private float maxGlowIntensity = 1.4f; 
    [SerializeField] private float maxOuterRadius = 4f;
    [SerializeField] private float glowFadeDuration = 1.0f; 

    [Header("SFX Settings")] // --- PHẦN THÊM MỚI ---
    [SerializeField] private AudioSource audioSource;       // Kéo AudioSource vào đây
    [SerializeField] private AudioClip vortexSuctionSFX;    // Âm thanh gió hút/năng lượng khi particle bay vào
    [SerializeField] private AudioClip upgradePanelOpenSFX; // Âm thanh "Ting" hoặc "Whoosh" khi bảng UI hiện ra
    [SerializeField] [Range(0, 1)] private float vortexVolume = 0.7f;
    [SerializeField] [Range(0, 1)] private float openVolume = 1.0f;

    [Header("Data")]
    public List<SkillData> allSkills; 

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (upgradePanel != null && !upgradePanel.activeSelf)
            {
                Debug.Log("<color=cyan>Debug: Opening Upgrade Panel</color>");
                ShowUpgradeOptions();
            }
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if(upgradePanel != null) upgradePanel.SetActive(false); 
        
        if (playerGlowLight != null) 
        {
            playerGlowLight.intensity = 0;
            playerGlowLight.pointLightOuterRadius = 0;
        }
    }

    public void StartWaveEndTransition()
    {
        StartCoroutine(WaveEndRoutine()); 
    }

    private IEnumerator WaveEndRoutine()
    {
        Time.timeScale = 1f; 

        var velocityModule = fireflySystem.velocityOverLifetime;
        var emissionModule = fireflySystem.emission;
        var colorModule = fireflySystem.colorOverLifetime; 
        var mainModule = fireflySystem.main;
        
        emissionModule.enabled = false; 
        velocityModule.enabled = true; 
        velocityModule.space = ParticleSystemSimulationSpace.World; 
        colorModule.enabled = true; 

        // SFX: Phát âm thanh hút particle
        if (audioSource != null && vortexSuctionSFX != null)
        {
            audioSource.PlayOneShot(vortexSuctionSFX, vortexVolume);
        }

        float elapsed = 0f; 
        while (elapsed < transitionDelay) 
        {
            float normalizedTime = elapsed / transitionDelay;
            
            if (playerTransform != null)
            {
                fireflySystem.transform.position = playerTransform.position;
                velocityModule.radial = Mathf.Lerp(0, vortexPullStrength, normalizedTime * 2f);

                if (playerGlowLight != null)
                {
                    playerGlowLight.intensity = normalizedTime * maxGlowIntensity;
                    playerGlowLight.pointLightOuterRadius = normalizedTime * maxOuterRadius;
                }

                if (normalizedTime >= fadeStartPercentage)
                {
                    float fadeAlpha = Mathf.Lerp(1f, 0f, (normalizedTime - fadeStartPercentage) / (1f - fadeStartPercentage));
                    Gradient grad = new Gradient();
                    grad.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(fadeAlpha, 0f), new GradientAlphaKey(fadeAlpha, 1f) }
                    );
                    colorModule.color = grad;
                }
            }
            elapsed += Time.unscaledDeltaTime; 
            yield return null; 
        }

        fireflySystem.Clear(); 
        velocityModule.radial = 0f; 
        velocityModule.enabled = false;

        Gradient resetGrad = new Gradient();
        resetGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        colorModule.color = resetGrad;
        colorModule.enabled = false; 

        emissionModule.enabled = true; 

        if (playerGlowLight != null)
        {
            playerGlowLight.intensity = maxGlowIntensity;
            playerGlowLight.pointLightOuterRadius = maxOuterRadius;
        }

        ShowUpgradeOptions(); 
        StartCoroutine(FadeOutGlowLight());
    }

    private IEnumerator FadeOutGlowLight()
    {
        if (playerGlowLight == null) yield break;
        float timer = 0f;
        while (timer < glowFadeDuration)
        {
            timer += Time.unscaledDeltaTime; 
            float progress = timer / glowFadeDuration;
            playerGlowLight.intensity = Mathf.Lerp(maxGlowIntensity, 0f, progress);
            playerGlowLight.pointLightOuterRadius = Mathf.Lerp(maxOuterRadius, 0f, progress);
            yield return null;
        }
        playerGlowLight.intensity = 0f;
    }
    
    public void ShowUpgradeOptions()
    {
        Time.timeScale = 0; 

        // SFX: Phát âm thanh mở bảng nâng cấp
        if (audioSource != null && upgradePanelOpenSFX != null)
        {
            audioSource.PlayOneShot(upgradePanelOpenSFX, openVolume);
        }

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
            if (cardUI != null) cardUI.Setup(skill);
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
            if (panelCanvasGroup != null) panelCanvasGroup.alpha = easedProgress;
            cardsContainer.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);
            yield return null;
        }

        if (panelCanvasGroup != null) panelCanvasGroup.alpha = 1;
        cardsContainer.localScale = targetScale;
    }

    public void SelectUpgrade(SkillData skill, Transform selectedCardTransform)
    {
        ApplySkillToCurrentPlayer(skill);

        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.UpdatePersistentStats(skill);
        }

        if (selectionBurstPrefab != null)
        {
            ParticleSystem burst = Instantiate(selectionBurstPrefab, selectedCardTransform.position, Quaternion.identity);
            var main = burst.main;
            main.useUnscaledTime = true; 
            burst.Play();
            Destroy(burst.gameObject, 2f); 
        }
        StartCoroutine(DelayedClose()); 
    }

    private void ApplySkillToCurrentPlayer(SkillData skill)
    {
        switch (skill.upgradeType)
        {
            case SkillData.SkillType.HealthUp:
                playerHealth.IncreaseMaxHealth((int)skill.valueAmount); break;
            case SkillData.SkillType.DamageUp:
                playerCombat.damage1 += (int)skill.valueAmount;
                playerCombat.damage2 += (int)skill.valueAmount;
                playerCombat.damage3 += (int)skill.valueAmount; break;
            case SkillData.SkillType.DashCooldown:
                playerMovement.dashCooldown = Mathf.Max(0.1f, playerMovement.dashCooldown - skill.valueAmount); break;
            case SkillData.SkillType.SpeedUp:
                playerMovement.speedMultiplier += skill.valueAmount; break;
            case SkillData.SkillType.LifeSteal:
                playerCombat.lifestealChance += skill.secondValue; break;
            case SkillData.SkillType.SkillDamage:
                playerCombat.skillDamageBonus += (int)skill.valueAmount; break;
            case SkillData.SkillType.HealthRegen:
                playerHealth.healthRegen += (int)skill.valueAmount; break;
            case SkillData.SkillType.CritChance:
                playerCombat.critChance += skill.valueAmount; break;
        }
    }

    private IEnumerator DelayedClose()
    {
        yield return new WaitForSecondsRealtime(0.15f); 
        CloseUpgradePanel();
    }

    public void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1; 
    }

    List<SkillData> GetRandomSkills(int count)
    {
        List<SkillData> filteredPool = new List<SkillData>(allSkills);

        List<SkillData> result = new List<SkillData>();
        for (int i = 0; i < count; i++)
        {
            if (filteredPool.Count == 0) break;

            SkillData.Rarity targetRarity = (Random.value < 0.2f) ? SkillData.Rarity.Rare : SkillData.Rarity.Common;
            
            List<SkillData> rarityMatch = filteredPool.FindAll(s => s.skillRarity == targetRarity);
            
            SkillData selectedSkill = (rarityMatch.Count > 0) ? rarityMatch[Random.Range(0, rarityMatch.Count)] : filteredPool[0];

            if (selectedSkill != null)
            {
                result.Add(selectedSkill);
                filteredPool.Remove(selectedSkill); 
            }
        }
        return result;
    }
}
