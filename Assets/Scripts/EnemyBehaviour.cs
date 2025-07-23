using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public EnemyData enemyData;
    
    [Header("Effects")]
    public GameObject explosionPrefab;
    
    [Header("Spawn Protection")]
    public float spawnInvincibilityDuration = 5f;
    public bool hasSpawnProtection = true;

    private int currentHealth;
    private float nextFireTime;
    private SpriteRenderer spriteRenderer;
    private bool isSpawnInvincible = false;
    private Coroutine spawnProtectionCoroutine;
    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (enemyData == null)
        {
            Debug.LogWarning("EnemyData is not assigned to " + gameObject.name + "! Trying to find one...", this);
            
            Enemy enemyComponent = GetComponent<Enemy>();
            if (enemyComponent != null && enemyComponent.enemyData != null)
            {
                enemyData = enemyComponent.enemyData;
                Debug.Log("Found EnemyData from Enemy component!");
            }
            else
            {
                EnemyData[] allEnemyData = Resources.FindObjectsOfTypeAll<EnemyData>();
                if (allEnemyData.Length > 0)
                {
                    enemyData = allEnemyData[0];
                    Debug.LogWarning($"Using fallback EnemyData: {enemyData.name}");
                }
                else
                {
                    Debug.LogError("No EnemyData found anywhere! Enemy will be destroyed.");
                    Destroy(gameObject);
                    return;
                }
            }
        }

        currentHealth = enemyData.maxHealth;

        if (spriteRenderer != null && enemyData.enemySprite != null)
        {
            spriteRenderer.sprite = enemyData.enemySprite;
        }
        transform.localScale = enemyData.scale;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        nextFireTime = Time.time + enemyData.baseFireRate;
        
        if (hasSpawnProtection)
        {
            StartSpawnProtection();
        }
    }

    public void EnableImmediateShooting()
    {
        nextFireTime = Time.time;
    }
    
    void Update()
    {
        bool isPartOfFormation = transform.parent != null && 
                                (transform.parent.GetComponent<ZigZagFormation1>() != null || 
                                 transform.parent.GetComponent<SkullFormation>() != null ||
                                 transform.parent.GetComponent<GridFormation>() != null);
        
        if (!isPartOfFormation)
        {
            transform.Translate(Vector2.down * enemyData.moveSpeed * Time.deltaTime);
        }

        if (Time.time >= nextFireTime && enemyData.projectilePrefab != null)
        {
            Shoot();
            nextFireTime = Time.time + enemyData.baseFireRate;
        }
    }

    void Shoot()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyShoot();
        }
        
        GameObject projectileGO = Instantiate(enemyData.projectilePrefab, transform.position, Quaternion.identity);

        EnemyProjectile enemyProjectile = projectileGO.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            enemyProjectile.damage = enemyData.projectileDamage;
            enemyProjectile.speed = enemyData.projectileSpeed;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isSpawnInvincible)
        {
            Debug.Log($"Enemy {gameObject.name} is protected by spawn invincibility! Damage blocked.");
            return;
        }
        
        currentHealth -= damageAmount;
        Debug.Log($"{enemyData.enemyName} took {damageAmount} damage. Current Health: {currentHealth}");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHit();
        }

        if (currentHealth <= 0)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyExplosion();
            }
            
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            
            TryDropPowerUp();
            
            GameObject canvasObject = GameObject.Find("Canvas");
            if (canvasObject != null)
            {
                GameUI gameUI = canvasObject.GetComponent<GameUI>();
                if (gameUI != null)
                {
                    gameUI.AddScore(enemyData.scoreValue);
                }
            }
            
            Debug.Log($"{enemyData.enemyName} destroyed! Score added: {enemyData.scoreValue}");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            BulletScript bulletScript = other.GetComponent<BulletScript>();
            if (bulletScript != null)
            {
                TakeDamage(bulletScript.damage);
            }
            else
            {
                TakeDamage(10);
            }
            
            Destroy(other.gameObject);
        }
    }

    void OnBecameInvisible()
    {
        BossEnemy bossComponent = GetComponent<BossEnemy>();
        if (bossComponent != null)
        {
            Debug.Log($"EnemyBehaviour: {gameObject.name} is a boss, not destroying on invisible");
            return;
        }
        
        if (transform.parent != null)
        {
            StraightRowFormation straightFormation = transform.parent.GetComponent<StraightRowFormation>();
            ZigZagFormation1 zigzagFormation = transform.parent.GetComponent<ZigZagFormation1>();
            CircleFormation circleFormation = transform.parent.GetComponent<CircleFormation>();
            GridFormation gridFormation = transform.parent.GetComponent<GridFormation>();
            
            if (straightFormation != null || zigzagFormation != null || circleFormation != null || gridFormation != null)
            {
                Debug.Log($"EnemyBehaviour: {gameObject.name} is part of formation, not destroying on invisible");
                return;
            }
        }
        
        Vector3 screenBottom = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        if (transform.position.y < screenBottom.y - 5f)
        {
            Debug.Log($"EnemyBehaviour: {gameObject.name} destroyed for being off-screen (y: {transform.position.y})");
            Destroy(gameObject);
        }
    }
    
    [Header("Power-Up Drops")]
    public bool canDropPowerUps = false;
    public GameObject invincibilityPowerUpPrefab;
    
    [Header("Drop Rates")]
    [Range(0f, 1f)]
    public float invincibilityDropChance = 0.30f;
    
    private void TryDropPowerUp()
    {
        if (!canDropPowerUps)
        {
            return;
        }
        
        float randomValue = Random.Range(0f, 1f);
        Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
        
        if (randomValue <= invincibilityDropChance)
        {
            if (invincibilityPowerUpPrefab != null)
            {
                Instantiate(invincibilityPowerUpPrefab, dropPosition, Quaternion.identity);
                Debug.Log("EnemyBehaviour: Dropped Invincibility Power-Up (Prefab)");
            }
            else
            {
                CreateInvincibilityPowerUpFallback(dropPosition);
            }
        }
    }
    
    private void CreateInvincibilityPowerUpFallback(Vector3 position)
    {
        GameObject powerUp = new GameObject("InvincibilityPowerUp_Fallback");
        powerUp.transform.position = position;
        powerUp.tag = "PowerUp";
        
        SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
        sr.color = Color.cyan;
        sr.sortingOrder = 5;
        
        Texture2D texture = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                texture.SetPixel(x, y, Color.cyan);
            }
        }
        texture.Apply();
        sr.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        
        CircleCollider2D col = powerUp.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        
        powerUp.AddComponent<SimplePowerUp>().powerUpType = "Invincibility";
        
        Debug.Log("EnemyBehaviour: Created fallback Invincibility Power-Up");
    }
    
    private void StartSpawnProtection()
    {
        isSpawnInvincible = true;
        spawnProtectionCoroutine = StartCoroutine(SpawnProtectionCoroutine());
        
        Invoke(nameof(ForceRemoveProtection), spawnInvincibilityDuration + 1f);
        
        Debug.Log($"Enemy {gameObject.name} spawn protection activated for {spawnInvincibilityDuration} seconds");
    }
    
    private void ForceRemoveProtection()
    {
        if (isSpawnInvincible)
        {
            Debug.Log($"Enemy {gameObject.name} FORCED protection removal (backup safety)");
            RemoveSpawnProtection();
        }
    }
    
    System.Collections.IEnumerator SpawnProtectionCoroutine()
    {
        float endTime = Time.time + spawnInvincibilityDuration;
        bool isFlashing = false;
        
        Debug.Log($"Enemy {gameObject.name} protection will end at time: {endTime}, current time: {Time.time}");
        
        while (Time.time < endTime)
        {
            if (spriteRenderer != null)
            {
                isFlashing = !isFlashing;
                if (isFlashing)
                {
                    spriteRenderer.color = new Color(originalColor.r * 0.7f, originalColor.g * 0.9f, originalColor.b * 1.2f, originalColor.a);
                }
                else
                {
                    spriteRenderer.color = originalColor;
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        isSpawnInvincible = false;
        Debug.Log($"Enemy {gameObject.name} spawn protection expired at time: {Time.time}");
    }
    
    public void RemoveSpawnProtection()
    {
        if (spawnProtectionCoroutine != null)
        {
            StopCoroutine(spawnProtectionCoroutine);
            spawnProtectionCoroutine = null;
        }
        
        CancelInvoke(nameof(ForceRemoveProtection));
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        isSpawnInvincible = false;
        Debug.Log($"Enemy {gameObject.name} spawn protection manually removed");
    }
}