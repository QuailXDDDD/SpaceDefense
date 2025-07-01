using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 5f; // Public speed variable
    public int damage = 10; // Public damage variable
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

    //protected virtual void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        // Assuming your Player has a script with a TakeDamage method (e.g., PlayerHealth.cs)
    //        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
    //        if (playerHealth != null)
    //        {
    //            playerHealth.TakeDamage(damage);
    //        }
    //        Debug.Log($"Enemy projectile hit {other.name}! Deals {damage} damage.");
    //        Destroy(gameObject); // Destroy the projectile on impact
    //    }
    //}

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
