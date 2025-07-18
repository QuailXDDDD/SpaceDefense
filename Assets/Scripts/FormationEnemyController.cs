using UnityEngine;

public class FormationEnemyController : MonoBehaviour
{
    // Reference to the ScriptableObject holding this enemy's data
    public EnemyData enemyData;

    private int currentHealth;
    private float nextFireTime;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyData is not assigned to " + gameObject.name + "! Destroying enemy.", this);
            Destroy(gameObject);
            return;
        }

        // Initialize current health from the ScriptableObject's max health
        currentHealth = enemyData.maxHealth;

        // Apply visual properties from EnemyData
        if (spriteRenderer != null && enemyData.enemySprite != null)
        {
            spriteRenderer.sprite = enemyData.enemySprite;
        }
        transform.localScale = enemyData.scale;

        // Set the initial fire time (e.g., shoot immediately or after a short delay)
        nextFireTime = Time.time + enemyData.baseFireRate;
    }

    void Update()
    {
        // --- Shooting Logic Only ---
        // Formation enemies don't move individually, they only shoot
        // Check if it's time to shoot and if a projectile prefab is assigned
        if (Time.time >= nextFireTime && enemyData.projectilePrefab != null)
        {
            Shoot();
            // Set the next time the enemy can fire
            nextFireTime = Time.time + enemyData.baseFireRate;
        }
    }

    // Handles the enemy shooting a projectile
    void Shoot()
    {
        // Play shooting sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyShoot();
        }
        
        // Instantiate the projectile prefab at the enemy's position
        GameObject projectileGO = Instantiate(enemyData.projectilePrefab, transform.position, Quaternion.identity);

        // Get the projectile script and pass relevant data
        EnemyProjectile enemyProjectile = projectileGO.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            enemyProjectile.damage = enemyData.projectileDamage;
            enemyProjectile.speed = enemyData.projectileSpeed;
        }
    }

    // Public method to take damage, called by player bullets
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{enemyData.enemyName} took {damageAmount} damage. Current Health: {currentHealth}");
        
        // Play hit sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHit();
        }

        if (currentHealth <= 0)
        {
            // Play explosion sound effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyExplosion();
            }
            
            // Create explosion effect (try to find explosion prefab from Enemy component if available)
            Enemy enemyComponent = GetComponent<Enemy>();
            if (enemyComponent != null && enemyComponent.explosionPrefab != null)
            {
                Instantiate(enemyComponent.explosionPrefab, transform.position, Quaternion.identity);
            }
            
            Debug.Log($"{enemyData.enemyName} destroyed! Score added: {enemyData.scoreValue}");
            Destroy(gameObject); // Destroy the enemy GameObject
        }
    }

    // Handles collisions when 'Is Trigger' is checked on the Collider2D
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider belongs to a PlayerBullet
        if (other.CompareTag("PlayerBullet"))
        {
            // For now, just take a fixed amount of damage
            // You can implement PlayerBullet script later
            TakeDamage(10); // Default damage
            // Destroy the player bullet after it hits the enemy
            Destroy(other.gameObject);
        }
        // You might add logic for collision with Player here if not using physics collisions
        // else if (other.CompareTag("Player"))
        // {
        //      // Deal damage to player, destroy enemy, etc.
        // }
    }

    // Destroy enemy if it moves off-screen (for formation enemies, this might not be needed)
    void OnBecameInvisible()
    {
        // For formation enemies, we might want to handle this differently
        // or let the formation manager handle cleanup
        // For now, we'll keep it simple
        if (transform.position.y < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f)
        {
            Destroy(gameObject);
        }
    }
} 