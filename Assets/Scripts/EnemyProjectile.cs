using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 5f; // Public speed variable
    public int damage = 25; // Public damage variable
    public float lifeTime = 3f; // Destroy after a few seconds to avoid clutter
    
    private Vector3 moveDirection = Vector3.down; // Default direction

    void Start()
    {
        Destroy(gameObject, lifeTime); // Self-destruct after lifeTime
    }

    void Update()
    {
        // Move in the specified direction
        transform.Translate(moveDirection * speed * Time.deltaTime);
    }
    
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        // Update rotation to face the direction
        if (direction != Vector3.zero)
        {
            // Calculate angle from Vector3.down (0, -1, 0) to the target direction
            float angle = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"EnemyProjectile hit: {other.name} with tag: {other.tag}");
        
        // Ignore player bullets - enemy projectiles should pass through them
        if (other.CompareTag("PlayerBullet"))
        {
            Debug.Log("Enemy projectile ignoring player bullet");
            return;
        }
        
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Enemy projectile hit player!");
            
            // Play projectile hit sound effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayProjectileHit();
            }
            
            // Note: PlayerShip's OnTriggerEnter2D will handle the damage
            // Don't call TakeDamage here to avoid double damage
            
            Destroy(gameObject); // Destroy the projectile on impact
        }
    }

    // OnBecameInvisible() is still a good fallback if lifeTime isn't enough or for edge cases
    void OnBecameInvisible()
    {
        // Check if the projectile is well below the screen before destroying
        if (transform.position.y < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f)
        {
            Destroy(gameObject);
        }
    }
}
