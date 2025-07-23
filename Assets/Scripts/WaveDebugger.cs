using UnityEngine;

public class WaveDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugMode = true;
    public bool validateOnStart = true;
    
    [Header("Manual Testing")]
    [SerializeField] private bool testSpawnEnemy = false;
    public GameObject testEnemyPrefab;
    
    void Start()
    {
        if (validateOnStart)
        {
            ValidateWaveSystemSetup();
        }
    }
    
    void Update()
    {
        if (testSpawnEnemy && testEnemyPrefab != null)
        {
            TestSpawnEnemy();
            testSpawnEnemy = false;
        }
    }
    
    [ContextMenu("Validate Wave System Setup")]
    public void ValidateWaveSystemSetup()
    {
        Debug.Log("=== WAVE SYSTEM DEBUG VALIDATION ===");
        
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager == null)
        {
            Debug.LogError("❌ WaveManager not found in scene! Please create one.");
            return;
        }
        
        Debug.Log("✅ WaveManager found");
        
        ValidatePrefabAssignment("Wave 1 Enemy Prefab", waveManager.wave1EnemyPrefab);
        ValidatePrefabAssignment("Wave 2 Enemy Prefab", waveManager.wave2EnemyPrefab);
        ValidatePrefabAssignment("Wave 3 Enemy Prefab", waveManager.wave3EnemyPrefab);
        ValidatePrefabAssignment("Wave 4 Enemy Prefab", waveManager.wave4EnemyPrefab);
        ValidatePrefabAssignment("Boss Prefab", waveManager.bossPrefab);
        
        CheckForConflictingSpawners();
        
        Debug.Log($"Current Wave: {waveManager.currentWave}");
        Debug.Log($"Wave In Progress: {waveManager.waveInProgress}");
        Debug.Log($"Active Enemies: {waveManager.activeEnemies.Count}");
        
        Debug.Log("=== VALIDATION COMPLETE ===");
    }
    
    void ValidatePrefabAssignment(string prefabName, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError($"❌ {prefabName} is not assigned!");
        }
        else
        {
            Debug.Log($"✅ {prefabName} assigned: {prefab.name}");
            
            if (prefab.GetComponent<Enemy>() == null && prefab.GetComponent<EnemyBehaviour>() == null && prefab.GetComponent<BossEnemy>() == null)
            {
                Debug.LogWarning($"⚠️ {prefabName} doesn't have Enemy, EnemyBehaviour, or BossEnemy component!");
            }
            
            if (prefab.GetComponent<Collider2D>() == null)
            {
                Debug.LogWarning($"⚠️ {prefabName} doesn't have Collider2D component!");
            }
            
            if (prefab.GetComponent<SpriteRenderer>() == null)
            {
                Debug.LogWarning($"⚠️ {prefabName} doesn't have SpriteRenderer component!");
            }
        }
    }
    
    void CheckForConflictingSpawners()
    {
        ZigZagFormation1[] zigzagFormations = FindObjectsByType<ZigZagFormation1>(FindObjectsSortMode.None);
        if (zigzagFormations.Length > 0)
        {
            foreach (var formation in zigzagFormations)
            {
                if (formation.transform.parent == null)
                {
                    Debug.LogWarning($"⚠️ Found existing ZigZagFormation1 on '{formation.gameObject.name}' that might conflict with wave system. Consider disabling it.");
                }
            }
        }
        
        SkullFormation[] skullFormations = FindObjectsByType<SkullFormation>(FindObjectsSortMode.None);
        if (skullFormations.Length > 0)
        {
            foreach (var formation in skullFormations)
            {
                if (formation.transform.parent == null)
                {
                    Debug.LogWarning($"⚠️ Found existing SkullFormation on '{formation.gameObject.name}' that might conflict with wave system. Consider disabling it.");
                }
            }
        }
        
        MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        int bossSpawnerCount = 0;
        foreach (var behaviour in allBehaviours)
        {
            if (behaviour.GetType().Name == "BossSpawner")
            {
                bossSpawnerCount++;
            }
        }
        if (bossSpawnerCount > 0)
        {
            Debug.LogWarning($"⚠️ Found {bossSpawnerCount} BossSpawner(s) that might conflict with wave system. The wave system handles boss spawning automatically.");
        }
    }
    
    [ContextMenu("Test Spawn Enemy")]
    public void TestSpawnEnemy()
    {
        if (testEnemyPrefab == null)
        {
            Debug.LogError("Test Enemy Prefab is not assigned!");
            return;
        }
        
        Vector3 spawnPos = transform.position + Vector3.up * 2f;
        GameObject testEnemy = Instantiate(testEnemyPrefab, spawnPos, Quaternion.identity);
        
        if (testEnemy != null)
        {
            Debug.Log($"✅ Successfully spawned test enemy: {testEnemy.name}");
            
            Destroy(testEnemy, 5f);
        }
        else
        {
            Debug.LogError("❌ Failed to spawn test enemy!");
        }
    }
    
    [ContextMenu("Force Start Wave 1")]
    public void ForceStartWave1()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager == null)
        {
            Debug.LogError("WaveManager not found!");
            return;
        }
        
        Debug.Log("Forcing Wave 1 start...");
        waveManager.ResetWaves();
    }
    
    [ContextMenu("Get Current Wave Info")]
    public void GetCurrentWaveInfo()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager == null)
        {
            Debug.LogError("WaveManager not found!");
            return;
        }
        
        Debug.Log($"=== CURRENT WAVE INFO ===");
        Debug.Log($"Current Wave: {waveManager.currentWave}");
        Debug.Log($"Wave In Progress: {waveManager.waveInProgress}");
        Debug.Log($"Active Enemies Count: {waveManager.activeEnemies.Count}");
        Debug.Log($"Wave Info: {waveManager.GetWaveInfo()}");
        
        // List active enemies
        for (int i = 0; i < waveManager.activeEnemies.Count; i++)
        {
            if (waveManager.activeEnemies[i] != null)
            {
                Debug.Log($"Enemy {i + 1}: {waveManager.activeEnemies[i].name}");
            }
            else
            {
                Debug.Log($"Enemy {i + 1}: NULL (destroyed)");
            }
        }
    }
    
    [ContextMenu("Check Scene for Enemies")]
    public void CheckSceneForEnemies()
    {
        Debug.Log("=== CHECKING SCENE FOR ENEMIES ===");
        
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        EnemyBehaviour[] enemyBehaviours = FindObjectsByType<EnemyBehaviour>(FindObjectsSortMode.None);
        BossEnemy[] bosses = FindObjectsByType<BossEnemy>(FindObjectsSortMode.None);
        
        Debug.Log($"Found {enemies.Length} Enemy components in scene");
        Debug.Log($"Found {enemyBehaviours.Length} EnemyBehaviour components in scene");
        Debug.Log($"Found {bosses.Length} BossEnemy components in scene");
        
        StraightRowFormation[] straightFormations = FindObjectsByType<StraightRowFormation>(FindObjectsSortMode.None);
        ZigZagFormation1[] zigzagFormations = FindObjectsByType<ZigZagFormation1>(FindObjectsSortMode.None);
        CircleFormation[] circleFormations = FindObjectsByType<CircleFormation>(FindObjectsSortMode.None);
        
        Debug.Log($"Found {straightFormations.Length} StraightRowFormation components");
        Debug.Log($"Found {zigzagFormations.Length} ZigZagFormation1 components");
        Debug.Log($"Found {circleFormations.Length} CircleFormation components");
        
        if (straightFormations.Length > 0)
        {
            foreach (var formation in straightFormations)
            {
                Debug.Log($"StraightRowFormation '{formation.name}' has {formation.transform.childCount} children");
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!enableDebugMode) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(0, 10, 0), 1f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-2, 10, 0), new Vector3(2, 10, 0));
        
        if (Camera.main != null)
        {
            Camera mainCam = Camera.main;
            float height = mainCam.orthographicSize * 2f;
            float width = height * mainCam.aspect;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, height, 0));
        }
    }
} 