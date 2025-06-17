using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyData enemyData; // Assign the Scriptable Object here in the Inspector

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

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has been destroyed!");
        // TODO: Add explosion effects, score, etc.
        Destroy(gameObject);
    }
}
