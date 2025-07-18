using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    public float lifetime = 3f;
    
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            // Set gravity to 0 to prevent falling
            rb.gravityScale = 0f;
            // Set velocity to move upward
            rb.linearVelocity = Vector2.up * speed;
        }
        
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rb == null)
        {
            transform.Translate(Vector2.up * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Bullet hit: {other.name} with tag: {other.tag}");
        
        // Ignore enemy projectiles - player bullets should pass through them
        if (other.CompareTag("EnemyProjectile") || other.CompareTag("EnemyBullet"))
        {
            Debug.Log("Player bullet ignoring enemy projectile");
            return;
        }
        
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Bullet hit enemy!");
            
            // Play projectile hit sound effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayProjectileHit();
            }
            
            // Try different enemy types
            FormationEnemyController formationEnemy = other.GetComponent<FormationEnemyController>();
            if (formationEnemy != null)
            {
                formationEnemy.TakeDamage(damage);
                Debug.Log($"Damaged FormationEnemyController: {damage} damage");
            }
            else
            {
                // Try EnemyBehaviour (now has TakeDamage method)
                EnemyBehaviour enemyBehaviour = other.GetComponent<EnemyBehaviour>();
                if (enemyBehaviour != null)
                {
                    enemyBehaviour.TakeDamage(damage);
                    Debug.Log($"Damaged EnemyBehaviour: {damage} damage");
                }
                else
                {
                    // Try base Enemy class
                    Enemy enemyBase = other.GetComponent<Enemy>();
                    if (enemyBase != null)
                    {
                        enemyBase.TakeDamage(damage);
                        Debug.Log($"Damaged Enemy: {damage} damage");
                    }
                    else
                    {
                        // If no script found, just destroy the enemy
                        Debug.Log("No enemy script found, destroying enemy directly");
                        Destroy(other.gameObject);
                    }
                }
            }
            
            // Destroy the bullet
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"Bullet hit something with tag: {other.tag}");
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
