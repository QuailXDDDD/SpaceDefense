using UnityEngine;
using System.Collections.Generic;

public class ZigZagFormation1 : MonoBehaviour
{
    [Header("Formation Settings")]
    public GameObject enemyPrefab;
    public float spacing = 1.0f;
    public Vector3 enemyScale = new Vector3(0.5f, 0.5f, 1f);
    
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float screenEdgeBuffer = 1f; // How far from screen edge before changing direction
    public float downwardSpeed = 1f; // Speed moving down the screen
    public bool stayInPosition = true; // If true, formation stays at target position for zigzag
    
    [Header("Zigzag Pattern")]
    public int rows = 4;
    public int cols = 6;
    
    [Header("Spawn Settings")]
    public bool spawnFromOffScreen = true;
    public float entryDuration = 2f; // Time to move from spawn to formation position
    
    private float screenLeftEdge;
    private float screenRightEdge;
    private List<EnemyRow> enemyRows = new List<EnemyRow>();
    private float formationHeight;
    private Vector3 targetPosition;
    private bool hasEnteredScreen = false;

    [System.Serializable]
    public class EnemyRow
    {
        public List<GameObject> enemies = new List<GameObject>();
        public bool movingRight;
        public float rowY;
    }

    void Start()
    {
        // Calculate screen boundaries using camera orthographic size
        Camera mainCam = Camera.main;
        float cameraWidth = mainCam.orthographicSize * mainCam.aspect;
        screenLeftEdge = -cameraWidth + screenEdgeBuffer;
        screenRightEdge = cameraWidth - screenEdgeBuffer;
        
        // Calculate formation dimensions
        formationHeight = (rows - 1) * spacing;
        
        if (spawnFromOffScreen)
        {
            // Set target position to upper part of screen for zigzag movement
            float screenHeight = mainCam.orthographicSize;
            targetPosition = new Vector3(0, screenHeight * 0.7f, 0); // 70% up the screen
            // Keep current off-screen position as starting point
            Debug.Log($"ZigZagFormation1: Starting at {transform.position}, target: {targetPosition}");
        }
        else
        {
            targetPosition = transform.position;
        }
        
        CreateZigzagFormation();
        
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
            MoveRows();
            
            // Only move formation down if not set to stay in position
            if (!stayInPosition)
            {
                MoveFormationDown();
            }
        }
        
        // Debug: Log formation status every few seconds
        if (Time.time % 3f < Time.deltaTime) // Every 3 seconds
        {
            int totalEnemies = GetRemainingEnemyCount();
            Debug.Log($"ZigZagFormation1: {totalEnemies} enemies alive, hasEnteredScreen: {hasEnteredScreen}, position: {transform.position}, stayInPosition: {stayInPosition}");
        }
    }

    void CreateZigzagFormation()
    {
        // Create enemies in a zigzag pattern
        for (int row = 0; row < rows; row++)
        {
            EnemyRow enemyRow = new EnemyRow();
            enemyRow.rowY = -row * spacing;
            
            // Alternate direction for each row
            enemyRow.movingRight = (row % 2 == 0);
            
            for (int col = 0; col < cols; col++)
            {
                // Create zigzag pattern: even rows start from left, odd rows start from right
                int actualCol = (row % 2 == 0) ? col : (cols - 1 - col);
                
                // Calculate position with zigzag offset
                float xPos = actualCol * spacing;
                float yPos = enemyRow.rowY;
                
                // Center the formation horizontally
                float formationWidth = (cols - 1) * spacing;
                Vector3 offset = new Vector3(-formationWidth / 2f, formationHeight / 2f, 0);
                Vector3 pos = new Vector3(xPos, yPos, 0) + offset;
                
                GameObject enemy = Instantiate(enemyPrefab, transform.position + pos, Quaternion.identity, transform);
                
                // Set scale only if different from current scale
                if (enemy.transform.localScale != enemyScale)
                {
                    enemy.transform.localScale = enemyScale;
                }
                
                enemyRow.enemies.Add(enemy);
            }
            
            enemyRows.Add(enemyRow);
        }
    }
    
    System.Collections.IEnumerator EntryMovement()
    {
        Vector3 startPos = transform.position;
        Debug.Log($"ZigZagFormation1: Starting entry movement from {startPos} to {targetPosition} over {entryDuration} seconds");
        
        // Temporarily disable enemy behaviors during entry to prevent off-screen destruction
        List<EnemyBehaviour> enemyBehaviours = new List<EnemyBehaviour>();
        foreach (EnemyRow row in enemyRows)
        {
            foreach (GameObject enemy in row.enemies)
            {
                if (enemy != null)
                {
                    EnemyBehaviour enemyBehaviour = enemy.GetComponent<EnemyBehaviour>();
                    if (enemyBehaviour != null)
                    {
                        enemyBehaviours.Add(enemyBehaviour);
                        enemyBehaviour.enabled = false;
                    }
                }
            }
        }
        
        float timer = 0f;
        
        while (timer < entryDuration)
        {
            float progress = timer / entryDuration;
            transform.position = Vector3.Lerp(startPos, targetPosition, progress);
            timer += Time.deltaTime;
            
            // Debug progress every 0.5 seconds
            if (timer % 0.5f < Time.deltaTime)
            {
                Debug.Log($"ZigZagFormation1: Entry progress {(progress * 100):F0}% - Position: {transform.position}");
            }
            
            yield return null;
        }
        
        transform.position = targetPosition;
        hasEnteredScreen = true;
        
        // Re-enable enemy behaviors
        foreach (EnemyBehaviour enemyBehaviour in enemyBehaviours)
        {
            if (enemyBehaviour != null)
            {
                enemyBehaviour.enabled = true;
            }
        }
        
        Debug.Log($"ZigZagFormation1: Entry complete! Final position: {targetPosition}, starting zigzag movement. StayInPosition: {stayInPosition}");
        
        // Enable enemy shooting immediately after entry
        foreach (EnemyRow row in enemyRows)
        {
            foreach (GameObject enemy in row.enemies)
            {
                if (enemy != null)
                {
                    EnemyBehaviour enemyBehaviour = enemy.GetComponent<EnemyBehaviour>();
                    if (enemyBehaviour != null)
                    {
                        enemyBehaviour.enabled = true;
                        enemyBehaviour.EnableImmediateShooting();
                    }
                    
                    // Also enable immediate shooting for any attack components
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
        }
    }
    

    
    void MoveFormationDown()
    {
        // Move the entire formation downward
        transform.Translate(Vector3.down * downwardSpeed * Time.deltaTime);
    }

    void MoveRows()
    {
        foreach (EnemyRow row in enemyRows)
        {
            // Check if any enemy in this row hits screen edges
            bool shouldReverse = false;
            
            foreach (GameObject enemy in row.enemies)
            {
                if (enemy != null)
                {
                    float enemyX = enemy.transform.position.x;
                    
                    if (row.movingRight && enemyX >= screenRightEdge)
                    {
                        shouldReverse = true;
                        break;
                    }
                    else if (!row.movingRight && enemyX <= screenLeftEdge)
                    {
                        shouldReverse = true;
                        break;
                    }
                }
            }
            
            // Reverse direction if needed
            if (shouldReverse)
            {
                row.movingRight = !row.movingRight;
            }
            
            // Move all enemies in this row
            float direction = row.movingRight ? 1f : -1f;
            foreach (GameObject enemy in row.enemies)
            {
                if (enemy != null)
                {
                    enemy.transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);
                }
            }
        }
        
        // Clean up destroyed enemies from the lists
        CleanupDestroyedEnemies();
    }
    
    void CleanupDestroyedEnemies()
    {
        foreach (EnemyRow row in enemyRows)
        {
            // Remove null references (destroyed enemies)
            row.enemies.RemoveAll(enemy => enemy == null);
        }
    }
    
    // Public method to check if formation is cleared
    public bool IsFormationCleared()
    {
        int totalEnemies = 0;
        foreach (EnemyRow row in enemyRows)
        {
            totalEnemies += row.enemies.Count;
        }
        return totalEnemies == 0;
    }
    
    // Public method to get the number of remaining enemies
    public int GetRemainingEnemyCount()
    {
        int count = 0;
        foreach (EnemyRow row in enemyRows)
        {
            count += row.enemies.Count;
        }
        return count;
    }
} 