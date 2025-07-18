using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public float delayBetweenWaves = 3f;
    public Vector3 offScreenSpawnPosition = new Vector3(0, 10, 0);
    
    [Header("Wave 1 - Straight Row")]
    public GameObject wave1EnemyPrefab;
    public int wave1EnemyCount = 3;
    public float wave1EnemySpacing = 2f;
    public float wave1MoveSpeed = 2f;
    
    [Header("Wave 2 - ZigZag Formation")]
    public GameObject wave2EnemyPrefab;
    public int wave2Rows = 4;
    public int wave2Cols = 6;
    public float wave2Spacing = 1f;
    public float wave2MoveSpeed = 2f;
    
    [Header("Wave 3 - Circle Formation")]
    public GameObject wave3EnemyPrefab;
    public GameObject bossPrefab;
    public int wave3EnemyCount = 5;
    public float wave3CircleRadius = 3f;
    public float wave3RotationSpeed = 30f; // degrees per second
    public float wave3MoveSpeed = 1f;
    
    [Header("Current Wave Info")]
    public int currentWave = 0;
    public bool waveInProgress = false;
    
    // References to active formations
    private GameObject currentFormation;
    public List<GameObject> activeEnemies = new List<GameObject>(); // Public for UI access
    
    // Public events for other systems to listen to
    public static System.Action<int> OnWaveStarted;
    public static System.Action<int> OnWaveCompleted;
    public static System.Action OnAllWavesCompleted;
    
    void Start()
    {
        StartCoroutine(StartWaveSequence());
    }
    
    void Update()
    {
        // Check if current wave is completed (but wait a bit for enemies to properly spawn)
        if (waveInProgress && Time.time > waveStartTime + 1f && IsCurrentWaveCleared())
        {
            CompleteCurrentWave();
        }
    }
    
    private float waveStartTime;
    
    IEnumerator StartWaveSequence()
    {
        yield return new WaitForSeconds(1f); // Initial delay
        
        for (int wave = 1; wave <= 3; wave++)
        {
            yield return StartCoroutine(SpawnWave(wave));
            
            // Wait for wave to be cleared
            while (waveInProgress)
            {
                yield return null;
            }
            
            // Delay between waves (except after last wave)
            if (wave < 3)
            {
                yield return new WaitForSeconds(delayBetweenWaves);
            }
        }
        
        Debug.Log("All waves completed!");
        OnAllWavesCompleted?.Invoke();
    }
    
    IEnumerator SpawnWave(int waveNumber)
    {
        currentWave = waveNumber;
        waveInProgress = true;
        waveStartTime = Time.time;
        activeEnemies.Clear();
        
        // Ensure player can shoot during wave transitions
        EnsurePlayerCanShoot();
        
        Debug.Log($"Starting Wave {waveNumber}");
        OnWaveStarted?.Invoke(waveNumber);
        
        switch (waveNumber)
        {
            case 1:
                yield return StartCoroutine(SpawnWave1_StraightRow());
                break;
            case 2:
                yield return StartCoroutine(SpawnWave2_ZigZag());
                break;
            case 3:
                yield return StartCoroutine(SpawnWave3_Circle());
                break;
        }
    }
    
    IEnumerator SpawnWave1_StraightRow()
    {
        Debug.Log("WaveManager: Creating Wave 1 - Straight Row Formation");
        
        // Check if prefab is assigned
        if (wave1EnemyPrefab == null)
        {
            Debug.LogError("WaveManager: Wave 1 Enemy Prefab is not assigned!");
            yield break;
        }
        
        // Create formation container
        currentFormation = new GameObject("Wave1_StraightRow");
        currentFormation.transform.position = offScreenSpawnPosition;
        
        // Ensure formation container doesn't interfere with player bullets
        Collider2D formationCollider = currentFormation.GetComponent<Collider2D>();
        if (formationCollider != null)
        {
            Destroy(formationCollider);
        }
        
        // Add StraightRowFormation component
        StraightRowFormation formation = currentFormation.AddComponent<StraightRowFormation>();
        formation.enemyPrefab = wave1EnemyPrefab;
        formation.enemyCount = wave1EnemyCount;
        formation.spacing = wave1EnemySpacing;
        formation.moveSpeed = wave1MoveSpeed;
        
        Debug.Log($"WaveManager: Formation component added, waiting for enemies to spawn...");
        
        // Wait longer for the formation to initialize and spawn enemies
        yield return new WaitForSeconds(0.1f);
        
        // Wait until enemies are actually spawned
        float timeout = 5f;
        float elapsed = 0f;
        while (currentFormation.transform.childCount == 0 && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (currentFormation.transform.childCount == 0)
        {
            Debug.LogError("WaveManager: No enemies spawned after timeout! Check enemy prefab assignment and formation script.");
            yield break;
        }
        
        // Get references to all spawned enemies
        for (int i = 0; i < currentFormation.transform.childCount; i++)
        {
            activeEnemies.Add(currentFormation.transform.GetChild(i).gameObject);
        }
        
        Debug.Log($"WaveManager: Wave 1 spawned {activeEnemies.Count} enemies successfully");
    }
    
    IEnumerator SpawnWave2_ZigZag()
    {
        Debug.Log("WaveManager: Creating Wave 2 - ZigZag Formation");
        
        // Check if prefab is assigned
        if (wave2EnemyPrefab == null)
        {
            Debug.LogError("WaveManager: Wave 2 Enemy Prefab is not assigned!");
            yield break;
        }
        
        // Create formation container
        currentFormation = new GameObject("Wave2_ZigZag");
        currentFormation.transform.position = offScreenSpawnPosition;
        
        // Ensure formation container doesn't interfere with player bullets
        Collider2D formationCollider = currentFormation.GetComponent<Collider2D>();
        if (formationCollider != null)
        {
            Destroy(formationCollider);
        }
        
        // Add ZigZagFormation component
        ZigZagFormation1 formation = currentFormation.AddComponent<ZigZagFormation1>();
        formation.enemyPrefab = wave2EnemyPrefab;
        formation.rows = wave2Rows;
        formation.cols = wave2Cols;
        formation.spacing = wave2Spacing;
        formation.moveSpeed = wave2MoveSpeed;
        formation.stayInPosition = true; // Stay at target position for zigzag movement
        formation.spawnFromOffScreen = true; // Enable off-screen spawning
        
        Debug.Log($"WaveManager: ZigZag formation component added, waiting for enemies to spawn...");
        
        // Wait longer for the formation to initialize and spawn enemies
        yield return new WaitForSeconds(0.1f);
        
        // Wait until enemies are actually spawned
        float timeout = 5f;
        float elapsed = 0f;
        while (currentFormation.transform.childCount == 0 && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (currentFormation.transform.childCount == 0)
        {
            Debug.LogError("WaveManager: No enemies spawned for Wave 2! Check enemy prefab assignment and formation script.");
            yield break;
        }
        
        // Get references to all spawned enemies
        for (int i = 0; i < currentFormation.transform.childCount; i++)
        {
            activeEnemies.Add(currentFormation.transform.GetChild(i).gameObject);
        }
        
        Debug.Log($"WaveManager: Wave 2 spawned {activeEnemies.Count} enemies successfully");
    }
    
    IEnumerator SpawnWave3_Circle()
    {
        Debug.Log("WaveManager: Creating Wave 3 - Circle Formation with Boss");
        
        // Check if prefabs are assigned
        if (wave3EnemyPrefab == null)
        {
            Debug.LogError("WaveManager: Wave 3 Enemy Prefab is not assigned!");
            yield break;
        }
        
        if (bossPrefab == null)
        {
            Debug.LogError("WaveManager: Boss Prefab is not assigned!");
            yield break;
        }
        
        // Create formation container
        currentFormation = new GameObject("Wave3_Circle");
        currentFormation.transform.position = offScreenSpawnPosition;
        
        // Ensure formation container doesn't interfere with player bullets
        Collider2D formationCollider = currentFormation.GetComponent<Collider2D>();
        if (formationCollider != null)
        {
            Destroy(formationCollider);
        }
        
        // Add CircleFormation component
        CircleFormation formation = currentFormation.AddComponent<CircleFormation>();
        formation.enemyPrefab = wave3EnemyPrefab;
        formation.bossPrefab = bossPrefab;
        formation.enemyCount = wave3EnemyCount;
        formation.circleRadius = wave3CircleRadius;
        formation.rotationSpeed = wave3RotationSpeed;
        formation.moveSpeed = wave3MoveSpeed;
        formation.stayInPosition = true; // Stay at target position for circular rotation
        formation.spawnFromOffScreen = true; // Enable off-screen spawning
        
        Debug.Log($"WaveManager: Circle formation component added, waiting for enemies and boss to spawn...");
        
        // Wait longer for the formation to initialize and spawn enemies
        yield return new WaitForSeconds(0.1f);
        
        // Wait until enemies are actually spawned (should be enemies + boss)
        float timeout = 5f;
        float elapsed = 0f;
        int expectedCount = wave3EnemyCount + 1; // enemies + boss
        while (currentFormation.transform.childCount < expectedCount && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (currentFormation.transform.childCount == 0)
        {
            Debug.LogError("WaveManager: No enemies or boss spawned for Wave 3! Check prefab assignments and formation script.");
            yield break;
        }
        
        // Get references to all spawned enemies (including boss)
        for (int i = 0; i < currentFormation.transform.childCount; i++)
        {
            activeEnemies.Add(currentFormation.transform.GetChild(i).gameObject);
        }
        
        Debug.Log($"WaveManager: Wave 3 spawned {activeEnemies.Count} enemies and boss successfully");
    }
    
    bool IsCurrentWaveCleared()
    {
        // Remove null references (destroyed enemies)
        int beforeCount = activeEnemies.Count;
        activeEnemies.RemoveAll(enemy => enemy == null);
        int afterCount = activeEnemies.Count;
        
        if (beforeCount != afterCount)
        {
            Debug.Log($"WaveManager: Cleaned up {beforeCount - afterCount} destroyed enemies. {afterCount} enemies remaining.");
        }
        
        return activeEnemies.Count == 0;
    }
    
    void CompleteCurrentWave()
    {
        waveInProgress = false;
        Debug.Log($"Wave {currentWave} completed!");
        OnWaveCompleted?.Invoke(currentWave);
        
        // Clean up formation object
        if (currentFormation != null)
        {
            Destroy(currentFormation);
        }
    }
    
    // Public methods for external control
    public void StartNextWave()
    {
        if (!waveInProgress && currentWave < 3)
        {
            StartCoroutine(SpawnWave(currentWave + 1));
        }
    }
    
    public void ResetWaves()
    {
        StopAllCoroutines();
        
        // Clean up current formation
        if (currentFormation != null)
        {
            Destroy(currentFormation);
        }
        
        // Clear active enemies
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        
        currentWave = 0;
        waveInProgress = false;
        
        // Restart wave sequence
        StartCoroutine(StartWaveSequence());
    }
    
    // Utility method to get current wave info
    public string GetWaveInfo()
    {
        if (waveInProgress)
        {
            return $"Wave {currentWave} - Enemies Remaining: {activeEnemies.Count}";
        }
        else if (currentWave >= 3)
        {
            return "All waves completed!";
        }
        else
        {
            return "Preparing next wave...";
        }
    }
    
    // Ensure player can shoot during wave transitions
    void EnsurePlayerCanShoot()
    {
        PlayerShip player = FindFirstObjectByType<PlayerShip>();
        if (player != null)
        {
            // Make sure player is not disabled or blocked
            player.enabled = true;
            Debug.Log("WaveManager: Ensured player can shoot during wave transition");
        }
        else
        {
            Debug.LogWarning("WaveManager: Player not found when trying to ensure shooting ability");
        }
    }
} 