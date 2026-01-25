using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Skill_Summon : MonoBehaviour
{
    [Header("Cài đặt")]
    public BossController boss;
    public SummonSkillData skillData;

    [Header("Cài đặt Môi trường (QUAN TRỌNG)")]
    // [THAY ĐỔI] Dùng Tag thay vì LayerMask
    [Tooltip("Điền đúng tên Tag của tường/vật cản vào đây (VD: Obstacles)")]
    public string obstacleTag = "Obstacles"; 
    
    [Header("Hiệu ứng")]
    public GameObject spawnIndicatorPrefab; 

    public float currentCooldown = 0f;
    public bool IsReady => currentCooldown <= 0;

    private void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    public void ActivateSkill()
    {
        StartCoroutine(ExecuteSummonRoutine());
    }

    // --- LUỒNG 1: QUẢN LÝ HÀNH ĐỘNG CỦA BOSS ---
    private IEnumerator ExecuteSummonRoutine()
    {
        boss.ChangeState(BossState.Attacking);
        boss.rb.velocity = Vector2.zero;
        
        boss.FacePlayer();
        boss.PlayAnim("Cry"); 

        StartCoroutine(ProcessSpawnSequence());

        yield return new WaitForSeconds(skillData.totalCastDuration);

        boss.ChangeState(BossState.Recovering);
        boss.PlayAnim("Idle"); 
        
        yield return new WaitForSeconds(skillData.recoverTime);

        boss.ChangeState(BossState.Idle);
        currentCooldown = skillData.autoCooldown;
    }

    // --- LUỒNG 2: QUẢN LÝ VÒNG TRÒN & QUÁI ---
    private IEnumerator ProcessSpawnSequence()
    {
        foreach (var wave in skillData.minionWaves)
        {
            for (int i = 0; i < wave.count; i++)
            {
                // Tìm vị trí hợp lệ (tránh Tag tường)
                Vector2 spawnPos = GetValidSpawnPosition();

                StartCoroutine(SpawnSingleMinion(wave.prefab, spawnPos));
                
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    // [MỚI] Hàm tìm vị trí spawn hợp lệ dựa trên TAG
    private Vector2 GetValidSpawnPosition()
    {
        int maxAttempts = 15; 
        float checkRadius = 0.5f; 

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * skillData.spawnRadius;

            // 1. Quét tất cả mọi thứ tại điểm đó
            Collider2D[] hits = Physics2D.OverlapCircleAll(randomPoint, checkRadius);

            bool hitObstacle = false;
            
            // 2. Duyệt qua danh sách xem có cái nào là Tường không
            foreach (var hit in hits)
            {
                if (hit.CompareTag(obstacleTag))
                {
                    hitObstacle = true;
                    break; // Dính tường rồi, dừng kiểm tra, thử điểm khác
                }
            }

            // 3. Nếu không dính tường -> Vị trí ngon
            if (!hitObstacle)
            {
                return randomPoint;
            }
        }

        // Nếu thử mãi vẫn dính tường -> Trả về vị trí Boss
        return transform.position;
    }

    private IEnumerator SpawnSingleMinion(GameObject minionPrefab, Vector2 position)
    {
        GameObject indicator = null;
        if (spawnIndicatorPrefab != null)
        {
            indicator = Instantiate(spawnIndicatorPrefab, position, Quaternion.identity);
        }

        yield return new WaitForSeconds(skillData.spawnDelay);

        if (minionPrefab != null)
        {
            Instantiate(minionPrefab, position, Quaternion.identity);
        }

        float remainingTime = skillData.indicatorLifeTime - skillData.spawnDelay;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        if (indicator != null)
        {
            Destroy(indicator);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (skillData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, skillData.spawnRadius);
        }
    }
}