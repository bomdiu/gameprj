using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Skill_Summon : MonoBehaviour
{
    [Header("Cài đặt")]
    public BossController boss;
    public SummonSkillData skillData;
    
    [Header("Hiệu ứng")]
    public GameObject spawnIndicatorPrefab; // Prefab vòng tròn

    private float currentCooldown = 0f;

    private void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
        else 
        {
            if (boss.CanAttack())
            {
                ActivateSkill();
            }
        }
    }

    public void ActivateSkill()
    {
        StartCoroutine(ExecuteSummonRoutine());
    }

    // --- LUỒNG 1: QUẢN LÝ HÀNH ĐỘNG CỦA BOSS ---
    private IEnumerator ExecuteSummonRoutine()
    {
        // 1. Boss bắt đầu diễn
        boss.ChangeState(BossState.Attacking);
        boss.rb.velocity = Vector2.zero;
        
        boss.FacePlayer();
        boss.PlayAnim("Cry"); // Boss bắt đầu khóc

        // 2. Kích hoạt quy trình gọi đệ (Chạy song song, không bắt Boss đợi)
        StartCoroutine(ProcessSpawnSequence());

        // 3. Boss cứ khóc cho đến hết thời gian quy định (totalCastDuration)
        // Dù quái ra xong hay chưa thì Boss vẫn khóc cho đủ diễn xuất
        yield return new WaitForSeconds(skillData.totalCastDuration);

        // 4. Boss nghỉ mệt
        boss.ChangeState(BossState.Recovering);
        boss.PlayAnim("Idle"); // Về Idle thở
        
        yield return new WaitForSeconds(skillData.recoverTime);

        // 5. Xong
        boss.ChangeState(BossState.Idle);
        currentCooldown = skillData.autoCooldown;
    }

    // --- LUỒNG 2: QUẢN LÝ VÒNG TRÒN & QUÁI ---
    private IEnumerator ProcessSpawnSequence()
    {
        // Duyệt qua từng loại quái trong danh sách
        foreach (var wave in skillData.minionWaves)
        {
            for (int i = 0; i < wave.count; i++)
            {
                // Tính vị trí ngẫu nhiên cho con quái này
                Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * skillData.spawnRadius;

                // Gọi hàm xử lý riêng cho từng con (để chúng nó tự quản lý thời gian của mình)
                StartCoroutine(SpawnSingleMinion(wave.prefab, spawnPos));
                
                // (Tùy chọn) Thêm delay nhỏ giữa các con để chúng không hiện ra cùng 1 frame (nhìn tự nhiên hơn)
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    // Hàm xử lý vòng đời của 1 lần spawn: Vòng tròn hiện -> Đợi -> Quái hiện -> Vòng tròn mất
    private IEnumerator SpawnSingleMinion(GameObject minionPrefab, Vector2 position)
    {
        // 1. Hiện vòng tròn (Telegraph)
        GameObject indicator = null;
        if (spawnIndicatorPrefab != null)
        {
            indicator = Instantiate(spawnIndicatorPrefab, position, Quaternion.identity);
            // Có thể set parent là null để nó nằm im dưới đất, không chạy theo boss
        }

        // 2. Đợi đến "thời điểm vàng" để quái xuất hiện
        // (Ví dụ: Vòng tròn tồn tại 1.5s, nhưng giây thứ 1.0 là quái hiện ra)
        yield return new WaitForSeconds(skillData.spawnDelay);

        // 3. Spawn Quái
        if (minionPrefab != null)
        {
            Instantiate(minionPrefab, position, Quaternion.identity);
            // Thêm VFX nổ/bụi ở đây nếu muốn
        }

        // 4. Đợi nốt thời gian còn lại của vòng tròn (nếu có) rồi xóa vòng tròn
        float remainingTime = skillData.indicatorLifeTime - skillData.spawnDelay;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // 5. Xóa vòng tròn
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