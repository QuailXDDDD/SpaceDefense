using UnityEngine;
using System.Collections.Generic;

public class CircleFormation : MonoBehaviour
{
    [Header("Formation Settings")]
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public int enemyCount = 5;
    public float circleRadius = 3f;
    public Vector3 enemyScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 bossScale = new Vector3(0.8f, 0.8f, 1f);
    
    [Header("Movement Settings")]
    public float moveSpeed = 1f; // Formation movement speed (downward)
    public float rotationSpeed = 30f; // Degrees per second for circle rotation
    public Vector3 moveDirection = Vector3.down;
    public bool stayInPosition = true; // If true, formation stays at target position for rotation
    
    [Header("Spawn Settings")]
    public bool spawnFromOffScreen = true;
    public float entryDuration = 3f; // Time to move from spawn to formation position
    
    [Header("Boss Settings")]
    public bool activateBossOnEntry = true;
    
    private List<GameObject> circleEnemies = new List<GameObject>();
    private GameObject bossEnemy;
    private Vector3 targetPosition;
    private bool hasEnteredScreen = false;
    private float currentRotationAngle = 0f;
    
    // Store initial positions relative to formation center
    private List<Vector3> initialEnemyPositions = new List<Vector3>();
    
    void Start()
    {
        Camera mainCam = Camera.main;
        float screenHeight = mainCam.orthographicSize;
        
        if (spawnFromOffScreen)
        {
            // Set target position to upper part of screen for circle rotation
            targetPosition = new Vector3(0, screenHeight * 0.6f, 0); // 60% up the screen
            // Keep current off-screen position as starting point
            Debug.Log($"CircleFormation: Starting at {transform.position}, target: {targetPosition}");
        }
        else
        {
            targetPosition = transform.position;
        }
        
        CreateCircleFormation();
        
        if (spawnFromOffScreen)
        {
            StartCoroutine(EntryMovement());
        }
        else
        {
            hasEnteredScreen = true;
            if (activateBossOnEntry)
            {
                ActivateBoss();
            }
        }
    }
    
    void Update()
    {
        if (hasEnteredScreen)
        {
            RotateCircle();
            
            // Only move formation down if not set to stay in position
            if (!stayInPosition)
            {
                MoveFormation();
            }
        }
        
        // Clean up destroyed enemies
        CleanupDestroyedEnemies();
        
        // Debug: Log formation status every few seconds
        if (Time.time % 3f < Time.deltaTime) // Every 3 seconds
        {
            int totalEnemies = GetRemainingEnemyCount();
            Debug.Log($"CircleFormation: {totalEnemies} enemies/boss alive, hasEnteredScreen: {hasEnteredScreen}, position: {transform.position}, stayInPosition: {stayInPosition}");
        }
    }
    
    void CreateCircleFormation()
    {
        // First, create the boss in the center
        if (bossPrefab != null)
        {
            bossEnemy = Instantiate(bossPrefab, transform.position, Quaternion.identity, transform);
            
            // Set scale only if different from current scale
            if (bossEnemy.transform.localScale != bossScale)
            {
                bossEnemy.transform.localScale = bossScale;
            }
            
            // Disable boss initially if we're spawning from off-screen
            if (spawnFromOffScreen)
            {
                BossEnemy bossScript = bossEnemy.GetComponent<BossEnemy>();
                if (bossScript != null)
                {
                    bossScript.enabled = false;
                }
                
                // Disable boss attack components initially
                MonoBehaviour[] attackComponents = bossEnemy.GetComponents<MonoBehaviour>();
                foreach (var component in attackComponents)
                {
                    if (component is BasicAttacker || component is BurstAttacker || component is SpreadAttacker || component is BossPhaseAttacker)
                    {
                        component.enabled = false;
                    }
                }
            }
            
            Debug.Log("CircleFormation: Boss created at center");
        }
        
        // Create enemies in circle around the boss
        float angleStep = 360f / enemyCount;
        
        for (int i = 0; i < enemyCount; i++)
        {
            // Calculate angle in radians
            float angle = i * angleStep * Mathf.Deg2Rad;
            
            // Calculate position on circle
            Vector3 circlePosition = new Vector3(
                Mathf.Cos(angle) * circleRadius,
                Mathf.Sin(angle) * circleRadius,
                0
            );
            
            // Store initial position
            initialEnemyPositions.Add(circlePosition);
            
            // Spawn enemy
            GameObject enemy = Instantiate(enemyPrefab, transform.position + circlePosition, Quaternion.identity, transform);
            
            // Set scale only if different from current scale
            if (enemy.transform.localScale != enemyScale)
            {
                enemy.transform.localScale = enemyScale;
            }
            
            // Disable individual enemy movement - formation will handle it
            EnemyBehaviour enemyBehaviour = enemy.GetComponent<EnemyBehaviour>();
            if (enemyBehaviour != null)
            {
                // Set move speed to 0 so it doesn't move individually
                if (enemyBehaviour.enemyData != null)
                {
                    enemyBehaviour.enemyData.moveSpeed = 0f;
                }
            }
            
            EnemyMover mover = enemy.GetComponent<EnemyMover>();
            if (mover != null)
            {
                mover.enabled = false;
            }
            
            circleEnemies.Add(enemy);
        }
        
        Debug.Log($"CircleFormation: Created {enemyCount} enemies in circle formation around boss");
    }
    
    System.Collections.IEnumerator EntryMovement()
    {
        Vector3 startPos = transform.position;
        Debug.Log($"CircleFormation: Starting entry movement from {startPos} to {targetPosition} over {entryDuration} seconds");
        
        // Temporarily disable enemy behaviors during entry to prevent off-screen destruction
        List<EnemyBehaviour> enemyBehaviours = new List<EnemyBehaviour>();
        foreach (GameObject enemy in circleEnemies)
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
        
        // Also disable boss behaviors during entry
        BossEnemy bossScript = null;
        if (bossEnemy != null)
        {
            bossScript = bossEnemy.GetComponent<BossEnemy>();
            if (bossScript != null)
            {
                bossScript.enabled = false;
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
                Debug.Log($"CircleFormation: Entry progress {(progress * 100):F0}% - Position: {transform.position}");
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
        
        // Activate the boss
        if (activateBossOnEntry)
        {
            ActivateBoss();
        }
        
        Debug.Log($"CircleFormation: Entry complete! Final position: {targetPosition}, starting rotation. StayInPosition: {stayInPosition}");
        
        // Enable enemy shooting immediately after entry
        foreach (GameObject enemy in circleEnemies)
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
        
        // Also enable boss shooting immediately
        if (bossEnemy != null)
        {
            BossEnemy bossComponent = bossEnemy.GetComponent<BossEnemy>();
            if (bossComponent != null)
            {
                bossComponent.enabled = true;
            }
            
            // Enable immediate shooting for boss attack components
            BasicAttacker basicAttacker = bossEnemy.GetComponent<BasicAttacker>();
            if (basicAttacker != null)
            {
                basicAttacker.EnableImmediateShooting();
            }
            
            SpreadAttacker spreadAttacker = bossEnemy.GetComponent<SpreadAttacker>();
            if (spreadAttacker != null)
            {
                spreadAttacker.EnableImmediateShooting();
            }
            
            BurstAttacker burstAttacker = bossEnemy.GetComponent<BurstAttacker>();
            if (burstAttacker != null)
            {
                burstAttacker.EnableImmediateShooting();
            }
            
            // Check for boss-specific attack components
            BossPhaseAttacker bossPhaseAttacker = bossEnemy.GetComponent<BossPhaseAttacker>();
            if (bossPhaseAttacker != null)
            {
                bossPhaseAttacker.enabled = true;
                bossPhaseAttacker.EnableImmediateShooting();
            }
        }
    }
    

    
    void ActivateBoss()
    {
        if (bossEnemy != null)
        {
            BossEnemy bossScript = bossEnemy.GetComponent<BossEnemy>();
            if (bossScript != null)
            {
                bossScript.enabled = true;
                Debug.Log("CircleFormation: BossEnemy script enabled");
            }
            
            // Enable boss attack components
            MonoBehaviour[] attackComponents = bossEnemy.GetComponents<MonoBehaviour>();
            int enabledCount = 0;
            foreach (var component in attackComponents)
            {
                if (component is BasicAttacker || component is BurstAttacker || component is SpreadAttacker || component is BossPhaseAttacker)
                {
                    component.enabled = true;
                    enabledCount++;
                }
            }
            
            Debug.Log($"CircleFormation: Boss activated! Enabled {enabledCount} attack components at position {bossEnemy.transform.position}");
        }
        else
        {
            Debug.LogWarning("CircleFormation: Cannot activate boss - bossEnemy is null!");
        }
    }
    
    void RotateCircle()
    {
        // Update rotation angle
        currentRotationAngle += rotationSpeed * Time.deltaTime;
        
        // Apply rotation to circle enemies
        for (int i = 0; i < circleEnemies.Count && i < initialEnemyPositions.Count; i++)
        {
            if (circleEnemies[i] != null)
            {
                // Get the initial position and apply rotation
                Vector3 initialPos = initialEnemyPositions[i];
                
                // Apply rotation matrix
                float radians = currentRotationAngle * Mathf.Deg2Rad;
                Vector3 rotatedPos = new Vector3(
                    initialPos.x * Mathf.Cos(radians) - initialPos.y * Mathf.Sin(radians),
                    initialPos.x * Mathf.Sin(radians) + initialPos.y * Mathf.Cos(radians),
                    initialPos.z
                );
                
                // Set the enemy's position relative to formation center
                circleEnemies[i].transform.position = transform.position + rotatedPos;
            }
        }
    }
    
    void MoveFormation()
    {
        // Move the entire formation downward
        transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime);
    }
    
    void CleanupDestroyedEnemies()
    {
        // Remove null references and their corresponding initial positions
        for (int i = circleEnemies.Count - 1; i >= 0; i--)
        {
            if (circleEnemies[i] == null)
            {
                circleEnemies.RemoveAt(i);
                if (i < initialEnemyPositions.Count)
                {
                    initialEnemyPositions.RemoveAt(i);
                }
            }
        }
        
        // Check if boss is destroyed
        if (bossEnemy == null)
        {
            Debug.Log("CircleFormation: Boss has been destroyed!");
        }
    }
    
    // Public method to check if formation is cleared (all enemies + boss destroyed)
    public bool IsFormationCleared()
    {
        CleanupDestroyedEnemies();
        return circleEnemies.Count == 0 && bossEnemy == null;
    }
    
    // Public method to check if only circle enemies are cleared (boss might still be alive)
    public bool AreCircleEnemiesCleared()
    {
        CleanupDestroyedEnemies();
        return circleEnemies.Count == 0;
    }
    
    // Public method to check if boss is destroyed
    public bool IsBossDestroyed()
    {
        return bossEnemy == null;
    }
    
    // Public method to get the number of remaining enemies (circle enemies + boss)
    public int GetRemainingEnemyCount()
    {
        CleanupDestroyedEnemies();
        int count = circleEnemies.Count;
        if (bossEnemy != null) count++;
        return count;
    }
    
    // Public method to get all active enemies (circle enemies + boss)
    public List<GameObject> GetActiveEnemies()
    {
        CleanupDestroyedEnemies();
        List<GameObject> allEnemies = new List<GameObject>(circleEnemies);
        if (bossEnemy != null)
        {
            allEnemies.Add(bossEnemy);
        }
        return allEnemies;
    }
    
    // Public method to manually activate boss
    public void ManuallyActivateBoss()
    {
        ActivateBoss();
    }
    
    // Public method to adjust rotation speed
    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }
    
    // Public method to adjust circle radius (will recalculate positions)
    public void SetCircleRadius(float newRadius)
    {
        circleRadius = newRadius;
        
        // Recalculate initial positions
        initialEnemyPositions.Clear();
        float angleStep = 360f / enemyCount;
        
        for (int i = 0; i < enemyCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 circlePosition = new Vector3(
                Mathf.Cos(angle) * circleRadius,
                Mathf.Sin(angle) * circleRadius,
                0
            );
            initialEnemyPositions.Add(circlePosition);
        }
    }
    
    // Gizmo to show formation in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        
        // Draw circle outline
        Vector3 center = transform.position;
        float angleStep = 360f / Mathf.Max(enemyCount, 1);
        Vector3 previousPoint = center + new Vector3(circleRadius, 0, 0);
        
        for (int i = 1; i <= enemyCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 currentPoint = center + new Vector3(
                Mathf.Cos(angle) * circleRadius,
                Mathf.Sin(angle) * circleRadius,
                0
            );
            
            Gizmos.DrawLine(previousPoint, currentPoint);
            Gizmos.DrawWireCube(currentPoint, Vector3.one * 0.5f);
            previousPoint = currentPoint;
        }
        
        // Draw boss position
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, Vector3.one * 0.8f);
        
        // Draw movement direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, moveDirection.normalized * 2f);
    }
} 