using UnityEngine;
using System.Collections.Generic;

public class StraightRowFormation : MonoBehaviour
{
    [Header("Formation Settings")]
    public GameObject enemyPrefab;
    public int enemyCount = 3;
    public float spacing = 2f;
    public Vector3 enemyScale = new Vector3(0.5f, 0.5f, 1f);
    
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public Vector3 moveDirection = Vector3.down;
    
    [Header("Spawn Settings")]
    public bool spawnFromOffScreen = true;
    public float entryDuration = 2f;
    
    private List<GameObject> enemies = new List<GameObject>();
    private Vector3 targetPosition;
    private bool hasEnteredScreen = false;
    
    void Start()
    {
        if (spawnFromOffScreen)
        {
            targetPosition = transform.position;
            float screenTop = Camera.main.orthographicSize;
            transform.position = new Vector3(targetPosition.x, screenTop + 3f, targetPosition.z);
        }
        
        CreateStraightRowFormation();
        
        if (spawnFromOffScreen)
        {
            StartCoroutine(EntryMovement());
        }
        else
        {
            hasEnteredScreen = true;
        }
    }
    
    void Update()
    {
        if (hasEnteredScreen)
        {
            MoveFormation();
        }
        
        CleanupDestroyedEnemies();
        
        if (Time.time % 2f < Time.deltaTime)
        {
            Debug.Log($"StraightRowFormation: {enemies.Count} enemies alive, hasEnteredScreen: {hasEnteredScreen}");
        }
    }
    
    void CreateStraightRowFormation()
    {
        Debug.Log($"StraightRowFormation: Starting to create formation with {enemyCount} enemies");
        
        if (enemyPrefab == null)
        {
            Debug.LogError("StraightRowFormation: Enemy prefab is not assigned!");
            return;
        }
        
        float totalWidth = (enemyCount - 1) * spacing;
        Vector3 startPosition = new Vector3(-totalWidth / 2f, 0, 0);
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 enemyPosition = startPosition + new Vector3(i * spacing, 0, 0);
            Vector3 worldPosition = transform.position + enemyPosition;
            
            Debug.Log($"StraightRowFormation: Spawning enemy {i + 1} at position {worldPosition}");
            
            GameObject enemy = Instantiate(enemyPrefab, worldPosition, Quaternion.identity, transform);
            
            if (enemy == null)
            {
                Debug.LogError($"StraightRowFormation: Failed to instantiate enemy {i + 1}!");
                continue;
            }
            
            if (enemy.transform.localScale != enemyScale)
            {
                enemy.transform.localScale = enemyScale;
            }
            
            EnemyBehaviour enemyBehaviour = enemy.GetComponent<EnemyBehaviour>();
            if (enemyBehaviour != null && enemyBehaviour.enemyData == null)
            {
                EnemyBehaviour prefabEnemyBehaviour = enemyPrefab.GetComponent<EnemyBehaviour>();
                if (prefabEnemyBehaviour != null && prefabEnemyBehaviour.enemyData != null)
                {
                    enemyBehaviour.enemyData = prefabEnemyBehaviour.enemyData;
                    Debug.Log($"StraightRowFormation: Assigned EnemyData to enemy {i + 1}");
                }
                else
                {
                    Debug.LogWarning($"StraightRowFormation: Enemy {i + 1} missing EnemyData! This will cause it to be destroyed.");
                }
            }
            
            EnemyMover mover = enemy.GetComponent<EnemyMover>();
            if (mover == null)
            {
                mover = enemy.AddComponent<EnemyMover>();
            }
            
            mover.moveSpeed = 0f;
            mover.moveDirection = Vector3.down;
            mover.enabled = false;
            
            enemies.Add(enemy);
            Debug.Log($"StraightRowFormation: Enemy {i + 1} created successfully");
        }
        
        Debug.Log($"StraightRowFormation: Created {enemies.Count} enemies in straight row. Transform has {transform.childCount} children.");
    }
    
    System.Collections.IEnumerator EntryMovement()
    {
        Debug.Log("StraightRowFormation: Entering screen...");
        Vector3 startPos = transform.position;
        float timer = 0f;
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                EnemyBehaviour enemyBehaviour = enemy.GetComponent<EnemyBehaviour>();
                if (enemyBehaviour != null)
                {
                    enemyBehaviour.enabled = false;
                }
            }
        }
        
        while (timer < entryDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPosition, timer / entryDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
        hasEnteredScreen = true;
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                EnemyBehaviour enemyBehaviour = enemy.GetComponent<EnemyBehaviour>();
                if (enemyBehaviour != null)
                {
                    enemyBehaviour.enabled = true;
                }
                
                EnemyMover mover = enemy.GetComponent<EnemyMover>();
                if (mover != null)
                {
                    mover.moveSpeed = moveSpeed;
                    mover.enabled = true;
                }
            }
        }
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                EnemyBehaviour enemyBehaviour = enemy.GetComponent<EnemyBehaviour>();
                if (enemyBehaviour != null)
                {
                    enemyBehaviour.enabled = true;
                    enemyBehaviour.EnableImmediateShooting();
                }
                
                BasicAttacker basicAttacker = enemy.GetComponent<BasicAttacker>();
                if (basicAttacker != null)
                {
                    basicAttacker.EnableImmediateShooting();
                }
                
                SpreadAttacker spreadAttacker = enemy.GetComponent<SpreadAttacker>();
                if (spreadAttacker != null)
                {
                    spreadAttacker.EnableImmediateShooting();
                }
                
                BurstAttacker burstAttacker = enemy.GetComponent<BurstAttacker>();
                if (burstAttacker != null)
                {
                    burstAttacker.EnableImmediateShooting();
                }
            }
        }
        
        Debug.Log($"StraightRowFormation: Entered screen, {enemies.Count} enemies moving straight down");
    }
    
    void MoveFormation()
    {
        bool anyEnemyHasIndividualMovement = false;
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                EnemyMover mover = enemy.GetComponent<EnemyMover>();
                if (mover != null && mover.enabled)
                {
                    anyEnemyHasIndividualMovement = true;
                    break;
                }
            }
        }
        
        if (!anyEnemyHasIndividualMovement)
        {
            transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
    }
    
    void CleanupDestroyedEnemies()
    {
        enemies.RemoveAll(enemy => enemy == null);
    }
    
    public bool IsFormationCleared()
    {
        CleanupDestroyedEnemies();
        return enemies.Count == 0;
    }
    
    public int GetRemainingEnemyCount()
    {
        CleanupDestroyedEnemies();
        return enemies.Count;
    }
    
    public List<GameObject> GetActiveEnemies()
    {
        CleanupDestroyedEnemies();
        return new List<GameObject>(enemies);
    }
    
    public void StartDownwardMovement()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                EnemyMover mover = enemy.GetComponent<EnemyMover>();
                if (mover == null)
                {
                    mover = enemy.AddComponent<EnemyMover>();
                }
                
                mover.moveSpeed = moveSpeed;
                mover.moveDirection = Vector3.down;
                mover.enabled = true;
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (enemyPrefab == null) return;
        
        Gizmos.color = Color.yellow;
        float totalWidth = (enemyCount - 1) * spacing;
        Vector3 startPosition = transform.position + new Vector3(-totalWidth / 2f, 0, 0);
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 enemyPosition = startPosition + new Vector3(i * spacing, 0, 0);
            Gizmos.DrawWireCube(enemyPosition, Vector3.one * 0.5f);
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, moveDirection.normalized * 2f);
    }
} 