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
    public float screenEdgeBuffer = 1f;
    public float downwardSpeed = 1f;
    public bool stayInPosition = true;
    
    [Header("Zigzag Pattern")]
    public int rows = 4;
    public int cols = 6;
    
    [Header("Spawn Settings")]
    public bool spawnFromOffScreen = true;
    public float entryDuration = 2f;
    
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
        Camera mainCam = Camera.main;
        float cameraWidth = mainCam.orthographicSize * mainCam.aspect;
        screenLeftEdge = -cameraWidth + screenEdgeBuffer;
        screenRightEdge = cameraWidth - screenEdgeBuffer;
        
        formationHeight = (rows - 1) * spacing;
        
        if (spawnFromOffScreen)
        {
            float screenHeight = mainCam.orthographicSize;
            // Điều chỉnh target position để đảm bảo toàn bộ formation hiển thị trong khung hình
            // Tính toán position sao cho hàng trên cùng không bị ra ngoài màn hình
            float maxY = screenHeight - 0.5f; // Để lại khoảng trống 0.5f từ edge
            float formationTopOffset = formationHeight / 2f;
            targetPosition = new Vector3(0, Mathf.Min(screenHeight * 0.7f, maxY - formationTopOffset), 0);
            Debug.Log($"ZigZagFormation1: Starting at {transform.position}, target: {targetPosition}, formationHeight: {formationHeight}");
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
            
            if (!stayInPosition)
            {
                MoveFormationDown();
            }
        }
        
        if (Time.time % 3f < Time.deltaTime)
        {
            int totalEnemies = GetRemainingEnemyCount();
            Debug.Log($"ZigZagFormation1: {totalEnemies} enemies alive, hasEnteredScreen: {hasEnteredScreen}, position: {transform.position}, stayInPosition: {stayInPosition}");
        }
    }

    void CreateZigzagFormation()
    {
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        
        for (int row = 0; row < rows; row++)
        {
            EnemyRow enemyRow = new EnemyRow();
            enemyRow.rowY = -row * spacing;
            
            enemyRow.movingRight = (row % 2 == 0);
            
            for (int col = 0; col < cols; col++)
            {
                int actualCol = (row % 2 == 0) ? col : (cols - 1 - col);
                
                float xPos = actualCol * spacing;
                float yPos = enemyRow.rowY;
                
                float formationWidth = (cols - 1) * spacing;
                Vector3 offset = new Vector3(-formationWidth / 2f, formationHeight / 2f, 0);
                Vector3 pos = new Vector3(xPos, yPos, 0) + offset;
                Vector3 worldPos = transform.position + pos;
                
                // Track min/max Y positions
                minY = Mathf.Min(minY, worldPos.y);
                maxY = Mathf.Max(maxY, worldPos.y);
                
                GameObject enemy = Instantiate(enemyPrefab, worldPos, Quaternion.identity, transform);
                
                if (enemy.transform.localScale != enemyScale)
                {
                    enemy.transform.localScale = enemyScale;
                }
                
                enemyRow.enemies.Add(enemy);
            }
            
            enemyRows.Add(enemyRow);
        }
        
        // Debug log để kiểm tra vị trí formation
        float screenHeight = Camera.main.orthographicSize;
        Debug.Log($"ZigZagFormation1: Formation created. Y range: {minY:F2} to {maxY:F2} (Screen height: -{screenHeight:F2} to {screenHeight:F2})");
        
        if (maxY > screenHeight)
        {
            Debug.LogWarning($"ZigZagFormation1: Formation extends above screen! Top at {maxY:F2}, screen top at {screenHeight:F2}");
        }
        if (minY < -screenHeight)
        {
            Debug.LogWarning($"ZigZagFormation1: Formation extends below screen! Bottom at {minY:F2}, screen bottom at {-screenHeight:F2}");
        }
    }
    
    System.Collections.IEnumerator EntryMovement()
    {
        Vector3 startPos = transform.position;
        Debug.Log($"ZigZagFormation1: Starting entry movement from {startPos} to {targetPosition} over {entryDuration} seconds");
        

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
            
            if (timer % 0.5f < Time.deltaTime)
            {
                Debug.Log($"ZigZagFormation1: Entry progress {(progress * 100):F0}% - Position: {transform.position}");
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
        
        Debug.Log($"ZigZagFormation1: Entry complete! Final position: {targetPosition}, starting zigzag movement. StayInPosition: {stayInPosition}");
        

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
        transform.Translate(Vector3.down * downwardSpeed * Time.deltaTime);
    }

    void MoveRows()
    {
        foreach (EnemyRow row in enemyRows)
        {
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
            
            if (shouldReverse)
            {
                row.movingRight = !row.movingRight;
            }
            
            float direction = row.movingRight ? 1f : -1f;
            foreach (GameObject enemy in row.enemies)
            {
                if (enemy != null)
                {
                    enemy.transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);
                }
            }
        }
        
        CleanupDestroyedEnemies();
    }
    
    void CleanupDestroyedEnemies()
    {
        foreach (EnemyRow row in enemyRows)
        {
            row.enemies.RemoveAll(enemy => enemy == null);
        }
    }
    

    public bool IsFormationCleared()
    {
        int totalEnemies = 0;
        foreach (EnemyRow row in enemyRows)
        {
            totalEnemies += row.enemies.Count;
        }
        return totalEnemies == 0;
    }
    
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