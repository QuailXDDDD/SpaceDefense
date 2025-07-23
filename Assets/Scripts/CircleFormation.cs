using UnityEngine;
using System.Collections.Generic;

public class CircleFormation : MonoBehaviour
{
    [Header("Formation Settings")]
    public GameObject enemyPrefab;
    public GameObject specialEnemyPrefab;
    public GameObject bossPrefab;
    public int enemyCount = 5;
    public float circleRadius = 3f;
    public Vector3 enemyScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 bossScale = new Vector3(0.8f, 0.8f, 1f);
    
    [Header("Movement Settings")]
    public float moveSpeed = 1f;
    public float rotationSpeed = 30f;
    public Vector3 moveDirection = Vector3.down;
    public bool stayInPosition = true;
    
    [Header("Spawn Settings")]
    public bool spawnFromOffScreen = true;
    public float entryDuration = 3f;
    
    [Header("Boss Settings")]
    public bool activateBossOnEntry = true;
    
    private List<GameObject> circleEnemies = new List<GameObject>();
    private GameObject bossEnemy;
    private Vector3 targetPosition;
    private bool hasEnteredScreen = false;
    private float currentRotationAngle = 0f;
    
    private List<Vector3> initialEnemyPositions = new List<Vector3>();
    
    void Start()
    {
        Camera mainCam = Camera.main;
        float screenHeight = mainCam.orthographicSize;
        
        if (spawnFromOffScreen)
        {
            targetPosition = new Vector3(0, screenHeight * 0.6f, 0);
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
            
            if (!stayInPosition)
            {
                MoveFormation();
            }
        }
        
        CleanupDestroyedEnemies();
        
        if (Time.time % 3f < Time.deltaTime)
        {
            int totalEnemies = GetRemainingEnemyCount();
            Debug.Log($"CircleFormation: {totalEnemies} enemies/boss alive, hasEnteredScreen: {hasEnteredScreen}, position: {transform.position}, stayInPosition: {stayInPosition}");
        }
    }
    
    void CreateCircleFormation()
    {
        if (bossPrefab != null)
        {
            Debug.Log($"CircleFormation: Creating boss at position {transform.position}");
            bossEnemy = Instantiate(bossPrefab, transform.position, Quaternion.identity, transform);
            
            if (bossEnemy == null)
            {
                Debug.LogError("CircleFormation: Failed to instantiate boss!");
                return;
            }
            
            Debug.Log($"CircleFormation: Boss instantiated successfully. Position: {bossEnemy.transform.position}, Active: {bossEnemy.activeInHierarchy}");
            
            if (bossEnemy.transform.localScale != bossScale)
            {
                bossEnemy.transform.localScale = bossScale;
            }
            
            BossEnemy bossScript = bossEnemy.GetComponent<BossEnemy>();
            if (bossScript != null)
            {
                Debug.Log($"CircleFormation: Boss script found. Health: {bossScript.CurrentHealth}");
                
                if (spawnFromOffScreen)
                {
                    bossScript.enabled = false;
                    Debug.Log("CircleFormation: Boss script disabled for entry movement");
                }
            }
            else
            {
                Debug.LogError("CircleFormation: Boss script not found on boss prefab!");
            }
            
            SpriteRenderer bossRenderer = bossEnemy.GetComponent<SpriteRenderer>();
            if (bossRenderer != null)
            {
                Debug.Log($"CircleFormation: Boss sprite renderer - Sprite: {bossRenderer.sprite != null}, Color: {bossRenderer.color}, Enabled: {bossRenderer.enabled}, SortingOrder: {bossRenderer.sortingOrder}");
                
                bossRenderer.enabled = true;
                
                if (bossRenderer.color.a < 0.1f)
                {
                    bossRenderer.color = Color.white;
                    Debug.Log("CircleFormation: Fixed boss color (was transparent)");
                }
                
                bossRenderer.sortingOrder = 10;
                
                if (bossRenderer.sprite == null)
                {
                    BossEnemy bossComponent = bossEnemy.GetComponent<BossEnemy>();
                    if (bossComponent != null && bossComponent.enemyData != null && bossComponent.enemyData.enemySprite != null)
                    {
                        bossRenderer.sprite = bossComponent.enemyData.enemySprite;
                        Debug.Log("CircleFormation: Assigned sprite from BossEnemyData");
                    }
                    else
                    {
                        Debug.LogWarning("CircleFormation: Boss has no sprite assigned!");
                    }
                }
                
                Debug.Log($"CircleFormation: After fixes - Sprite: {bossRenderer.sprite != null}, Color: {bossRenderer.color}, SortingOrder: {bossRenderer.sortingOrder}");
            }
            else
            {
                Debug.LogError("CircleFormation: No SpriteRenderer found on boss!");
            }
            
            if (spawnFromOffScreen)
            {
                MonoBehaviour[] attackComponents = bossEnemy.GetComponents<MonoBehaviour>();
                int disabledCount = 0;
                foreach (var component in attackComponents)
                {
                    if (component is BasicAttacker || component is BurstAttacker || component is SpreadAttacker || component is BossPhaseAttacker)
                    {
                        component.enabled = false;
                        disabledCount++;
                    }
                }
                Debug.Log($"CircleFormation: Disabled {disabledCount} attack components");
            }
            
            Debug.Log("CircleFormation: Boss created at center");
        }
        else
        {
            Debug.LogError("CircleFormation: Boss prefab is null!");
        }
        
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
            
            GameObject prefabToUse = enemyPrefab;
            bool isSpecialEnemy = false;
            
            if (i == 0 && specialEnemyPrefab != null)
            {
                prefabToUse = specialEnemyPrefab;
                isSpecialEnemy = true;
                Debug.Log("CircleFormation: Spawning special invincibility drop enemy at position 0");
            }
            
            GameObject enemy = Instantiate(prefabToUse, transform.position + circlePosition, Quaternion.identity, transform);
            
            if (enemy.transform.localScale != enemyScale)
            {
                enemy.transform.localScale = enemyScale;
            }
            
            EnemyBehaviour enemyBehaviour = enemy.GetComponent<EnemyBehaviour>();
            if (enemyBehaviour != null)
            {
                if (enemyBehaviour.enemyData != null)
                {
                    enemyBehaviour.enemyData.moveSpeed = 0f;
                }
                
                if (isSpecialEnemy)
                {
                    enemyBehaviour.canDropPowerUps = true;
                    Debug.Log("CircleFormation: Enabled power-up drops for special enemy");
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
            
            if (timer % 0.5f < Time.deltaTime)
            {
                Debug.Log($"CircleFormation: Entry progress {(progress * 100):F0}% - Position: {transform.position}");
            }
            
            yield return null;
        }
        
        transform.position = targetPosition;
        hasEnteredScreen = true;
        
        foreach (EnemyBehaviour enemyBehaviour in enemyBehaviours)
        {
            if (enemyBehaviour != null)
            {
                enemyBehaviour.enabled = true;
            }
        }
        
        if (activateBossOnEntry)
        {
            ActivateBoss();
        }
        
        Debug.Log($"CircleFormation: Entry complete! Final position: {targetPosition}, starting rotation. StayInPosition: {stayInPosition}");
        
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
        
        if (bossEnemy != null)
        {
            BossEnemy bossComponent = bossEnemy.GetComponent<BossEnemy>();
            if (bossComponent != null)
            {
                bossComponent.enabled = true;
            }
            
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
            Debug.Log($"CircleFormation: Activating boss. Current state - Active: {bossEnemy.activeInHierarchy}, Position: {bossEnemy.transform.position}");
            
            BossEnemy bossScript = bossEnemy.GetComponent<BossEnemy>();
            if (bossScript != null)
            {
                bossScript.enabled = true;
                Debug.Log($"CircleFormation: BossEnemy script enabled. Health: {bossScript.CurrentHealth}");
                
                if (bossEnemy == null)
                {
                    Debug.LogError("CircleFormation: Boss was destroyed after enabling script!");
                    return;
                }
            }
            else
            {
                Debug.LogError("CircleFormation: Boss script not found during activation!");
            }
            
            SpriteRenderer bossRenderer = bossEnemy.GetComponent<SpriteRenderer>();
            if (bossRenderer != null)
            {
                Debug.Log($"CircleFormation: After activation - Boss sprite: {bossRenderer.sprite != null}, Color: {bossRenderer.color}, Enabled: {bossRenderer.enabled}, SortingOrder: {bossRenderer.sortingOrder}");
                
                bossRenderer.enabled = true;
                bossRenderer.sortingOrder = 10;
                
                if (bossRenderer.color.a < 0.1f || bossRenderer.color == Color.clear)
                {
                    bossRenderer.color = Color.white;
                    Debug.Log("CircleFormation: Fixed boss color after activation");
                }
            }
            
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
            Debug.LogError("CircleFormation: Cannot activate boss - bossEnemy is null!");
        }
    }
    
    void RotateCircle()
    {
        currentRotationAngle += rotationSpeed * Time.deltaTime;
        
        for (int i = 0; i < circleEnemies.Count && i < initialEnemyPositions.Count; i++)
        {
            if (circleEnemies[i] != null)
            {
                Vector3 initialPos = initialEnemyPositions[i];
                
                float radians = currentRotationAngle * Mathf.Deg2Rad;
                Vector3 rotatedPos = new Vector3(
                    initialPos.x * Mathf.Cos(radians) - initialPos.y * Mathf.Sin(radians),
                    initialPos.x * Mathf.Sin(radians) + initialPos.y * Mathf.Cos(radians),
                    initialPos.z
                );
                
                circleEnemies[i].transform.position = transform.position + rotatedPos;
            }
        }
    }
    
    void MoveFormation()
    {
        transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime);
    }
    
    void CleanupDestroyedEnemies()
    {
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
        
        if (bossEnemy == null)
        {
            Debug.Log("CircleFormation: Boss has been destroyed!");
        }
    }
    
    public bool IsFormationCleared()
    {
        CleanupDestroyedEnemies();
        return circleEnemies.Count == 0 && bossEnemy == null;
    }
    
    public bool AreCircleEnemiesCleared()
    {
        CleanupDestroyedEnemies();
        return circleEnemies.Count == 0;
    }
    
    public bool IsBossDestroyed()
    {
        return bossEnemy == null;
    }
    
    public int GetRemainingEnemyCount()
    {
        CleanupDestroyedEnemies();
        int count = circleEnemies.Count;
        if (bossEnemy != null) count++;
        return count;
    }
    
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
    
    public void ManuallyActivateBoss()
    {
        ActivateBoss();
    }
    
    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }
    
    public void SetCircleRadius(float newRadius)
    {
        circleRadius = newRadius;
        
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
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        
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
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, Vector3.one * 0.8f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, moveDirection.normalized * 2f);
    }
} 