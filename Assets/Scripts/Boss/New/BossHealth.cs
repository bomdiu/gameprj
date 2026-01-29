using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 

public class BossHealth : MonoBehaviour
{
    // ==========================================
    [Header("Stats Configuration")]
    public int maxHealth = 1000;

    [Header("Visual Settings")]
    [Tooltip("The sorting layer the boss moves to when dying (e.g., 'Foreground')")]
    public string deathSortingLayer = "Foreground"; 

    [Header("Settings")]
    public GameObject damageTextPrefab; 

    [Header("Juice Settings")]
    [SerializeField] private float hitstopDuration = 0.08f; 
    [SerializeField] private float knockbackForce = 0f;     
    [SerializeField] private float knockbackStunTime = 0.0f; 

    [Header("Morph (Squash & Stretch)")]
    [SerializeField] private Vector3 squashScale = new Vector3(1.1f, 0.9f, 1f); 
    [SerializeField] private float squashHoldDuration = 0.05f; 
    [SerializeField] private float morphRestoreSpeed = 5f;

    [Header("Death Juice & Outro")]
    [SerializeField] private GameObject deathParticlePrefab;
    [SerializeField] private float deathDestroyDelay = 3.0f; 
    public string outroSceneName = "Outro"; 

    [Header("Audio Settings")] // MỚI: Quản lý âm thanh cho Boss
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bossHurtSFX;
    [SerializeField] private AudioClip bossDeathExplosionSFX;

    [Header("Visual Orientation")]
    [SerializeField] private bool isFacingRightByDefault = true; 

    [Header("Live Stats")]
    public float currentHealth; 
    
    private DamageFlash damageFlash;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BossHealthUI healthUI; 

    private Vector3 originalScale;
    private Coroutine morphCoroutine;
    private bool isDead = false; 

    [Header("Collision Settings")]
    public string playerWeaponTag = "PlayerCombat"; 

    [Header("Fade Effect Settings")]
    public float explosionDuration = 1.0f; 
    public float fadeDuration = 2.0f; 
    public Color fadeColor = Color.white; 

    public bool IsPhase2 
    {
        get 
        {
            if (currentHealth <= 0) return false;
            return currentHealth <= (maxHealth * 0.5f);
        }
    }

    private void Awake()
    {
        damageFlash = GetComponentInChildren<DamageFlash>(); 
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        healthUI = FindObjectOfType<BossHealthUI>(); 

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (sr != null) originalScale = sr.transform.localScale;
        else originalScale = transform.localScale;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthUI != null) healthUI.UpdateHealth(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        float damageToApply = Mathf.Abs(amount);
        if (damageToApply <= 0) return;

        // MỚI: Phát âm thanh nhận sát thương
        if (audioSource != null && bossHurtSFX != null)
        {
            audioSource.PlayOneShot(bossHurtSFX, 1.2f);
        }

        currentHealth = Mathf.Max(currentHealth - damageToApply, 0);

        if (healthUI != null) healthUI.UpdateHealth(currentHealth, maxHealth);

        ShowDamagePopup(Mathf.RoundToInt(damageToApply));
        if (damageFlash != null) damageFlash.Flash();
        if (HitStopManager.Instance != null) HitStopManager.Instance.HitStop(hitstopDuration);
        ApplyMorphEffect();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;
        if (collision.CompareTag("Obstacles")) return;
        if (collision.transform.IsChildOf(transform)) return;

        if (collision.CompareTag(playerWeaponTag))
        {
            PlayerCombat combat = collision.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                float damageToTake = combat.GetCurrentDamage(); 
                TakeDamage(damageToTake);
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        BossMusicController music = FindObjectOfType<BossMusicController>();
        if (music != null) music.StopMusic();
        
        if (rb != null) rb.velocity = Vector2.zero;
        
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            if (s != this) s.enabled = false;
        }

        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        Time.timeScale = 0.2f; 

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) 
        {
            anim.SetTrigger("Death");
        }

        if (damageFlash != null) 
        {
            damageFlash.StopAllCoroutines(); 
        }

        yield return new WaitForSecondsRealtime(deathDestroyDelay);

        Time.timeScale = 1f; 

        // MỚI: Phát âm thanh nổ khi Boss tan biến
        if (bossDeathExplosionSFX != null)
        {
            // Dùng PlayClipAtPoint để âm thanh không bị mất khi chuyển cảnh/xóa Boss
            AudioSource.PlayClipAtPoint(bossDeathExplosionSFX, transform.position);
        }

        if (deathParticlePrefab != null) 
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }

        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(explosionDuration);

        yield return StartCoroutine(FadeScreenRoutine());

        if (Application.CanStreamedLevelBeLoaded(outroSceneName))
        {
            SceneManager.LoadScene(outroSceneName);
        }
        else
        {
            Debug.LogError("Scene '" + outroSceneName + "' chưa được thêm vào Build Settings!");
            Destroy(gameObject);
        }
    }

    private IEnumerator FadeScreenRoutine()
    {
        GameObject canvasObj = new GameObject("TempFadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; 
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        Image fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f); 
        
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        yield return new WaitForSecondsRealtime(0.2f);
    }

    private void ApplyMorphEffect()
    {
        if (morphCoroutine != null) StopCoroutine(morphCoroutine);
        morphCoroutine = StartCoroutine(MorphRoutine());
    }

    private IEnumerator MorphRoutine()
    {
        Transform targetTransform = (sr != null) ? sr.transform : transform;

        targetTransform.localScale = Vector3.Scale(originalScale, squashScale);
        yield return new WaitForSecondsRealtime(squashHoldDuration);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime * morphRestoreSpeed;
            targetTransform.localScale = Vector3.Lerp(targetTransform.localScale, originalScale, elapsed);
            yield return null;
        }

        targetTransform.localScale = originalScale;
    }

    void ShowDamagePopup(int amount)
    {
        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 2f; 
            spawnPos.z = -1f; 
            GameObject textInstance = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            
            DamagePopup popupScript = textInstance.GetComponent<DamagePopup>();
            if (popupScript != null) popupScript.Setup(amount, Color.red); 
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(50);
        }
    }
}