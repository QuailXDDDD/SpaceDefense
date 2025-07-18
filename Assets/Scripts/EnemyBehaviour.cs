using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    // Reference to the ScriptableObject holding this enemy's data
    public EnemyData enemyData;
    
    [Header("Effects")]
    public GameObject explosionPrefab; // Assign explosion effect prefab here

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
            Debug.LogWarning("EnemyData is not assigned to " + gameObject.name + "! Trying to find one...", this);
            
            // Try to find EnemyData from other enemy components
            Enemy enemyComponent = GetComponent<Enemy>();
            if (enemyComponent != null && enemyComponent.enemyData != null)
            {
                enemyData = enemyComponent.enemyData;
                Debug.Log("Found EnemyData from Enemy component!");
            }
            else
            {
                // Last resort - load a basic enemy data asset
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

    // Public method to enable immediate shooting (called by formations)
    public void EnableImmediateShooting()
    {
        nextFireTime = Time.time; // Allow shooting immediately
    }
    
    void Update()
    {
        // --- Movement Logic ---
        // Only move if not part of a formation (formation handles movement)
        bool isPartOfFormation = transform.parent != null && 
                                (transform.parent.GetComponent<ZigZagFormation1>() != null || 
                                 transform.parent.GetComponent<SkullFormation>() != null);
        
        if (!isPartOfFormation)
        {
            // Move the enemy straight down
            transform.Translate(Vector2.down * enemyData.moveSpeed * Time.deltaTime);
        }

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
        // Play shooting sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyShoot();
        }
        
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
            
            // Create explosion effect
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
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
            // Get the BulletScript to retrieve its damage amount
            BulletScript bulletScript = other.GetComponent<BulletScript>();
            if (bulletScript != null)
            {
                TakeDamage(bulletScript.damage);
            }
            else
            {
                // Default damage if no script found
                TakeDamage(10);
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

    // Destroy enemy if it moves off-screen
    void OnBecameInvisible()
    {
        // Don't destroy if this enemy is part of a formation that's still entering the screen
        if (transform.parent != null)
        {
            StraightRowFormation straightFormation = transform.parent.GetComponent<StraightRowFormation>();
            ZigZagFormation1 zigzagFormation = transform.parent.GetComponent<ZigZagFormation1>();
            CircleFormation circleFormation = transform.parent.GetComponent<CircleFormation>();
            
            // If part of any formation, let the formation handle destruction
            if (straightFormation != null || zigzagFormation != null || circleFormation != null)
            {
                Debug.Log($"EnemyBehaviour: {gameObject.name} is part of formation, not destroying on invisible");
                return;
            }
        }
        
        // Only destroy if enemy is significantly below the screen (not just off the top/sides)
        Vector3 screenBottom = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        if (transform.position.y < screenBottom.y - 5f)
        {
            Debug.Log($"EnemyBehaviour: {gameObject.name} destroyed for being off-screen (y: {transform.position.y})");
            Destroy(gameObject);
        }
    }
}