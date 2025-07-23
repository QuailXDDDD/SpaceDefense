using UnityEngine;
using System.Collections.Generic;

public class GridFormation : MonoBehaviour
{
    [Header("Formation Settings")]
    public GameObject enemyPrefab;
    public int rows = 2;
    public int cols = 5;
    public float spacing = 1.5f;
    public Vector3 enemyScale = new Vector3(0.5f, 0.5f, 1f);
    
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public Vector3 moveDirection = Vector3.down;
    public bool stayInPosition = false;
    
    [Header("Spawn Settings")]
    public bool spawnFromOffScreen = true;
    public float entryDuration = 2f;
    
    private List<GameObject> enemies = new List<GameObject>();
    private Vector3 targetPosition;
    private bool hasEnteredScreen = false;
    private float startTime;
    
    void Start()
    {
        startTime = Time.time;
        
        if (spawnFromOffScreen)
        {
            float screenTop = Camera.main.orthographicSize;
            
            if (stayInPosition)
            {
                targetPosition = new Vector3(0, screenTop * 0.6f, 0);
            }
            else
            {
                targetPosition = new Vector3(0, screenTop - 1f, 0);
            }
            
            transform.position = new Vector3(0, screenTop + 3f, 0);
            
            Debug.Log($"GridFormation: Starting at {transform.position}, target: {targetPosition}, stayInPosition: {stayInPosition}");
        }
        else
        {
            targetPosition = transform.position;
        }
        
        CreateGridFormation();
        
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
        if (!hasEnteredScreen && Time.time > startTime + 5f)
        {
            Debug.Log("GridFormation: FALLBACK - Forcing hasEnteredScreen = true after timeout");
            hasEnteredScreen = true;
        }
        
        if (hasEnteredScreen && !stayInPosition)
        {
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
            
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"GridFormation: MOVING - Position: {transform.position}, Speed: {moveSpeed}");
            }
        }
        else if (hasEnteredScreen && stayInPosition)
        {
            if (Time.frameCount % 180 == 0)
            {
                Debug.Log($"GridFormation: STAYING IN POSITION - Position: {transform.position}");
            }
        }
        
        CleanupDestroyedEnemies();
        
        if (Time.time % 4f < Time.deltaTime)
        {
            Debug.Log($"GridFormation: {enemies.Count} enemies alive, hasEnteredScreen: {hasEnteredScreen}, stayInPosition: {stayInPosition}, position: {transform.position}");
        }
    }
    
    void CreateGridFormation()
    {
        Debug.Log($"GridFormation: Starting to create {rows}x{cols} grid formation");
        
        // Check if enemy prefab is assigned
        if (enemyPrefab == null)
        {
            Debug.LogError("GridFormation: Enemy prefab is not assigned!");
            return;
        }
        
        // Calculate formation dimensions and offset to center it
        float formationWidth = (cols - 1) * spacing;
        float formationHeight = (rows - 1) * spacing;
        Vector3 offset = new Vector3(-formationWidth / 2f, formationHeight / 2f, 0);
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // Calculate position for this enemy
                Vector3 gridPosition = new Vector3(col * spacing, -row * spacing, 0) + offset;
                Vector3 worldPosition = transform.position + gridPosition;
                
                Debug.Log($"GridFormation: Spawning enemy at row {row}, col {col}, position {worldPosition}");
                
                // Spawn enemy
                GameObject enemy = Instantiate(enemyPrefab, worldPosition, Quaternion.identity, transform);
                
                if (enemy == null)
                {
                    Debug.LogError($"GridFormation: Failed to instantiate enemy at row {row}, col {col}!");
                    continue;
                }
                
                // Set scale only if different from current scale
                if (enemy.transform.localScale != enemyScale)
                {
                    enemy.transform.localScale = enemyScale;
                }
                
                // Check if enemy has required components and fix missing EnemyData
                EnemyBehaviour enemyBehaviour = enemy.GetComponent<EnemyBehaviour>();
                if (enemyBehaviour != null && enemyBehaviour.enemyData == null)
                {
                    // Try to get EnemyData from the prefab's original components
                    EnemyBehaviour prefabEnemyBehaviour = enemyPrefab.GetComponent<EnemyBehaviour>();
                    if (prefabEnemyBehaviour != null && prefabEnemyBehaviour.enemyData != null)
                    {
                        enemyBehaviour.enemyData = prefabEnemyBehaviour.enemyData;
                        Debug.Log($"GridFormation: Assigned EnemyData to enemy at row {row}, col {col}");
                    }
                    else
                    {
                        Debug.LogWarning($"GridFormation: Enemy at row {row}, col {col} missing EnemyData!");
                    }
                }
                
                // Disable any existing EnemyMover component since formation handles all movement
                EnemyMover mover = enemy.GetComponent<EnemyMover>();
                if (mover != null)
                {
                    mover.enabled = false;
                }
                
                enemies.Add(enemy);
                Debug.Log($"GridFormation: Enemy at row {row}, col {col} created successfully");
            }
        }
        
        Debug.Log($"GridFormation: Created {enemies.Count} enemies in {rows}x{cols} grid. Transform has {transform.childCount} children.");
    }
    
    System.Collections.IEnumerator EntryMovement()
    {
        Debug.Log($"GridFormation: Starting entry movement from {transform.position} to {targetPosition} over {entryDuration} seconds");
        Vector3 startPos = transform.position;
        float timer = 0f;
        
        // Disable enemy behaviors during entry to prevent premature destruction
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
        
        Debug.Log($"GridFormation: Entry complete! Position: {transform.position}, hasEnteredScreen: {hasEnteredScreen}, stayInPosition: {stayInPosition}");
        
        if (stayInPosition)
        {
            Debug.Log($"GridFormation: Formation will STAY at current position");
        }
        else
        {
            Debug.Log($"GridFormation: Formation will continue MOVING DOWN through screen");
        }
        
        // FORCE the movement to start by ensuring all flags are correct
        if (stayInPosition)
        {
            Debug.Log("GridFormation: stayInPosition is TRUE - formation will hold position");
        }
        
        // Re-enable enemy behaviors but NOT individual movement
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
                
                // IMPORTANT: Do NOT enable EnemyMover - formation handles all movement
                // Keep enemies in formation by ensuring their relative positions are maintained
                EnemyMover mover = enemy.GetComponent<EnemyMover>();
                if (mover != null)
                {
                    mover.enabled = false; // Keep disabled to maintain formation
                }
                
                // Enable immediate shooting for any attack components
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
        
        Debug.Log($"GridFormation: Entered screen, {enemies.Count} enemies active in formation");
    }
    
    void CleanupDestroyedEnemies()
    {
        // Remove null references (destroyed enemies)
        enemies.RemoveAll(enemy => enemy == null);
    }
    
    // Public method to check if formation is cleared
    public bool IsFormationCleared()
    {
        CleanupDestroyedEnemies();
        return enemies.Count == 0;
    }
    
    // Public method to get the number of remaining enemies
    public int GetRemainingEnemyCount()
    {
        CleanupDestroyedEnemies();
        return enemies.Count;
    }
    
    // Public method to get all active enemies
    public List<GameObject> GetActiveEnemies()
    {
        CleanupDestroyedEnemies();
        return new List<GameObject>(enemies);
    }
    
    // Gizmo to show formation in editor
    void OnDrawGizmosSelected()
    {
        if (enemyPrefab == null) return;
        
        Gizmos.color = Color.yellow;
        float formationWidth = (cols - 1) * spacing;
        float formationHeight = (rows - 1) * spacing;
        Vector3 offset = new Vector3(-formationWidth / 2f, formationHeight / 2f, 0);
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 gridPosition = new Vector3(col * spacing, -row * spacing, 0) + offset;
                Vector3 worldPosition = transform.position + gridPosition;
                Gizmos.DrawWireCube(worldPosition, Vector3.one * 0.5f);
            }
        }
        
        // Draw movement direction
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, moveDirection.normalized * 2f);
    }
} 