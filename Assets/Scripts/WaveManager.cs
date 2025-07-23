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
    
    [Header("Wave 2 - Grid Formation")]
    public GameObject wave2EnemyPrefab;
    public int wave2Rows = 2;
    public int wave2Cols = 5;
    public float wave2Spacing = 1.5f;
    public float wave2MoveSpeed = 2f;
    
    [Header("Wave 3 - ZigZag Formation")]
    public GameObject wave3EnemyPrefab;
    public int wave3Rows = 4;
    public int wave3Cols = 6;
    public float wave3Spacing = 1f;
    public float wave3MoveSpeed = 2f;
    
    [Header("Wave 4 - Circle Formation")]
    public GameObject wave4EnemyPrefab;
    public GameObject wave4SpecialEnemyPrefab;
    public GameObject bossPrefab;
    public int wave4EnemyCount = 5;
    public float wave4CircleRadius = 3f;
    public float wave4RotationSpeed = 30f;
    public float wave4MoveSpeed = 1f;
    
    [Header("Current Wave Info")]
    public int currentWave = 0;
    public bool waveInProgress = false;
    
    private GameObject currentFormation;
    public List<GameObject> activeEnemies = new List<GameObject>();
    
    public static System.Action<int> OnWaveStarted;
    public static System.Action<int> OnWaveCompleted;
    public static System.Action OnAllWavesCompleted;
    
    private bool playerReady = false;
    
    void Start()
    {
        PlayerShip.OnPlayerReady += OnPlayerReady;
        
        Debug.Log("WaveManager: Waiting for player ship to complete entrance sequence...");
    }
    
    void OnDestroy()
    {
        PlayerShip.OnPlayerReady -= OnPlayerReady;
    }
    
    void OnPlayerReady()
    {
        Debug.Log("WaveManager: Player ship is ready! Starting wave sequence...");
        playerReady = true;
        StartCoroutine(StartWaveSequence());
    }
    
    void Update()
    {
        if (waveInProgress && Time.time > waveStartTime + 1f && IsCurrentWaveCleared())
        {
            CompleteCurrentWave();
        }
    }
    
    private float waveStartTime;
    
    IEnumerator StartWaveSequence()
    {
        if (!playerReady)
        {
            Debug.LogWarning("WaveManager: Attempted to start waves before player is ready!");
            yield break;
        }
        
        yield return new WaitForSeconds(2f);
        
        for (int wave = 1; wave <= 4; wave++)
        {
            yield return StartCoroutine(SpawnWave(wave));
            
            while (waveInProgress)
            {
                yield return null;
            }
            
            if (wave < 4)
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
        
        EnsurePlayerCanShoot();
        
        Debug.Log($"Starting Wave {waveNumber}");
        OnWaveStarted?.Invoke(waveNumber);
        
        switch (waveNumber)
        {
            case 1:
                yield return StartCoroutine(SpawnWave1_StraightRow());
                break;
            case 2:
                yield return StartCoroutine(SpawnWave2_Grid());
                break;
            case 3:
                yield return StartCoroutine(SpawnWave3_ZigZag());
                break;
            case 4:
                yield return StartCoroutine(SpawnWave4_Circle());
                break;
        }
    }
    
    IEnumerator SpawnWave1_StraightRow()
    {
        Debug.Log("WaveManager: Creating Wave 1 - Straight Row Formation");
        
        if (wave1EnemyPrefab == null)
        {
            Debug.LogError("WaveManager: Wave 1 Enemy Prefab is not assigned!");
            yield break;
        }
        
        currentFormation = new GameObject("Wave1_StraightRow");
        currentFormation.transform.position = offScreenSpawnPosition;
        
        Collider2D formationCollider = currentFormation.GetComponent<Collider2D>();
        if (formationCollider != null)
        {
            Destroy(formationCollider);
        }
        
        StraightRowFormation formation = currentFormation.AddComponent<StraightRowFormation>();
        formation.enemyPrefab = wave1EnemyPrefab;
        formation.enemyCount = wave1EnemyCount;
        formation.spacing = wave1EnemySpacing;
        formation.moveSpeed = wave1MoveSpeed;
        
        Debug.Log($"WaveManager: Formation component added, waiting for enemies to spawn...");
        
        yield return new WaitForSeconds(0.1f);
        
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
        
        for (int i = 0; i < currentFormation.transform.childCount; i++)
        {
            activeEnemies.Add(currentFormation.transform.GetChild(i).gameObject);
        }
        
        Debug.Log($"WaveManager: Wave 1 spawned {activeEnemies.Count} enemies successfully");
    }
    
    IEnumerator SpawnWave2_Grid()
    {
        Debug.Log("WaveManager: Creating Wave 2 - Grid Formation");
        
        if (wave2EnemyPrefab == null)
        {
            Debug.LogError("WaveManager: Wave 2 Enemy Prefab is not assigned!");
            yield break;
        }
        
        currentFormation = new GameObject("Wave2_Grid");
        currentFormation.transform.position = offScreenSpawnPosition;
        
        Collider2D formationCollider = currentFormation.GetComponent<Collider2D>();
        if (formationCollider != null)
        {
            Destroy(formationCollider);
        }
        
        GridFormation formation = currentFormation.AddComponent<GridFormation>();
        formation.enemyPrefab = wave2EnemyPrefab;
        formation.rows = wave2Rows;
        formation.cols = wave2Cols;
        formation.spacing = wave2Spacing;
        formation.moveSpeed = wave2MoveSpeed;
        formation.stayInPosition = true;
        formation.spawnFromOffScreen = true;
        
        Debug.Log($"WaveManager: Grid formation component added, waiting for enemies to spawn...");
        
        yield return new WaitForSeconds(0.1f);
        
        float timeout = 5f;
        float elapsed = 0f;
        int expectedCount = wave2Rows * wave2Cols;
        while (currentFormation.transform.childCount < expectedCount && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (currentFormation.transform.childCount == 0)
        {
            Debug.LogError("WaveManager: No enemies spawned for Wave 2! Check enemy prefab assignment and formation script.");
            yield break;
        }
        
        for (int i = 0; i < currentFormation.transform.childCount; i++)
        {
            activeEnemies.Add(currentFormation.transform.GetChild(i).gameObject);
        }
        
        Debug.Log($"WaveManager: Wave 2 spawned {activeEnemies.Count} enemies successfully");
    }
    
    IEnumerator SpawnWave3_ZigZag()
    {
        Debug.Log("WaveManager: Creating Wave 3 - ZigZag Formation");
        
        if (wave3EnemyPrefab == null)
        {
            Debug.LogError("WaveManager: Wave 3 Enemy Prefab is not assigned!");
            yield break;
        }
        
        currentFormation = new GameObject("Wave3_ZigZag");
        currentFormation.transform.position = offScreenSpawnPosition;
        
        Collider2D formationCollider = currentFormation.GetComponent<Collider2D>();
        if (formationCollider != null)
        {
            Destroy(formationCollider);
        }
        
        ZigZagFormation1 formation = currentFormation.AddComponent<ZigZagFormation1>();
        formation.enemyPrefab = wave3EnemyPrefab;
        formation.rows = wave3Rows;
        formation.cols = wave3Cols;
        formation.spacing = wave3Spacing;
        formation.moveSpeed = wave3MoveSpeed;
        formation.stayInPosition = true;
        formation.spawnFromOffScreen = true;
        
        Debug.Log($"WaveManager: ZigZag formation component added, waiting for enemies to spawn...");
        
        yield return new WaitForSeconds(0.1f);
        
        float timeout = 5f;
        float elapsed = 0f;
        while (currentFormation.transform.childCount == 0 && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (currentFormation.transform.childCount == 0)
        {
            Debug.LogError("WaveManager: No enemies spawned for Wave 3! Check enemy prefab assignment and formation script.");
            yield break;
        }
        
        for (int i = 0; i < currentFormation.transform.childCount; i++)
        {
            activeEnemies.Add(currentFormation.transform.GetChild(i).gameObject);
        }
        
        Debug.Log($"WaveManager: Wave 3 spawned {activeEnemies.Count} enemies successfully");
    }
    
    IEnumerator SpawnWave4_Circle()
    {
        Debug.Log("WaveManager: Creating Wave 4 - Circle Formation with Boss");
        
        if (wave4EnemyPrefab == null)
        {
            Debug.LogError("WaveManager: Wave 4 Enemy Prefab is not assigned!");
            yield break;
        }
        
        if (bossPrefab == null)
        {
            Debug.LogError("WaveManager: Boss Prefab is not assigned!");
            yield break;
        }
        
        currentFormation = new GameObject("Wave4_Circle");
        currentFormation.transform.position = offScreenSpawnPosition;
        
        Collider2D formationCollider = currentFormation.GetComponent<Collider2D>();
        if (formationCollider != null)
        {
            Destroy(formationCollider);
        }
        
        CircleFormation formation = currentFormation.AddComponent<CircleFormation>();
        formation.enemyPrefab = wave4EnemyPrefab;
        formation.specialEnemyPrefab = wave4SpecialEnemyPrefab;
        formation.bossPrefab = bossPrefab;
        formation.enemyCount = wave4EnemyCount;
        formation.circleRadius = wave4CircleRadius;
        formation.rotationSpeed = wave4RotationSpeed;
        formation.moveSpeed = wave4MoveSpeed;
        formation.stayInPosition = true;
        formation.spawnFromOffScreen = true;
        
        Debug.Log($"WaveManager: Circle formation component added, waiting for enemies and boss to spawn...");
        
        yield return new WaitForSeconds(0.1f);
        
        float timeout = 5f;
        float elapsed = 0f;
        int expectedCount = wave4EnemyCount + 1;
        while (currentFormation.transform.childCount < expectedCount && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (currentFormation.transform.childCount == 0)
        {
            Debug.LogError("WaveManager: No enemies or boss spawned for Wave 4! Check prefab assignments and formation script.");
            yield break;
        }
        
        for (int i = 0; i < currentFormation.transform.childCount; i++)
        {
            activeEnemies.Add(currentFormation.transform.GetChild(i).gameObject);
        }
        
        Debug.Log($"WaveManager: Wave 4 spawned {activeEnemies.Count} enemies and boss successfully");
    }
    
    bool IsCurrentWaveCleared()
    {
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
        
        if (currentFormation != null)
        {
            Destroy(currentFormation);
        }
    }
    
    public void StartNextWave()
    {
        if (!waveInProgress && currentWave < 4)
        {
            StartCoroutine(SpawnWave(currentWave + 1));
        }
    }
    
    public void ResetWaves()
    {
        StopAllCoroutines();
        
        if (currentFormation != null)
        {
            Destroy(currentFormation);
        }
        
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
        
        StartCoroutine(StartWaveSequence());
    }
    
    public string GetWaveInfo()
    {
        if (waveInProgress)
        {
            return $"Wave {currentWave} - Enemies Remaining: {activeEnemies.Count}";
        }
        else if (currentWave >= 4)
        {
            return "All waves completed!";
        }
        else
        {
            return "Preparing next wave...";
        }
    }
    
    void EnsurePlayerCanShoot()
    {
        PlayerShip player = FindFirstObjectByType<PlayerShip>();
        if (player != null)
        {
            player.enabled = true;
            Debug.Log("WaveManager: Ensured player can shoot during wave transition");
        }
        else
        {
            Debug.LogWarning("WaveManager: Player not found when trying to ensure shooting ability");
        }
    }
} 