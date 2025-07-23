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
            rb.gravityScale = 0f;
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
        
        if (other.CompareTag("EnemyProjectile") || other.CompareTag("EnemyBullet"))
        {
            Debug.Log("Player bullet ignoring enemy projectile");
            return;
        }
        
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Bullet hit enemy!");
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayProjectileHit();
            }
            
            FormationEnemyController formationEnemy = other.GetComponent<FormationEnemyController>();
            if (formationEnemy != null)
            {
                formationEnemy.TakeDamage(damage);
                Debug.Log($"Damaged FormationEnemyController: {damage} damage");
            }
            else
            {
                EnemyBehaviour enemyBehaviour = other.GetComponent<EnemyBehaviour>();
                if (enemyBehaviour != null)
                {
                    enemyBehaviour.TakeDamage(damage);
                    Debug.Log($"Damaged EnemyBehaviour: {damage} damage");
                }
                else
                {
                    Enemy enemyBase = other.GetComponent<Enemy>();
                    if (enemyBase != null)
                    {
                        enemyBase.TakeDamage(damage);
                        Debug.Log($"Damaged Enemy: {damage} damage");
                    }
                    else
                    {
                        Debug.Log("No enemy script found, destroying enemy directly");
                        Destroy(other.gameObject);
                    }
                }
            }
            
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
