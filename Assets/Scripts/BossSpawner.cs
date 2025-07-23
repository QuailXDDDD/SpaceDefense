using UnityEngine;
using System.Collections;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public Vector3 bossSpawnPosition = new Vector3(0, 10, 0);
    public float spawnDelay = 2f;
    
    [Header("Formation Reference")]
    public ZigZagFormation1 formation;
    
    [Header("Boss Entry Settings")]
    public Vector3 bossEntryTarget = new Vector3(0, 3, 0);
    public float entryDuration = 3f;
    
    private bool bossSpawned = false;
    private bool formationCleared = false;
    
    void Start()
    {
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
        if (formation == null) return false;
        
        bool isCleared = formation.IsFormationCleared();
        int remainingEnemies = formation.GetRemainingEnemyCount();
        
        Debug.Log($"BossSpawner: Active enemies remaining: {remainingEnemies}");
        return isCleared;
    }
    
    IEnumerator SpawnBossWithDelay()
    {
        Debug.Log("BossSpawner: Formation cleared! Boss will spawn in " + spawnDelay + " seconds.");
        
        yield return new WaitForSeconds(spawnDelay);
        
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
        
        GameObject boss = Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
        
        BossEnemy bossEnemy = boss.GetComponent<BossEnemy>();
        if (bossEnemy != null)
        {
            bossEnemy.entryTargetPosition = bossEntryTarget;
            bossEnemy.entryMoveDuration = entryDuration;
        }
        
        bossSpawned = true;
        
        if (formation != null)
        {
            formation.enabled = false;
        }
        
        Debug.Log("BossSpawner: Boss spawned successfully!");
    }
    
    [ContextMenu("Spawn Boss Now")]
    public void SpawnBossNow()
    {
        if (!bossSpawned)
        {
            SpawnBoss();
        }
    }
    
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