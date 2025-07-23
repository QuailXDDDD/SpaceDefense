using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 25;
    public float lifeTime = 3f;
    
    private Vector3 moveDirection = Vector3.down;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime);
    }
    
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"EnemyProjectile hit: {other.name} with tag: {other.tag}");
        
        if (other.CompareTag("PlayerBullet"))
        {
            Debug.Log("Enemy projectile ignoring player bullet");
            return;
        }
        
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Enemy projectile hit player!");
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayProjectileHit();
            }
            
            Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        if (transform.position.y < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f)
        {
            Destroy(gameObject);
        }
    }
}
