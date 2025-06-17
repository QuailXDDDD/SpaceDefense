using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;
    public float lifeTime = 3f; // Destroy after a few seconds to avoid clutter

    void Start()
    {
        Destroy(gameObject, lifeTime); // Self-destruct after lifeTime
    }

    void Update()
    {
        // Move in its forward direction (which will be 'down' if spawned with correct rotation)
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Enemy projectile hit {other.name}! Deals {damage} damage.");
            Destroy(gameObject); // Destroy the projectile on impact
        }
    }
}
