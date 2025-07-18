using UnityEngine;
using System.Collections;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public Vector3 bossSpawnPosition = new Vector3(0, 10, 0); // Spawn off-screen above
    public float spawnDelay = 2f; // Delay before boss spawns after formation is cleared
    
    [Header("Formation Reference")]
    public ZigZagFormation1 formation; // Reference to the formation to monitor
    
    [Header("Boss Entry Settings")]
    public Vector3 bossEntryTarget = new Vector3(0, 3, 0); // Where boss stops after entering
    public float entryDuration = 3f; // How long it takes boss to enter
    
    private bool bossSpawned = false;
    private bool formationCleared = false;
    
    void Start()
    {
        // If no formation is assigned, try to find it automatically
        if (formation == null)
        {
            formation = FindObjectOfType<ZigZagFormation1>();
        }
        
        if (formation == null)
        {
            Debug.LogError("BossSpawner: No ZigZagFormation1 found! Please assign one manually.");
        }
    }
    
    void Update()
    {
        // Check if formation is cleared and boss hasn't been spawned yet
        if (!bossSpawned && !formationCleared && formation != null)
        {
            if (IsFormationCleared())
            {
                formationCleared = true;
                StartCoroutine(SpawnBossWithDelay());
            }
        }
    }
    
    bool IsFormationCleared()
    {
        // Check if all enemies in the formation are destroyed
        if (formation == null) return false;
        
        // Use the formation's built-in method to check if it's cleared
        bool isCleared = formation.IsFormationCleared();
        int remainingEnemies = formation.GetRemainingEnemyCount();
        
        Debug.Log($"BossSpawner: Active enemies remaining: {remainingEnemies}");
        return isCleared;
    }
    
    IEnumerator SpawnBossWithDelay()
    {
        Debug.Log("BossSpawner: Formation cleared! Boss will spawn in " + spawnDelay + " seconds.");
        
        // Wait for the specified delay
        yield return new WaitForSeconds(spawnDelay);
        
        // Spawn the boss
        SpawnBoss();
    }
    
    void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("BossSpawner: Boss prefab is not assigned!");
            return;
        }
        
        Debug.Log("BossSpawner: Spawning boss!");
        
        // Spawn boss at the specified position
        GameObject boss = Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
        
        // Get the BossEnemy component and set its entry target
        BossEnemy bossEnemy = boss.GetComponent<BossEnemy>();
        if (bossEnemy != null)
        {
            // Set the entry target position
            bossEnemy.entryTargetPosition = bossEntryTarget;
            bossEnemy.entryMoveDuration = entryDuration;
        }
        
        bossSpawned = true;
        
        // Optional: Disable the formation to prevent further spawning
        if (formation != null)
        {
            formation.enabled = false;
        }
        
        Debug.Log("BossSpawner: Boss spawned successfully!");
    }
    
    // Public method to manually trigger boss spawn (for testing)
    [ContextMenu("Spawn Boss Now")]
    public void SpawnBossNow()
    {
        if (!bossSpawned)
        {
            SpawnBoss();
        }
    }
    
    // Public method to reset the spawner (for multiple waves)
    public void ResetSpawner()
    {
        bossSpawned = false;
        formationCleared = false;
        
        if (formation != null)
        {
            formation.enabled = true;
        }
    }
} 