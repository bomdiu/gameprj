using UnityEngine;
using System.Collections;

public class Skill_Explosion : MonoBehaviour
{
    [Header("Cài đặt")]
    public BossController boss;
    public ExplosionSkillData skillData;
    public GameObject indicatorPrefab; // Prefab rỗng có gắn script ExplosionIndicator

    private float currentCooldown = 0f;

    private void Update()
    {
        if (currentCooldown > 0)
            currentCooldown -= Time.deltaTime;
        else if (boss.CanAttack())
            ActivateSkill();
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
            
            // Tạo Indicator
            GameObject newMine = Instantiate(indicatorPrefab, spawnPos, Quaternion.identity);
            
            // Setup dữ liệu
            ExplosionIndicator mineScript = newMine.GetComponent<ExplosionIndicator>();
            if (mineScript != null)
            {
                mineScript.Setup(skillData);
            }

            // Đợi trước khi rải quả tiếp theo
            yield return new WaitForSeconds(skillData.spawnInterval);
        }

        boss.ChangeState(BossState.Recovering);
        boss.PlayAnim("Idle");

        yield return new WaitForSeconds(skillData.bossRecoverTime);

        boss.ChangeState(BossState.Idle);
        currentCooldown = skillData.autoCooldown;
    }
}