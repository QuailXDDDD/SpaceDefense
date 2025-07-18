using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyData enemyData; // Assign the Scriptable Object here in the Inspector
    
    [Header("Effects")]
    public GameObject explosionPrefab; // Assign the Explosion_FX prefab here

    private int currentHealth;
    private SpriteRenderer spriteRenderer; // To display the enemy's sprite

    public int CurrentHealth => currentHealth; // Public getter for health

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
        transform.localScale = enemyData.scale; // Apply scale from data
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}");
        
        // Play hit sound effect
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
        
        // Play explosion sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyExplosion();
        }
        
        // Create explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        
        // TODO: Add score, etc.
        Destroy(gameObject);
    }
}
