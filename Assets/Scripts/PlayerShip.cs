using UnityEngine;

public class PlayerShip : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float screenBoundaryBuffer = 0.5f;
    
    [Header("Entrance Settings")]
    public float entranceSpeed = 3f;
    public Vector3 entranceStartOffset = new Vector3(0, -8f, 0);
    public Vector3 entranceTargetPosition = new Vector3(0, -3f, 0);
    public bool playerControlEnabled = false;
    
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public float bulletSpeed = 10f;
    public float bulletLifetime = 3f;
    
    [Header("Player Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("Power-Up States")]
    public bool isInvincible = false;
    public float invincibilityDuration = 5f;
    
    [Header("Effects")]
    public GameObject explosionPrefab;
    
    [Header("Invincibility Visual")]
    public Color invincibilityColor = Color.cyan;
    public float flashInterval = 0.1f;
    
    private float fireTimer = 0f;
    private float screenLeftEdge;
    private float screenRightEdge;
    private float screenTopEdge;
    private float screenBottomEdge;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    
    private Coroutine invincibilityCoroutine;
    private Color originalColor;
    
    private bool entranceComplete = false;
    private bool entranceInProgress = false;
    
    public static System.Action OnPlayerReady;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        currentHealth = maxHealth;
        
        CalculateScreenBoundaries();
        
        if (firePoint == null)
        {
            CreateFirePoint();
        }
        
        StartEntranceSequence();
    }

    void Update()
    {
        if (entranceInProgress)
        {
            HandleEntranceMovement();
        }
        else if (playerControlEnabled)
        {
            HandleInput();
        }
        
        UpdateFireTimer();
    }
    
    void StartEntranceSequence()
    {
        Vector3 startPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        startPosition.z = 0;
        startPosition += entranceStartOffset;
        transform.position = startPosition;
        
        playerControlEnabled = false;
        entranceInProgress = true;
        entranceComplete = false;
        
        Debug.Log("PlayerShip: Starting entrance sequence from " + startPosition);
    }
    
    void HandleEntranceMovement()
    {
        Vector3 targetWorldPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        targetWorldPosition.z = 0;
        targetWorldPosition += entranceTargetPosition;
        
        Vector3 direction = (targetWorldPosition - transform.position).normalized;
        transform.Translate(direction * entranceSpeed * Time.deltaTime, Space.World);
        
        float distanceToTarget = Vector3.Distance(transform.position, targetWorldPosition);
        if (distanceToTarget <= 0.1f)
        {
            transform.position = targetWorldPosition;
            CompleteEntranceSequence();
        }
    }
    
    void CompleteEntranceSequence()
    {
        entranceInProgress = false;
        entranceComplete = true;
        playerControlEnabled = true;
        
        Debug.Log("PlayerShip: Entrance sequence complete! Player can now take control.");
        
        OnPlayerReady?.Invoke();
    }

    void HandleInput()
    {
        if (!playerControlEnabled || !entranceComplete)
        {
            return;
        }
        
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(moveX, moveY, 0).normalized;
        
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        
        ClampToScreen();
        
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            TryShoot();
        }
    }

    void CalculateScreenBoundaries()
    {
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
        GameObject firePointObj = new GameObject("FirePoint");
        firePointObj.transform.SetParent(transform);
        firePointObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        firePoint = firePointObj.transform;
        
        Debug.Log("PlayerShip: Created fire point at " + firePoint.position);
    }

    void UpdateFireTimer()
    {
        fireTimer += Time.deltaTime;
    }

    void TryShoot()
    {
        if (!playerControlEnabled || !entranceComplete)
        {
            return;
        }
        
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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerShoot();
        }
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        if (bulletScript != null)
        {
            bulletScript.speed = bulletSpeed;
            bulletScript.lifetime = bulletLifetime;
        }
        else
        {
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = Vector2.up * bulletSpeed;
            }
            
            Destroy(bullet, bulletLifetime);
        }
        
        Debug.Log("PlayerShip: Fired bullet!");
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible)
        {
            Debug.Log("PlayerShip: Damage blocked by invincibility!");
            return;
        }
        
        currentHealth -= damage;
        int heartsLost = damage / 25;
        Debug.Log($"PlayerShip: Took {damage} damage! Lost {heartsLost} heart(s). Health: {currentHealth}/{maxHealth}");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHit();
        }
        
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
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerExplosion();
        }
        
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        
        enabled = false;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }
        
        GameOverManager.PlayerDied();
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    public bool IsPlayerReady()
    {
        return entranceComplete && playerControlEnabled;
    }
    
    [ContextMenu("Restart Entrance Sequence")]
    public void RestartEntranceSequence()
    {
        StartEntranceSequence();
    }
    
    [ContextMenu("Complete Entrance Immediately")]
    public void CompleteEntranceImmediately()
    {
        if (entranceInProgress)
        {
            CompleteEntranceSequence();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"PlayerShip hit by: {other.name} with tag: {other.tag}");
        
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet") || other.CompareTag("EnemyProjectile"))
        {
            TakeDamage(25);
            
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
    
    // Power-Up Management Methods
    public void ActivateInvincibility()
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }
        
        invincibilityCoroutine = StartCoroutine(InvincibilityCoroutine());
    }
    
    System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        Debug.Log("PlayerShip: Invincibility activated for " + invincibilityDuration + " seconds!");
        
        float elapsed = 0f;
        bool isFlashing = false;
        
        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isFlashing ? invincibilityColor : originalColor;
                isFlashing = !isFlashing;
            }
            
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        
        isInvincible = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        Debug.Log("PlayerShip: Invincibility ended!");
        invincibilityCoroutine = null;
    }
}
