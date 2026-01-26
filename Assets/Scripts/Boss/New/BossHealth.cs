using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Cần thiết để chuyển cảnh
using UnityEngine.UI; // [MỚI] Cần thiết để làm hiệu ứng Fade màn hình

public class BossHealth : MonoBehaviour
{
    // ==========================================
    // PHẦN CŨ (GIỮ NGUYÊN 100% LOGIC)
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
    [Tooltip("Tỷ lệ bẹt (X rộng ra, Y lùn đi)")]
    [SerializeField] private Vector3 squashScale = new Vector3(1.1f, 0.9f, 1f); 
    [Tooltip("Thời gian giữ trạng thái bẹt (giây)")]
    [SerializeField] private float squashHoldDuration = 0.05f; 
    [Tooltip("Tốc độ nảy lại hình dạng ban đầu")]
    [SerializeField] private float morphRestoreSpeed = 5f;

    [Header("Death Juice & Outro")]
    [SerializeField] private GameObject deathParticlePrefab;
    [Tooltip("Thời gian để Boss diễn hết Animation chết trước khi Nổ")]
    [SerializeField] private float deathDestroyDelay = 3.0f; 
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

    [Header("Collision Settings")]
    [Tooltip("Tag của vũ khí/đạn của Player (VD: 'PlayerAttack')")]
    public string playerWeaponTag = "PlayerCombat"; 

    // ==========================================
    // [MỚI] CÁC BIẾN CHO FADE EFFECT
    // ==========================================
    [Header("Fade Effect Settings")]
    [Tooltip("Thời gian chờ hiệu ứng nổ diễn ra xong mới bắt đầu Fade")]
    public float explosionDuration = 1.0f; 
    [Tooltip("Thời gian màn hình chuyển dần sang trắng")]
    public float fadeDuration = 2.0f; 
    [Tooltip("Màu Fade (Trắng hoặc Đen)")]
    public Color fadeColor = Color.white; 

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

    // --- COLLISION DAMAGE ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 0. Nếu chết rồi thì thôi
        if (isDead) return;

        // 1. [FIX QUAN TRỌNG] Bỏ qua Tường và các vật cản (Obstacles)
        // Dòng này ngăn boss tự hủy khi đâm vào tường
        if (collision.CompareTag("Obstacles")) return;
        
        // 2. [FIX PHỤ] Bỏ qua chính các bộ phận của Boss (Hitbox con, Radar...)
        // Để tránh việc Hitbox của skill Dash tự gây damage cho Boss
        if (collision.transform.IsChildOf(transform)) return;

        // 3. Chỉ nhận damage nếu đúng là Vũ khí Player
        if (collision.CompareTag(playerWeaponTag))
        {
            // Thử lấy damage từ script vũ khí (nếu có), nếu không thì mặc định 10
            PlayerCombat combat = collision.GetComponent<PlayerCombat>();
                if (combat != null)
                {
                    // Use the method to get damage
                    float damageToTake = combat.GetCurrentDamage(); 
                    TakeDamage(damageToTake);
                }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("BOSS DEAD!");

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

    // =========================================================
    // PHẦN LOGIC CHẾT (ĐÃ SỬA THEO YÊU CẦU CỦA BẠN)
    // =========================================================
    private IEnumerator DeathSequenceRoutine()
    {
        // 1. Slow Motion
        Time.timeScale = 0.2f; 

        // 2. Play Death Animation
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) 
        {
            anim.SetTrigger("Death");
        }

        // [FIX LỖI FLASH] Dừng ngay việc nháy trắng để Boss hiện rõ animation chết
        if (damageFlash != null) 
        {
            damageFlash.StopAllCoroutines(); // Dừng Coroutine flash
            // Nếu script DamageFlash của bạn có hàm ResetColor, hãy gọi nó. 
            // Nếu không, dòng trên là đủ để nó không Flash đè lên nữa.
        }

        // 3. CHỜ ANIMATION (Sử dụng deathDestroyDelay làm thời gian chờ Anim)
        // Dùng Realtime để chờ đúng giây thực tế dù game đang Slow Motion
        yield return new WaitForSecondsRealtime(deathDestroyDelay);

        // 4. ANIMATION XONG -> TRẢ TỐC ĐỘ GAME -> NỔ
        Time.timeScale = 1f; 

        // Sinh hiệu ứng nổ (Death Effect) SAU KHI animation xong
        if (deathParticlePrefab != null) 
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }

        // Tắt hình ảnh Boss đi (biến mất để nhường chỗ cho hiệu ứng nổ)
        if (sr != null) sr.enabled = false;

        // Chờ một chút cho hiệu ứng nổ tỏa sáng (explosionDuration)
        yield return new WaitForSeconds(explosionDuration);

        // 5. FADE MÀN HÌNH (SÁNG DẦN)
        yield return StartCoroutine(FadeScreenRoutine());

        // 6. CHUYỂN CẢNH OUTRO
        if (Application.CanStreamedLevelBeLoaded(outroSceneName))
        {
            SceneManager.LoadScene(outroSceneName);
        }
        else
        {
            Debug.LogError("❌ Scene '" + outroSceneName + "' chưa được thêm vào Build Settings!");
            Destroy(gameObject); // Fallback nếu lỗi
        }
    }

    // --- HÀM MỚI: TỰ TẠO HIỆU ỨNG FADE ---
    private IEnumerator FadeScreenRoutine()
    {
        // Tạo Canvas tạm thời
        GameObject canvasObj = new GameObject("TempFadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Đè lên tất cả
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Tạo ảnh Fade
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        Image fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f); // Bắt đầu trong suốt
        
        // Stretch full màn hình
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        // Chạy hiệu ứng Fade In
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        // Giữ màn hình đặc 1 chút trước khi load scene
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        yield return new WaitForSecondsRealtime(0.2f);
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