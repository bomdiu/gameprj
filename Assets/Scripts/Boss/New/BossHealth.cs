using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; // [QUAN TRỌNG] Cần thư viện này để làm Fade màn hình

public class BossHealth : MonoBehaviour
{
    // =========================================================
    // PHẦN CŨ CỦA BẠN (GIỮ NGUYÊN KHÔNG ĐỤNG CHẠM)
    // =========================================================
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
    [Tooltip("Tỷ lệ bẹt (X rộng ra, Y lùn đi)")]
    [SerializeField] private Vector3 squashScale = new Vector3(1.1f, 0.9f, 1f); 
    [Tooltip("Thời gian giữ trạng thái bẹt (giây)")]
    [SerializeField] private float squashHoldDuration = 0.05f; 
    [Tooltip("Tốc độ nảy lại hình dạng ban đầu")]
    [SerializeField] private float morphRestoreSpeed = 5f;

    // =========================================================
    // PHẦN MỚI: CẤU HÌNH DEATH & OUTRO (ĐÃ BỔ SUNG)
    // =========================================================
    [Header("Death Sequence & Outro")]
    [SerializeField] private GameObject deathParticlePrefab;
    
    [Tooltip("Thời gian để Boss diễn hết Animation chết (Hãy chỉnh khớp với clip Animation)")]
    [SerializeField] private float deathDestroyDelay = 3.0f; // Dùng biến cũ của bạn làm thời gian Anim

    [Tooltip("Thời gian chờ hiệu ứng nổ diễn ra trước khi bắt đầu Fade màn hình")]
    public float explosionDuration = 1.0f; // [MỚI]

    [Tooltip("Thời gian màn hình trắng dần")]
    public float fadeDuration = 2.0f; // [MỚI]
    
    [Tooltip("Màu chuyển cảnh (Mặc định là Trắng)")]
    public Color fadeColor = Color.white; // [MỚI]

    [Tooltip("Tên màn chơi Outro (nhớ Add vào Build Settings)")]
    public string outroSceneName = "Outro"; 

    [Header("Visual Orientation")]
    [SerializeField] private bool isFacingRightByDefault = true; 

    [Header("Live Stats")]
    public float currentHealth; 
    
    // References
    private DamageFlash damageFlash;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BossHealthUI healthUI; 

    private Vector3 originalScale;
    private Coroutine morphCoroutine;
    private bool isDead = false; 

    // --- COLLISION SETTINGS ---
    [Header("Collision Settings")]
    [Tooltip("Tag của vũ khí/đạn của Player (VD: 'PlayerAttack')")]
    public string playerWeaponTag = "PlayerAttack"; 

    // Logic Phase
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

        if (sr != null) originalScale = sr.transform.localScale;
        else originalScale = transform.localScale;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthUI != null) healthUI.UpdateHealth(currentHealth, maxHealth);
    }

    // --- MAIN DAMAGE FUNCTION ---
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        float damageToApply = Mathf.Abs(amount);
        if (damageToApply <= 0) return;

        // 1. Apply Logic
        currentHealth = Mathf.Max(currentHealth - damageToApply, 0);

        // 2. Update UI
        if (healthUI != null) healthUI.UpdateHealth(currentHealth, maxHealth);

        // 3. Visual Feedback (Juice)
        ShowDamagePopup(Mathf.RoundToInt(damageToApply));
        if (damageFlash != null) damageFlash.Flash();
        if (HitStopManager.Instance != null) HitStopManager.Instance.HitStop(hitstopDuration);
        ApplyMorphEffect();

        // 4. Check Death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // --- COLLISION DAMAGE ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag(playerWeaponTag))
        {
            float damageToTake = 10f; 
            TakeDamage(damageToTake);
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("BOSS DEAD!");

        // 1. Disable Physics & Logic
        if (rb != null) rb.velocity = Vector2.zero;
        
        // Disable AI Scripts
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            if (s != this) s.enabled = false;
        }

        // 2. Start Death Sequence
        StartCoroutine(DeathSequenceRoutine());
    }

    // =========================================================
    // PHẦN SỬA ĐỔI: TRÌNH TỰ CHẾT -> NỔ -> FADE -> OUTRO
    // =========================================================
    private IEnumerator DeathSequenceRoutine()
    {
        // BƯỚC 1: Slow Motion & Animation Chết
        Time.timeScale = 0.2f; 

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) 
        {
            anim.SetTrigger("Death");
        }
        
        // Nháy flash liên tục trong lúc giãy chết
        if (damageFlash != null) damageFlash.FlashIndefinitely();

        // CHỜ ANIMATION: Dùng Realtime để chờ đúng số giây (deathDestroyDelay) bất chấp slow motion
        // Đây là lúc Boss đang diễn hoạt cảnh ngã xuống
        yield return new WaitForSecondsRealtime(deathDestroyDelay);

        // BƯỚC 2: ANIMATION XONG RỒI -> MỚI NỔ BÙM
        Time.timeScale = 1f; // Trả lại tốc độ game bình thường

        // Sinh hiệu ứng nổ
        if (deathParticlePrefab != null) 
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }

        // Ẩn Boss đi (Chỉ tắt hình ảnh, không Destroy object vội)
        if (sr != null) sr.enabled = false;
        if (damageFlash != null) StopAllCoroutines(); // Tắt nháy

        // Chờ một chút cho người chơi ngắm hiệu ứng nổ
        yield return new WaitForSeconds(explosionDuration);

        // BƯỚC 3: FADE MÀN HÌNH TRẮNG
        yield return StartCoroutine(FadeScreenRoutine());

        // BƯỚC 4: CHUYỂN CẢNH OUTRO
        Debug.Log("Loading Outro: " + outroSceneName);
        if (Application.CanStreamedLevelBeLoaded(outroSceneName))
        {
            SceneManager.LoadScene(outroSceneName);
        }
        else
        {
            Debug.LogError("❌ Scene '" + outroSceneName + "' chưa được thêm vào Build Settings!");
        }
        
        // Hủy object Boss
        Destroy(gameObject); 
    }

    // --- HÀM MỚI: TỰ TẠO MÀN HÌNH FADE TRẮNG ---
    private IEnumerator FadeScreenRoutine()
    {
        // Tạo Canvas tạm thời
        GameObject canvasObj = new GameObject("TempFadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Đè lên tất cả
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Tạo tấm ảnh phủ kín màn hình
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        Image fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f); // Bắt đầu trong suốt
        
        // Stretch full màn hình
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        // Bắt đầu Fade từ 0 -> 1
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        // Giữ nguyên màu đặc một chút
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        yield return new WaitForSecondsRealtime(0.5f);
    }

    // --- VISUAL EFFECTS (Squash & Stretch) - GIỮ NGUYÊN ---
    private void ApplyMorphEffect()
    {
        if (morphCoroutine != null) StopCoroutine(morphCoroutine);
        morphCoroutine = StartCoroutine(MorphRoutine());
    }

    private IEnumerator MorphRoutine()
    {
        Transform targetTransform = (sr != null) ? sr.transform : transform;

        // Squash
        targetTransform.localScale = Vector3.Scale(originalScale, squashScale);
        yield return new WaitForSecondsRealtime(squashHoldDuration);

        // Return to normal
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

    // --- DEV TOOLS ---
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(50);
        }
    }
}