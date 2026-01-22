using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [System.Serializable]
    public class EnemyTypeConfig
    {
        public GameObject prefab;
        public int maxAmountInWave;
        [HideInInspector] public int currentSpawnedCount;
    }

    [System.Serializable]
    public class WaveData
    {
        public string waveName;
        public List<EnemyTypeConfig> enemyConfigs;
        public int totalEnemyCount;
        public float timeBetweenSpawns;
    }

    [Header("Wave Configuration")]
    public List<WaveData> waves; 
    public float timeBetweenWaves = 5f;
    
    [Header("Spawn Settings")]
    public float spawnRadius = 10f;
    public float minSpawnDistance = 3f; 
    public LayerMask obstacleLayer;      
    public GameObject spawnEffectPrefab; 
    public float effectDuration = 1.0f;  
    
    [Header("Live Tracking")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private int enemiesAlive;

    private Transform player;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        StartCoroutine(StartWaveSystem());
    }

    private IEnumerator StartWaveSystem()
    {
        while (currentWaveIndex < waves.Count)
        {
            foreach(var config in waves[currentWaveIndex].enemyConfigs) config.currentSpawnedCount = 0;

            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            
            while (enemiesAlive > 0) yield return new WaitForSeconds(0.5f);

            Debug.Log("Wave Clear! Starting Transition Sequence...");

            if (UpgradeManager.Instance != null)
            {
                // Call the new transition
                UpgradeManager.Instance.StartWaveEndTransition();

                // FIX: Wait a tiny bit for the panel to activate
                yield return new WaitForSecondsRealtime(0.1f);

                // Wait for the panel to close
                while (UpgradeManager.Instance.upgradePanel.activeSelf)
                {
                    yield return null; 
                }
                Debug.Log("Upgrade Selection Finished.");
            }

            yield return new WaitForSeconds(2.0f); 
            currentWaveIndex++;
            yield return new WaitForSeconds(timeBetweenWaves); 
        }
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        for (int i = 0; i < wave.totalEnemyCount; i++)
        {
            SpawnEnemy(wave);
            yield return new WaitForSeconds(wave.timeBetweenSpawns);
        }
    }

    private void SpawnEnemy(WaveData wave)
    {
        if (player == null) return;
        List<EnemyTypeConfig> validEnemies = new List<EnemyTypeConfig>();
        foreach (var config in wave.enemyConfigs)
            if (config.currentSpawnedCount < config.maxAmountInWave) validEnemies.Add(config);

        if (validEnemies.Count == 0) return;

        EnemyTypeConfig selected = validEnemies[Random.Range(0, validEnemies.Count)];
        selected.currentSpawnedCount++;

        Vector2 spawnPos = GetRandomSpawnPosition();
        StartCoroutine(ExecuteSpawn(spawnPos, selected.prefab));
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 finalPos = (Vector2)player.position + (Random.insideUnitCircle.normalized * spawnRadius);
        int attempts = 0;
        while (attempts < 10)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDist = Random.Range(minSpawnDistance, spawnRadius);
            Vector3 testPos = player.position + (Vector3)(randomDir * randomDist);
            if (!Physics2D.OverlapCircle(testPos, 0.4f, obstacleLayer)) return testPos;
            attempts++;
        }
        return finalPos;
    }

    private IEnumerator ExecuteSpawn(Vector2 position, GameObject enemyPrefab)
    {
        enemiesAlive++;
        if (spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, position, Quaternion.identity);
            Destroy(effect, effectDuration + 0.5f);
        }
        yield return new WaitForSeconds(effectDuration);
        Instantiate(enemyPrefab, position, Quaternion.identity);
    }

    public void OnEnemyKilled() => enemiesAlive--;

    private void OnDrawGizmos()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, spawnRadius);
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);
    }
}