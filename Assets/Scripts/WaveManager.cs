using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyGroup
{
    public string enemyName; // Tên để dễ nhớ (ví dụ: Slime)
    public GameObject enemyPrefab; // Kéo prefab quái vào đây
    public int enemyCount; // Số lượng cần spawn
}

[System.Serializable]
public class Wave
{
    public string waveName;
    public List<EnemyGroup> groups; // Danh sách các loại quái trong wave này
    public List<Transform> spawnPoints; // Các điểm spawn dành riêng cho wave này
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Cấu hình Waves")]
    public List<Wave> waves; // Chứa thông tin 3 đợt quái của bạn
    public float timeBetweenWaves = 5f; // Thời gian nghỉ giữa các đợt

    [Header("Phần thưởng")]
    public GameObject rewardItemPrefab; // Prefab cái rương/item rơi ra
    public Transform rewardSpawnPoint; // Vị trí rương sẽ rơi

    private int currentWaveIndex = 0;
    private int enemiesRemaining = 0; // Đếm số quái đang sống
    private bool isWaveActive = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Bắt đầu wave đầu tiên ngay khi vào game (hoặc gọi hàm này khi bạn muốn)
        StartCoroutine(StartNextWave(0));
    }

    IEnumerator StartNextWave(int index)
    {
        yield return new WaitForSeconds(2f); // Chờ xíu cho người chơi chuẩn bị

        if (index >= waves.Count)
        {
            // HẾT GAME / CHIẾN THẮNG
            Debug.Log("Đã clear hết mọi wave!");
            SpawnReward();
            yield break;
        }

        Debug.Log("Bắt đầu Wave: " + (index + 1));
        currentWaveIndex = index;
        SpawnWave(waves[index]);
    }

    void SpawnWave(Wave wave)
    {
        isWaveActive = true;
        enemiesRemaining = 0;

        // 1. Tính tổng số quái để theo dõi
        foreach (var group in wave.groups)
        {
            enemiesRemaining += group.enemyCount;
        }

        // 2. Spawn từng loại quái
        foreach (var group in wave.groups)
        {
            for (int i = 0; i < group.enemyCount; i++)
            {
                SpawnEnemy(group.enemyPrefab, wave.spawnPoints);
            }
        }
    }

    void SpawnEnemy(GameObject prefab, List<Transform> points)
    {
        if (points.Count == 0) return;

        // Chọn ngẫu nhiên 1 điểm trong danh sách spawnpoint của wave đó
        int randomIndex = Random.Range(0, points.Count);
        Transform spawnPoint = points[randomIndex];

        // Sinh ra quái
        Instantiate(prefab, spawnPoint.position, Quaternion.identity);
    }

    // --- HÀM NÀY ĐỂ QUÁI GỌI KHI CHẾT ---
    public void OnEnemyKilled()
    {
        if (!isWaveActive) return;

        enemiesRemaining--;
        Debug.Log("Quái chết! Còn lại: " + enemiesRemaining);

        if (enemiesRemaining <= 0)
        {
            // Clear xong wave hiện tại
            Debug.Log("Wave Clear!");
            isWaveActive = false;
            
            // Chuyển sang wave tiếp theo
            StartCoroutine(StartNextWave(currentWaveIndex + 1));
        }
    }

    void SpawnReward()
    {
        if (rewardItemPrefab != null)
        {
            Instantiate(rewardItemPrefab, rewardSpawnPoint.position, Quaternion.identity);
            Debug.Log("Rơi vật phẩm nâng cấp!");
        }
    }
}