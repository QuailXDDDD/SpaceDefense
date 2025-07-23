using UnityEngine;

public class InvincibilityPowerUp : MonoBehaviour
{
    [Header("Invincibility Power-Up")]
    public Sprite invincibilitySprite;
    public float fallSpeed = 2f;
    public float lifetime = 10f;
    
    private float timer = 0f;
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }
        
        if (invincibilitySprite != null)
        {
            sr.sprite = invincibilitySprite;
        }
        else
        {
            sr.color = Color.cyan;
            CreateDefaultInvincibilitySprite(sr);
        }
        
        Collider2D powerUpCollider = GetComponent<Collider2D>();
        if (powerUpCollider == null)
        {
            powerUpCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        powerUpCollider.isTrigger = true;
        
        gameObject.tag = "PowerUp";
        
        Destroy(gameObject, lifetime);
        
        Debug.Log("InvincibilityPowerUp: Created invincibility power-up");
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
                Debug.Log("InvincibilityPowerUp: Applying invincibility to player for 5 seconds");
                playerShip.ActivateInvincibility();
                
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
    
    private void CreateDefaultInvincibilitySprite(SpriteRenderer sr)
    {
        Texture2D texture = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float centerX = 16f;
                float centerY = 16f;
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                
                Vector2 fromCenter = new Vector2(x - centerX, y - centerY);
                float angle = Mathf.Atan2(fromCenter.y, fromCenter.x);
                float starRadius = 12f + 3f * Mathf.Sin(angle * 5f);
                
                if (distance < starRadius && distance > starRadius - 4f)
                {
                    texture.SetPixel(x, y, Color.cyan);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        sr.sprite = sprite;
    }
} 