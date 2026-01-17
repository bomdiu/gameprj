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
        public int maxAmountInWave; // NEW: Limit how many of THIS specific enemy can spawn
        [HideInInspector] public int currentSpawnedCount; // Tracks current wave total
    }

    [System.Serializable]
    public class WaveData
    {
        public string waveName;
        public List<EnemyTypeConfig> enemyConfigs; // NEW: Config for each enemy type
        public int totalEnemyCount;               // Total enemies for the whole wave
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
            // Reset per-enemy counters for the new wave
            foreach(var config in waves[currentWaveIndex].enemyConfigs) config.currentSpawnedCount = 0;

            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            
            while (enemiesAlive > 0) yield return new WaitForSeconds(0.5f);

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

        // Filter enemies that haven't hit their max count yet
        List<EnemyTypeConfig> validEnemies = new List<EnemyTypeConfig>();
        foreach (var config in wave.enemyConfigs)
        {
            if (config.currentSpawnedCount < config.maxAmountInWave)
                validEnemies.Add(config);
        }

        if (validEnemies.Count == 0) return;

        // Pick a valid enemy and increment its count
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

    // --- UPDATED GIZMOS ---
    private void OnDrawGizmos()
    {
        // Draw even when not selected so you can always see the spawn zone
        if (player == null) 
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        // Draw Spawn Radius (Cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, spawnRadius);

        // Draw Safe Zone (Red) - Enemies won't spawn here
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);
    }
}