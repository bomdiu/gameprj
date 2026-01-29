using UnityEngine;
using System.Collections;

public class BossIntro : MonoBehaviour
{
    [Header("Cài đặt Chính")]
    public BossController boss;
    public BossAI bossAI; 
    
    [Header("Cấu hình Intro")]
    public float roarDuration = 2.0f; 
    public float startDelay = 0.5f;   // Giảm delay xuống chút vì hội thoại đã dài rồi

    [Header("Hệ thống Rung Lắc (Juice)")]
    public float bossShakeIntensity = 0.1f; 
    public float cameraShakeIntensity = 0.3f; 

    [Header("Âm thanh & Effect")]
    public AudioClip roarSFX; 
    public GameObject roarEffectPrefab; 
    
    [Header("Debug")]
    public bool testIntroOnStart = false; // Tích vào nếu muốn test Intro mà ko cần hội thoại

    private AudioSource audioSource;
    private Vector3 originalPos; 

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        // 1. KHÓA AI NGAY LẬP TỨC
        // Đảm bảo Boss đứng im nhìn Player trong lúc Player đang đọc hội thoại
        if (bossAI != null) bossAI.enabled = false; 
        if (boss != null) boss.rb.velocity = Vector2.zero;

        // Lưu vị trí gốc
        originalPos = transform.localPosition;

        // Chỉ chạy ngay nếu đang test (Debug)
        if (testIntroOnStart)
        {
            StartIntroSequence();
        }
    }

    // --- HÀM PUBLIC MỚI ĐỂ GỌI TỪ DIALOGUE MANAGER ---
    public void StartIntroSequence()
    {
        StartCoroutine(PlayIntroRoutine());
    }

    private IEnumerator PlayIntroRoutine()
    {
        // Chờ 1 chút sau khi hộp thoại tắt để đỡ bị giật
        yield return new WaitForSeconds(startDelay);

        // Chuyển State sang Intro (để animation Roar chạy được nếu Animator set điều kiện)
        if(boss != null) 
        {
            boss.ChangeState(BossState.Intro);
            boss.FacePlayer();    
            boss.PlayAnim("RoarIntro"); 
            Debug.Log("🦁 BOSS ROAR!");
        }

        // 1. Rung Màn Hình
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(roarDuration, cameraShakeIntensity);
        }

        // 2. Phát Âm Thanh
        if (roarSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(roarSFX);
        }

        // 3. Spawn Effect
        if (roarEffectPrefab != null)
        {
            Instantiate(roarEffectPrefab, transform.position, Quaternion.identity);
        }

        // 4. Rung bản thân Boss
        StartCoroutine(ShakeBossBody(roarDuration));

        // Chờ diễn hoạt xong
        yield return new WaitForSeconds(roarDuration);

        // --- VÀO TRẬN ---
        Debug.Log("⚔️ FIGHT START!");
        
        // Trả về vị trí cũ
        transform.localPosition = originalPos;

        if (boss != null) boss.ChangeState(BossState.Idle);
        if (bossAI != null) bossAI.enabled = true;

        // Nếu có thanh máu Boss (BossHealthBar UI), bạn nên bật nó lên ở dòng này
        // Example: UIManager.Instance.ShowBossHealth(true);

        Destroy(this); // Hủy script Intro để tiết kiệm bộ nhớ
    }

    private IEnumerator ShakeBossBody(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * bossShakeIntensity;
            float y = Random.Range(-1f, 1f) * bossShakeIntensity;

            // Cộng vào originalPos để không bị trôi boss đi xa
            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}