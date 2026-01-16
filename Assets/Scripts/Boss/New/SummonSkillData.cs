using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSummonData", menuName = "Boss/Summon Data")]
public class SummonSkillData : ScriptableObject
{
    // Class con để định nghĩa 1 loại quái
    [System.Serializable]
    public class MinionConfig
    {
        public string name;           // Đặt tên cho dễ nhớ (VD: Orc, Goblin)
        public GameObject prefab;     // Prefab quái
        public int count = 1;         // Số lượng muốn gọi
    }

    [Header("Cấu hình Quái (Nhiều loại)")]
    public List<MinionConfig> minionWaves; // Danh sách các loại quái sẽ gọi
    public float spawnRadius = 3.0f;

    [Header("Thời gian Boss (Boss Timeline)")]
    public float totalCastDuration = 3.0f; // Tổng thời gian Boss đứng khóc (độc lập với việc quái ra nhanh hay chậm)
    public float recoverTime = 2.0f;
    public float autoCooldown = 10.0f;

    [Header("Đồng bộ Minion & Vòng tròn (Minion Timeline)")]
    public float indicatorLifeTime = 1.5f; // Tổng thời gian vòng tròn tồn tại
    [Tooltip("Quái sẽ xuất hiện sau bao nhiêu giây kể từ khi vòng tròn hiện ra?")]
    public float spawnDelay = 1.0f;        // Thời điểm quái xuất hiện (nên nhỏ hơn indicatorLifeTime)
}