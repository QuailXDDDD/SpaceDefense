using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
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
        // --- Movement Logic ---
        // Move the enemy straight down
        // The enemyData.moveSpeed is now set to 0, so this line will effectively do nothing.
        transform.Translate(Vector2.down * enemyData.moveSpeed * Time.deltaTime);

        // --- Shooting Logic ---
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
        // Instantiate the projectile prefab at the enemy's position
        GameObject projectileGO = Instantiate(enemyData.projectilePrefab, transform.position, Quaternion.identity);

        // Get the projectile script (assuming it has one, e.g., EnemyProjectile.cs)
        // And pass relevant data if needed, like damage and speed
        EnemyProjectile enemyProjectile = projectileGO.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            // Directly set properties since EnemyProjectile no longer has SetProjectileProperties
            enemyProjectile.damage = enemyData.projectileDamage;
            enemyProjectile.speed = enemyData.projectileSpeed;
        }
    }

    // --- Damage and Destruction Logic (Commented Out for now) ---
    /*
    // Public method to take damage, called by player bullets
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{enemyData.enemyName} took {damageAmount} damage. Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            // Optional: Add score to player, play explosion effect, etc.
            Debug.Log($"{enemyData.enemyName} destroyed! Score added: {enemyData.scoreValue}");
            Destroy(gameObject); // Destroy the enemy GameObject
        }
    }
    */

    // Handles collisions when 'Is Trigger' is checked on the Collider2D (Commented Out for now)
    /*
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider belongs to a PlayerBullet
        if (other.CompareTag("PlayerBullet"))
        {
            // Get the PlayerBullet script to retrieve its damage amount
            PlayerBullet playerBullet = other.GetComponent<PlayerBullet>();
            if (playerBullet != null)
            {
                TakeDamage(playerBullet.damage); // This line refers to the TakeDamage method above
            }
            // Destroy the player bullet after it hits the enemy
            Destroy(other.gameObject);
        }
        // You might add logic for collision with Player here if not using physics collisions
        // else if (other.CompareTag("Player"))
        // {
        //      // Deal damage to player, destroy enemy, etc.
        // }
    }
    */

    // Destroy enemy if it moves off-screen
    void OnBecameInvisible()
    {
        // Check if the enemy is well below the screen before destroying
        // Adjust -10f based on your camera's bottom edge and enemy size
        if (transform.position.y < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f)
        {
            Destroy(gameObject);
        }
    }
}