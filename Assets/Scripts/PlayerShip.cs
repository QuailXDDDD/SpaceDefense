using UnityEngine;

public class PlayerShip : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float screenBoundaryBuffer = 0.5f; // How far from screen edge the player can go
    
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public float bulletSpeed = 10f;
    public float bulletLifetime = 3f;
    
    [Header("Player Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("Effects")]
    public GameObject explosionPrefab; // Assign the Explosion_FX prefab here
    
    private float fireTimer = 0f;
    private float screenLeftEdge;
    private float screenRightEdge;
    private float screenTopEdge;
    private float screenBottomEdge;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        // Initialize health
        currentHealth = maxHealth;
        
        // Calculate screen boundaries
        CalculateScreenBoundaries();
        
        // Create fire point if it doesn't exist
        if (firePoint == null)
        {
            CreateFirePoint();
        }
    }

    void Update()
    {
        HandleInput();
        UpdateFireTimer();
    }

    void HandleInput()
    {
        // Movement input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Create movement vector
        Vector3 moveDirection = new Vector3(moveX, moveY, 0).normalized;
        
        // Apply movement
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        
        // Clamp position to screen boundaries
        ClampToScreen();
        
        // Shooting input
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            TryShoot();
        }
    }

    void CalculateScreenBoundaries()
    {
        // Use camera's orthographic size for accurate boundaries
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            float height = mainCam.orthographicSize;
            float width = height * mainCam.aspect;
            
            screenLeftEdge = -width + screenBoundaryBuffer;
            screenRightEdge = width - screenBoundaryBuffer;
            screenBottomEdge = -height + screenBoundaryBuffer;
            screenTopEdge = height - screenBoundaryBuffer;
        }
        else
        {
            // Fallback if no main camera
            Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
            screenLeftEdge = -screenBounds.x + screenBoundaryBuffer;
            screenRightEdge = screenBounds.x - screenBoundaryBuffer;
            screenTopEdge = screenBounds.y - screenBoundaryBuffer;
            screenBottomEdge = -screenBounds.y + screenBoundaryBuffer;
        }
    }

    void ClampToScreen()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, screenLeftEdge, screenRightEdge);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, screenBottomEdge, screenTopEdge);
        transform.position = clampedPosition;
    }

    void CreateFirePoint()
    {
        // Create a fire point as a child of the player
        GameObject firePointObj = new GameObject("FirePoint");
        firePointObj.transform.SetParent(transform);
        firePointObj.transform.localPosition = new Vector3(0, 0.5f, 0); // Above the player
        firePoint = firePointObj.transform;
        
        Debug.Log("PlayerShip: Created fire point at " + firePoint.position);
    }

    void UpdateFireTimer()
    {
        fireTimer += Time.deltaTime;
    }

    void TryShoot()
    {
        if (fireTimer >= fireRate && bulletPrefab != null)
        {
            Shoot();
            fireTimer = 0f;
        }
    }

    void Shoot()
    {
        if (firePoint == null)
        {
            Debug.LogError("PlayerShip: Fire point is null! Cannot shoot.");
            return;
        }

        // Play shooting sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerShoot();
        }
        
        // Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        // Set bullet properties
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        if (bulletScript != null)
        {
            bulletScript.speed = bulletSpeed;
            bulletScript.lifetime = bulletLifetime;
        }
        else
        {
            // If no PlayerBullet script, add basic movement
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = Vector2.up * bulletSpeed;
            }
            
            // Destroy bullet after lifetime
            Destroy(bullet, bulletLifetime);
        }
        
        Debug.Log("PlayerShip: Fired bullet!");
    }

    public void TakeDamage(int damage)
    {
        // Use the damage parameter passed in
        currentHealth -= damage;
        int heartsLost = damage / 25; // Calculate how many hearts were lost
        Debug.Log($"PlayerShip: Took {damage} damage! Lost {heartsLost} heart(s). Health: {currentHealth}/{maxHealth}");
        
        // Play hit sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHit();
        }
        
        // Visual feedback - flash red when taking damage
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRed());
        }
        
        if (currentHealth <= 0)
        {
            Debug.Log($"PlayerShip: Health reached 0! Destroying player...");
            Die();
        }
    }
    
    System.Collections.IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        Debug.Log("PlayerShip: Die() method called! Player destroyed! Game Over!");
        
        // Play explosion sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerExplosion();
        }
        
        // Create explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        
        // Disable player movement and shooting
        enabled = false;
        
        // Change sprite to indicate destruction
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }
        
        // Add game over logic here
        // For now, just destroy the player after a delay
        Destroy(gameObject, 2f);
    }
    
    // Public method to get current health percentage
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    // Public method to check if player is alive
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"PlayerShip hit by: {other.name} with tag: {other.tag}");
        
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet") || other.CompareTag("EnemyProjectile"))
        {
            // Always take exactly 1 heart of damage (25 health) regardless of projectile damage
            TakeDamage(25);
            
            // Destroy the enemy projectile
            if (other.CompareTag("EnemyBullet") || other.CompareTag("EnemyProjectile"))
            {
                Destroy(other.gameObject);
            }
        }
        else
        {
            Debug.Log($"PlayerShip collided with: {other.name} (tag: {other.tag}) - No damage taken");
        }
    }
}
