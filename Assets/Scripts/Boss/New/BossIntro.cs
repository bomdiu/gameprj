using UnityEngine;
using System.Collections;

public class BossIntro : MonoBehaviour
{
    [Header("Cài đặt Chính")]
    public BossController boss;
    public BossAI bossAI; 
    
    [Header("Cấu hình Intro")]
    public float roarDuration = 2.0f; // Thời gian gầm
    public float startDelay = 1.0f;   // Chờ 1 chút mới gầm

    [Header("Hệ thống Rung Lắc (Juice)")]
    [Tooltip("Độ mạnh khi rung bản thân Boss")]
    public float bossShakeIntensity = 0.1f; 
    [Tooltip("Độ mạnh khi rung Camera")]
    public float cameraShakeIntensity = 0.3f; 

    [Header("Âm thanh & Effect")]
    public AudioClip roarSFX; // Kéo file âm thanh gầm vào đây
    public GameObject roarEffectPrefab; // Effect gầm (bụi, sóng âm)
    
    private AudioSource audioSource;
    private Vector3 originalPos; // Để lưu vị trí gốc của Boss khi rung

    private void Awake()
    {
        // Tự động thêm AudioSource nếu chưa có
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        // 1. TẮT AI
        if (bossAI != null) bossAI.enabled = false; 

        // 2. Bắt đầu Intro
        StartCoroutine(PlayIntroRoutine());
    }

    private IEnumerator PlayIntroRoutine()
    {
        boss.ChangeState(BossState.Intro);
        boss.rb.velocity = Vector2.zero;

        // Lưu vị trí gốc để tí rung lắc xong trả về
        originalPos = transform.localPosition;

        yield return new WaitForSeconds(startDelay);

        // --- BẮT ĐẦU GẦM ---
        Debug.Log("🦁 BOSS ROAR!");
        boss.PlayAnim("RoarIntro"); 
        boss.FacePlayer();     

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

        // 3. Spawn Effect (Bụi/Sóng âm)
        if (roarEffectPrefab != null)
        {
            Instantiate(roarEffectPrefab, transform.position, Quaternion.identity);
        }

        // 4. Rung bản thân Boss (Chạy song song)
        StartCoroutine(ShakeBossBody(roarDuration));

        // Chờ diễn hoạt xong
        yield return new WaitForSeconds(roarDuration);

        // --- VÀO TRẬN ---
        Debug.Log("⚔️ FIGHT START!");
        
        // Đảm bảo trả boss về vị trí cũ (phòng khi rung bị lệch)
        transform.localPosition = originalPos;

        boss.ChangeState(BossState.Idle);

        if (bossAI != null) bossAI.enabled = true;

        Destroy(this); // Hủy script Intro
    }

    // Coroutine rung lắc bản thân Boss
    private IEnumerator ShakeBossBody(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Rung ngẫu nhiên xung quanh vị trí gốc
            float x = Random.Range(-1f, 1f) * bossShakeIntensity;
            float y = Random.Range(-1f, 1f) * bossShakeIntensity;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}