using UnityEngine;
using System.Collections;

public class Skill_Explosion : MonoBehaviour
{
    [Header("Cài đặt")]
    public BossController boss;
    public ExplosionSkillData skillData;
    public GameObject indicatorPrefab; // Prefab rỗng có gắn script ExplosionIndicator

    [Header("Audio")] // [MỚI] Phần âm thanh
    public AudioClip explosionSFX; // Kéo file âm thanh nổ (Bùm!) vào đây
    private AudioSource audioSource;

    [Header("Damage Settings")]
    public LayerMask targetLayer; // Chọn Layer "Player" ở đây

    public float currentCooldown = 0f;
    public bool IsReady => currentCooldown <= 0;

    private void Awake()
    {
        // [MỚI] Tự động tìm hoặc thêm AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (currentCooldown > 0)
            currentCooldown -= Time.deltaTime;
    }

    public void ActivateSkill()
    {
        StartCoroutine(ExecuteExplosionSequence());
    }

    private IEnumerator ExecuteExplosionSequence()
    {
        boss.ChangeState(BossState.Attacking);
        boss.rb.velocity = Vector2.zero;
        boss.FacePlayer();
        boss.PlayAnim("Roar");

        // Rải từng quả mìn
        for (int i = 0; i < skillData.explosionCount; i++)
        {
            Vector2 spawnPos = boss.player.position; // Lấy vị trí Player hiện tại
            
            // 1. Tạo Indicator (Hiệu ứng cảnh báo)
            GameObject newMine = Instantiate(indicatorPrefab, spawnPos, Quaternion.identity);
            
            // Setup dữ liệu visual
            ExplosionIndicator mineScript = newMine.GetComponent<ExplosionIndicator>();
            if (mineScript != null)
            {
                mineScript.Setup(skillData);
            }

            // 2. Bắt đầu đếm ngược để gây Damage (Logic chạy ngầm song song với Visual)
            StartCoroutine(HandleExplosionDamage(spawnPos));

            // Đợi trước khi rải quả tiếp theo
            yield return new WaitForSeconds(skillData.spawnInterval);
        }

        boss.ChangeState(BossState.Recovering);
        boss.PlayAnim("Idle");

        yield return new WaitForSeconds(skillData.bossRecoverTime);

        boss.ChangeState(BossState.Idle);
        currentCooldown = skillData.autoCooldown;
    }

    // --- HÀM MỚI: XỬ LÝ GÂY SÁT THƯƠNG ---
    private IEnumerator HandleExplosionDamage(Vector2 position)
    {
        // Chờ thời gian nổ (phải khớp với thời gian visual bên Indicator)
        yield return new WaitForSeconds(skillData.explosionDelay);

        // [MỚI] PHÁT TIẾNG NỔ NGAY LÚC GÂY DAMAGE
        if (explosionSFX != null && audioSource != null)
        {
            // Dùng PlayOneShot để có thể phát chồng nhiều tiếng nổ lên nhau mà không bị ngắt quãng
            audioSource.PlayOneShot(explosionSFX);
        }

        // Tạo vòng tròn kiểm tra va chạm tại vị trí đặt mìn
        Collider2D hit = Physics2D.OverlapCircle(position, skillData.explosionRadius, targetLayer);

        if (hit != null)
        {
            // Tìm component máu của Player (PlayerStats hoặc PlayerHealth)
            PlayerStats player = hit.GetComponent<PlayerStats>();
            if (player != null)
            {
                player.TakeDamage(skillData.damage);
            }
        }
    }
    
    // Vẽ vòng tròn nổ trong Scene để dễ căn chỉnh (Optional)
    private void OnDrawGizmosSelected()
    {
        if (skillData != null)
        {
            Gizmos.color = Color.red;
            // Vẽ minh họa tại vị trí boss đứng
            Gizmos.DrawWireSphere(transform.position, skillData.explosionRadius);
        }
    }
}