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
        CheckAbilityConditions();
    }
    
    private void CheckAbilityConditions()
    {
        float healthRatio = (float)bossEnemy.CurrentHealth / bossData.maxHealth;
        
        if (bossData.hasShieldAbility && canUseShield && healthRatio <= 0.3f && !isShieldActive)
        {
            ActivateShield();
        }
        
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
        
        if (shieldEffect != null)
        {
            Instantiate(shieldEffect, transform.position, transform.rotation, transform);
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = shieldColor;
        }
        
        Debug.Log("Boss: Shield activated!");
        
        StartCoroutine(ShieldDuration());
    }
    
    private IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(bossData.shieldDuration);
        
        isShieldActive = false;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        Debug.Log("Boss: Shield deactivated!");
        
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
        
        Vector3 currentPos = transform.position;
        Vector3 newPos = GetRandomTeleportPosition();
        
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, currentPos, Quaternion.identity);
        }
        
        transform.position = newPos;
        
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, newPos, Quaternion.identity);
        }
        
        Debug.Log($"Boss: Teleported from {currentPos} to {newPos}");
        
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
    
    public bool ShouldBlockDamage()
    {
        return isShieldActive;
    }
} 