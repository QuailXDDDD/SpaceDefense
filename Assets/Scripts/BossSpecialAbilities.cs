using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BossEnemy))]
public class BossSpecialAbilities : MonoBehaviour
{
    private BossEnemy bossEnemy;
    private BossEnemyData bossData;
    private SpriteRenderer spriteRenderer;
    
    [Header("Shield Ability")]
    public GameObject shieldEffect;
    public Color shieldColor = Color.cyan;
    
    [Header("Teleport Ability")]
    public GameObject teleportEffect;
    public float teleportRange = 3f;
    
    private bool isShieldActive = false;
    private bool canUseShield = true;
    private bool canUseTeleport = true;
    private Color originalColor;
    
    void Awake()
    {
        bossEnemy = GetComponent<BossEnemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (bossEnemy == null)
        {
            Debug.LogError("BossSpecialAbilities: BossEnemy component not found!", this);
            enabled = false;
            return;
        }
        
        if (bossEnemy.enemyData is BossEnemyData)
        {
            bossData = (BossEnemyData)bossEnemy.enemyData;
        }
        else
        {
            Debug.LogError("BossSpecialAbilities: EnemyData is not BossEnemyData!", this);
            enabled = false;
            return;
        }
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    void Start()
    {
        // Start ability cooldowns
        if (bossData.hasShieldAbility)
        {
            StartCoroutine(ShieldCooldown());
        }
        
        if (bossData.hasTeleportAbility)
        {
            StartCoroutine(TeleportCooldown());
        }
    }
    
    void Update()
    {
        // Check for ability activation based on health or other conditions
        CheckAbilityConditions();
    }
    
    private void CheckAbilityConditions()
    {
        float healthRatio = (float)bossEnemy.CurrentHealth / bossData.maxHealth;
        
        // Activate shield when health is low
        if (bossData.hasShieldAbility && canUseShield && healthRatio <= 0.3f && !isShieldActive)
        {
            ActivateShield();
        }
        
        // Teleport when health is very low
        if (bossData.hasTeleportAbility && canUseTeleport && healthRatio <= 0.2f)
        {
            Teleport();
        }
    }
    
    public void ActivateShield()
    {
        if (!canUseShield || isShieldActive) return;
        
        isShieldActive = true;
        canUseShield = false;
        
        // Visual effect
        if (shieldEffect != null)
        {
            Instantiate(shieldEffect, transform.position, transform.rotation, transform);
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = shieldColor;
        }
        
        Debug.Log("Boss: Shield activated!");
        
        // Shield duration
        StartCoroutine(ShieldDuration());
    }
    
    private IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(bossData.shieldDuration);
        
        // Deactivate shield
        isShieldActive = false;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        Debug.Log("Boss: Shield deactivated!");
        
        // Start cooldown
        StartCoroutine(ShieldCooldown());
    }
    
    private IEnumerator ShieldCooldown()
    {
        yield return new WaitForSeconds(bossData.shieldCooldown);
        canUseShield = true;
        Debug.Log("Boss: Shield ability ready!");
    }
    
    public void Teleport()
    {
        if (!canUseTeleport) return;
        
        canUseTeleport = false;
        
        // Calculate new position
        Vector3 currentPos = transform.position;
        Vector3 newPos = GetRandomTeleportPosition();
        
        // Teleport effect at current position
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, currentPos, Quaternion.identity);
        }
        
        // Move to new position
        transform.position = newPos;
        
        // Teleport effect at new position
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, newPos, Quaternion.identity);
        }
        
        Debug.Log($"Boss: Teleported from {currentPos} to {newPos}");
        
        // Start cooldown
        StartCoroutine(TeleportCooldown());
    }
    
    private Vector3 GetRandomTeleportPosition()
    {
        Vector3 currentPos = transform.position;
        Vector3 newPos;
        
        do
        {
            float randomX = Random.Range(-teleportRange, teleportRange);
            float randomY = Random.Range(-teleportRange, teleportRange);
            newPos = currentPos + new Vector3(randomX, randomY, 0);
            
            // Keep boss within screen bounds
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(newPos);
            if (viewportPos.x < 0.1f) newPos.x = currentPos.x - teleportRange;
            if (viewportPos.x > 0.9f) newPos.x = currentPos.x + teleportRange;
            if (viewportPos.y < 0.1f) newPos.y = currentPos.y - teleportRange;
            if (viewportPos.y > 0.9f) newPos.y = currentPos.y + teleportRange;
            
        } while (Vector3.Distance(currentPos, newPos) < teleportRange * 0.5f);
        
        return newPos;
    }
    
    private IEnumerator TeleportCooldown()
    {
        yield return new WaitForSeconds(bossData.teleportCooldown);
        canUseTeleport = true;
        Debug.Log("Boss: Teleport ability ready!");
    }
    
    public bool IsShieldActive()
    {
        return isShieldActive;
    }
    
    // Override the TakeDamage method to handle shield
    public bool ShouldBlockDamage()
    {
        return isShieldActive;
    }
} 