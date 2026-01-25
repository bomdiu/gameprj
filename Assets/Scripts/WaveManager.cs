using UnityEngine;
using UnityEngine.SceneManagement;
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
    
    [Header("Portal Settings")]
    public GameObject portalPrefab; 
    public Transform portalSpawnPoint; 
    public float portalFadeDuration = 2.0f; 
    public float portalSpawnDelay = 3f;

    [Header("Suction Settings")]
    public float suctionRadius = 12f;       
    public float baseSuctionStrength = 5f;  
    public float maxSuctionStrength = 25f;  
    public float suctionAcceleration = 2.0f; 
    public float absorptionDistance = 0.5f; 
    public ParticleSystem fireflySystem; // Drag your particle system here!

    [Header("Transition Settings")]
    public string nextSceneName; 
    public float delayBeforeLoad = 1.0f; 

    [Header("Spawn Settings")]
    public float spawnRadius = 15f; 
    public float minSpawnDistance = 5f; 
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
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (waves.Count > 0) StartCoroutine(StartWaveSystem());
    }

    private IEnumerator StartWaveSystem()
    {
        // Initial delay before first wave
        yield return new WaitForSeconds(3.0f);

        while (currentWaveIndex < waves.Count)
        {
            foreach(var config in waves[currentWaveIndex].enemyConfigs) config.currentSpawnedCount = 0;

            // 1. SPAWN THE WAVE
            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));

            // 2. WAIT BUFFER (Prevents skipping to upgrade before enemies exist)
            yield return new WaitForSeconds(1.0f);

            // 3. THE WAIT GATE: Strictly wait until all enemies are gone
            while (enemiesAlive > 0 || GameObject.FindGameObjectWithTag("Enemy") != null) 
            {
                // Safety: If scene is empty but counter is stuck, force zero
                if (enemiesAlive > 0 && GameObject.FindGameObjectWithTag("Enemy") == null)
                {
                    enemiesAlive = 0;
                }
                yield return new WaitForSeconds(0.2f);
            }

            // 4. WAVE IS DEFINITIVELY CLEARED - Now give the upgrade
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.StartWaveEndTransition();
                yield return new WaitForSecondsRealtime(0.5f); 

                while (UpgradeManager.Instance.upgradePanel.activeSelf) 
                {
                    yield return null; 
                }
            }

            // 5. CHECK FOR MAP END
            if (currentWaveIndex == waves.Count - 1)
            {
                if (SkillUnlockManager.Instance != null)
                {
                    SkillUnlockManager.Instance.CheckLevelCompletion();
                }

                SpawnPortal(); 
                yield break; 
            }

            // 6. PRE-WAVE DELAY
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
        List<EnemyTypeConfig> validEnemies = new List<EnemyTypeConfig>();
        foreach (var config in wave.enemyConfigs)
            if (config.currentSpawnedCount < config.maxAmountInWave) validEnemies.Add(config);
        
        if (validEnemies.Count == 0) return;
        
        EnemyTypeConfig selected = validEnemies[Random.Range(0, validEnemies.Count)];
        selected.currentSpawnedCount++;
        Vector2 spawnPos = GetRandomSpawnPosition();
        StartCoroutine(ExecuteSpawn(spawnPos, selected.prefab));
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

    private void SpawnPortal()
    {
        if (portalPrefab == null) return;
        Vector3 spawnPos = GetPortalSpawnPosition();
        GameObject portal = Instantiate(portalPrefab, spawnPos, Quaternion.identity);
        StartCoroutine(AnimatePortalEntrance(portal)); 
    }

    private IEnumerator AnimatePortalEntrance(GameObject portal)
    {
        SpriteRenderer sr = portal.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) yield break;

        Vector3 targetScale = portal.transform.localScale;
        portal.transform.localScale = Vector3.zero;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);

        yield return new WaitForSeconds(portalSpawnDelay);

        float timer = 0;
        while (timer < portalFadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / portalFadeDuration;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, Mathf.Lerp(0, 1, progress));
            portal.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
            yield return null;
        }

        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1);
        portal.transform.localScale = targetScale;

        StartCoroutine(PortalSuctionLogic(portal.transform.position));
    }

    private IEnumerator PortalSuctionLogic(Vector3 portalPos)
    {
        bool playerAbsorbed = false;
        float currentStrength = baseSuctionStrength;

        if (player != null)
        {
            Collider2D col = player.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.isKinematic = true;
        }

        while (!playerAbsorbed)
        {
            if (player != null)
            {
                float distToPlayer = Vector3.Distance(player.position, portalPos);
                if (distToPlayer < suctionRadius)
                {
                    currentStrength = Mathf.MoveTowards(currentStrength, maxSuctionStrength, suctionAcceleration * Time.deltaTime);
                    player.position = Vector3.MoveTowards(player.position, portalPos, currentStrength * Time.deltaTime);
                    
                    if (distToPlayer < absorptionDistance)
                    {
                        playerAbsorbed = true;
                        player.gameObject.SetActive(false); 
                    }
                }
            }

            // PARTICLE SUCTION LOGIC
            if (fireflySystem != null)
            {
                var vel = fireflySystem.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.World;
                float particlePull = maxSuctionStrength * 1.5f; 
                fireflySystem.transform.position = Vector3.MoveTowards(fireflySystem.transform.position, portalPos, particlePull * Time.deltaTime);
                vel.radial = -particlePull; 
            }

            yield return null;
        }

        yield return new WaitForSeconds(delayBeforeLoad);

        if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.ToBlack(nextSceneName);
        else SceneManager.LoadScene(nextSceneName);
    }

    private Vector3 GetPortalSpawnPosition()
    {
        if (portalSpawnPoint != null) return portalSpawnPoint.position;
        if (player != null)
        {
            float checkRadius = 3f;
            for (int i = 0; i < 20; i++)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                Vector3 testPos = player.position + (Vector3)(randomDir * checkRadius);
                if (!Physics2D.OverlapCircle(testPos, 0.6f, obstacleLayer)) return testPos;
                checkRadius += 0.2f;
            }
        }
        return Vector3.zero;
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Vector3 mapCenter = Vector3.zero; 
        for (int i = 0; i < 15; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDist = Random.Range(minSpawnDistance, spawnRadius);
            Vector3 testPos = mapCenter + (Vector3)(randomDir * randomDist);
            if (!Physics2D.OverlapCircle(testPos, 0.4f, obstacleLayer)) return testPos;
        }
        return (Vector2)Random.insideUnitCircle.normalized * spawnRadius;
    }
}