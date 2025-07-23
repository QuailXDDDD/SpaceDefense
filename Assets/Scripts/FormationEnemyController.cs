using UnityEngine;

public class FormationEnemyController : MonoBehaviour
{
    public EnemyData enemyData;

    private int currentHealth;
    private float nextFireTime;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
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

        currentHealth = enemyData.maxHealth;

        if (spriteRenderer != null && enemyData.enemySprite != null)
        {
            spriteRenderer.sprite = enemyData.enemySprite;
        }
        transform.localScale = enemyData.scale;

        nextFireTime = Time.time + enemyData.baseFireRate;
    }

    void Update()
    {
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
            
            Enemy enemyComponent = GetComponent<Enemy>();
            if (enemyComponent != null && enemyComponent.explosionPrefab != null)
            {
                Instantiate(enemyComponent.explosionPrefab, transform.position, Quaternion.identity);
            }
            
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
            TakeDamage(10);
            Destroy(other.gameObject);
        }
    }

    void OnBecameInvisible()
    {
        BossEnemy bossComponent = GetComponent<BossEnemy>();
        if (bossComponent != null)
        {
            Debug.Log($"FormationEnemyController: {gameObject.name} is a boss, not destroying on invisible");
            return;
        }
        
        if (transform.position.y < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f)
        {
            Destroy(gameObject);
        }
    }
} 