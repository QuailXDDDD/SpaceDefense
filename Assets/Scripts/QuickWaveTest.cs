using UnityEngine;

public class QuickWaveTest : MonoBehaviour
{
    [Header("Test Settings")]
    public GameObject enemyPrefab;
    public bool spawnTestEnemy = false;
    
    void Update()
    {
        if (spawnTestEnemy && enemyPrefab != null)
        {
            SpawnTestEnemy();
            spawnTestEnemy = false;
        }
        
        // Quick debug keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DebugActiveEnemies();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DebugWaveManager();
        }
    }
    
    void SpawnTestEnemy()
    {
        Vector3 spawnPos = new Vector3(0, 5, 0);
        GameObject testEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"Spawned test enemy: {testEnemy.name} at {spawnPos}");
        
        // Check if it has the required components
        EnemyBehaviour enemyBehaviour = testEnemy.GetComponent<EnemyBehaviour>();
        if (enemyBehaviour != null)
        {
            Debug.Log($"Test enemy has EnemyBehaviour. EnemyData: {(enemyBehaviour.enemyData != null ? enemyBehaviour.enemyData.name : "NULL")}");
        }
        else
        {
            Debug.Log("Test enemy has NO EnemyBehaviour component!");
        }
    }
    
    void DebugActiveEnemies()
    {
        EnemyBehaviour[] allEnemies = FindObjectsByType<EnemyBehaviour>(FindObjectsSortMode.None);
        Debug.Log($"=== ACTIVE ENEMIES DEBUG ===");
        Debug.Log($"Found {allEnemies.Length} EnemyBehaviour components in scene");
        
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i] != null)
            {
                Debug.Log($"Enemy {i + 1}: {allEnemies[i].name} - Position: {allEnemies[i].transform.position} - Parent: {(allEnemies[i].transform.parent != null ? allEnemies[i].transform.parent.name : "None")}");
            }
        }
    }
    
    void DebugWaveManager()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            Debug.Log($"=== WAVE MANAGER DEBUG ===");
            Debug.Log($"Current Wave: {waveManager.currentWave}");
            Debug.Log($"Wave In Progress: {waveManager.waveInProgress}");
            Debug.Log($"Active Enemies Count: {waveManager.activeEnemies.Count}");
            
            for (int i = 0; i < waveManager.activeEnemies.Count; i++)
            {
                if (waveManager.activeEnemies[i] != null)
                {
                    Debug.Log($"Active Enemy {i + 1}: {waveManager.activeEnemies[i].name}");
                }
                else
                {
                    Debug.Log($"Active Enemy {i + 1}: NULL (destroyed)");
                }
            }
        }
        else
        {
            Debug.LogError("No WaveManager found in scene!");
        }
    }
    
    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 200, 100), "Wave Test Debug");
        GUI.Label(new Rect(20, 30, 180, 20), "Press 1: Debug Enemies");
        GUI.Label(new Rect(20, 50, 180, 20), "Press 2: Debug WaveManager");
        GUI.Label(new Rect(20, 70, 180, 20), "Check Console for output");
    }
} 