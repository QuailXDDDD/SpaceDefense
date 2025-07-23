using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyData enemyData;
    
    [Header("Effects")]
    public GameObject explosionPrefab;
    
    [Header("Spawn Protection")]
    public float spawnInvincibilityDuration = 5f;
    public bool hasSpawnProtection = true;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    protected bool isSpawnInvincible = false;
    protected Coroutine spawnProtectionCoroutine;
    private Color originalColor;

    public int CurrentHealth => currentHealth;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Enemy: SpriteRenderer component not found on " + gameObject.name, this);
        }
    }

    protected virtual void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError("Enemy: EnemyData Scriptable Object not assigned to " + gameObject.name, this);
            return;
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
        
        if (hasSpawnProtection)
        {
            StartSpawnProtection();
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (isSpawnInvincible)
        {
            Debug.Log($"Enemy {gameObject.name} is protected by spawn invincibility! Damage blocked.");
            return;
        }
        
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHit();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has been destroyed!");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyExplosion();
        }
        
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        
        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject != null && enemyData != null)
        {
            GameUI gameUI = canvasObject.GetComponent<GameUI>();
            if (gameUI != null)
            {
                gameUI.AddScore(enemyData.scoreValue);
            }
        }
        
        Destroy(gameObject);
    }
    
    protected void StartSpawnProtection()
    {
        if (this == null || gameObject == null)
        {
            Debug.LogError("StartSpawnProtection called on destroyed object!");
            return;
        }
        
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
                    float alpha = Mathf.Max(originalColor.a, 0.8f);
                    spriteRenderer.color = new Color(originalColor.r * 0.7f, originalColor.g * 0.9f, originalColor.b * 1.2f, alpha);
                }
                else
                {
                    float alpha = Mathf.Max(originalColor.a, 0.8f);
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
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
    
    protected void RemoveSpawnProtection()
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
