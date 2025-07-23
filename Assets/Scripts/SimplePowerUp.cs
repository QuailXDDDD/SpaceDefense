using UnityEngine;

public class SimplePowerUp : MonoBehaviour
{
    public string powerUpType = "Invincibility";
    public float fallSpeed = 2f;
    public float lifetime = 10f;
    
    private float timer = 0f;
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
        
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);
        
        timer += Time.deltaTime * 2f;
        float floatOffset = Mathf.Sin(timer) * 0.3f;
        
        Vector3 newPosition = transform.position;
        newPosition.x = startPosition.x + floatOffset;
        transform.position = newPosition;
        
        startPosition.y = newPosition.y;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerShip playerShip = other.GetComponent<PlayerShip>();
            if (playerShip != null)
            {
                playerShip.ActivateInvincibility();
                Debug.Log("SimplePowerUp: Applied Invincibility to player");
                
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPowerUp();
                }
                
                Destroy(gameObject);
            }
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