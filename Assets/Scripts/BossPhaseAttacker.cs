using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BossEnemy))]
public class BossPhaseAttacker : MonoBehaviour
{
    private BossEnemy bossEnemy;
    private BossEnemyData bossData;
    private SpriteRenderer spriteRenderer;
    
    [Header("Fire Points")]
    public Transform[] firePoints;
    
    private float nextFireTime;
    private int currentPhase = 1;
    
    void Awake()
    {
        bossEnemy = GetComponent<BossEnemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (bossEnemy == null)
        {
            Debug.LogError("BossPhaseAttacker: BossEnemy component not found!", this);
            enabled = false;
            return;
        }
        
        if (bossEnemy.enemyData is BossEnemyData)
        {
            bossData = (BossEnemyData)bossEnemy.enemyData;
        }
        else
        {
            Debug.LogError("BossPhaseAttacker: EnemyData is not BossEnemyData!", this);
            enabled = false;
            return;
        }
    }
    
    void Start()
    {
        if (firePoints == null || firePoints.Length == 0)
        {
            CreateDefaultFirePoints();
        }
        
        UpdatePhase(1);
    }
    
    private void CreateDefaultFirePoints()
    {
        Debug.Log("BossPhaseAttacker: Creating default fire points...");
        
        firePoints = new Transform[3];
        
        GameObject centerFirePoint = new GameObject("FirePoint_Center");
        centerFirePoint.transform.SetParent(transform);
        centerFirePoint.transform.localPosition = new Vector3(0, -0.5f, 0);
        centerFirePoint.transform.localRotation = Quaternion.Euler(0, 0, 0);
        firePoints[0] = centerFirePoint.transform;
        
        GameObject leftFirePoint = new GameObject("FirePoint_Left");
        leftFirePoint.transform.SetParent(transform);
        leftFirePoint.transform.localPosition = new Vector3(-0.8f, -0.3f, 0);
        leftFirePoint.transform.localRotation = Quaternion.Euler(0, 0, 0);
        firePoints[1] = leftFirePoint.transform;
        
        GameObject rightFirePoint = new GameObject("FirePoint_Right");
        rightFirePoint.transform.SetParent(transform);
        rightFirePoint.transform.localPosition = new Vector3(0.8f, -0.3f, 0);
        rightFirePoint.transform.localRotation = Quaternion.Euler(0, 0, 0);
        firePoints[2] = rightFirePoint.transform;
        
        Debug.Log("BossPhaseAttacker: Created 3 default fire points (Center, Left, Right)");
    }
    
    public void EnableImmediateShooting()
    {
        nextFireTime = Time.time;
    }
    
    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Attack();
            UpdateFireRate();
        }
    }
    
    public void UpdatePhase(int newPhase)
    {
        currentPhase = newPhase;
        UpdateFireRate();
        UpdateVisuals();
        
        Debug.Log($"Boss Phase Attacker: Switched to Phase {currentPhase}");
    }
    
    private void UpdateFireRate()
    {
        float fireRate = GetCurrentPhaseFireRate();
        nextFireTime = Time.time + (1f / fireRate);
    }
    
    private float GetCurrentPhaseFireRate()
    {
        switch (currentPhase)
        {
            case 1: return bossData.phase1FireRate;
            case 2: return bossData.phase2FireRate;
            case 3: return bossData.phase3FireRate;
            default: return bossData.phase1FireRate;
        }
    }
    
    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            switch (currentPhase)
            {
                case 1:
                    spriteRenderer.color = bossData.phase1Color;
                    break;
                case 2:
                    spriteRenderer.color = bossData.phase2Color;
                    break;
                case 3:
                    spriteRenderer.color = bossData.phase3Color;
                    break;
            }
        }
    }
    
    private void Attack()
    {
        switch (currentPhase)
        {
            case 1:
                Phase1Attack();
                break;
            case 2:
                Phase2Attack();
                break;
            case 3:
                Phase3Attack();
                break;
        }
    }
    
    private void Phase1Attack()
    {
        if (firePoints.Length > 0 && firePoints[0] != null)
        {
            FireProjectile(firePoints[0], bossData.phase1ProjectilePrefab, 
                          bossData.phase1ProjectileDamage, bossData.phase1ProjectileSpeed);
        }
    }
    
    private void Phase2Attack()
    {
        StartCoroutine(BurstAttack());
    }
    
    private void Phase3Attack()
    {
        SpreadAttack();
    }
    
    private IEnumerator BurstAttack()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossShoot();
        }
        
        for (int burst = 0; burst < bossData.phase2BurstCount; burst++)
        {
            foreach (Transform firePoint in firePoints)
            {
                if (firePoint != null)
                {
                    GameObject projectile = Instantiate(bossData.phase2ProjectilePrefab, 
                                                      firePoint.position, Quaternion.identity);
                    
                    EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
                    if (projectileScript != null)
                    {
                        projectileScript.damage = bossData.phase2ProjectileDamage;
                        projectileScript.speed = bossData.phase2ProjectileSpeed;
                        projectileScript.SetDirection(Vector3.down);
                    }
                }
            }
            
            if (burst < bossData.phase2BurstCount - 1)
            {
                yield return new WaitForSeconds(bossData.phase2BurstDelay);
            }
        }
    }
    
    private void SpreadAttack()
    {
        if (firePoints.Length == 0) return;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossShoot();
        }
        
        float angleStep = bossData.phase3SpreadAngle / (bossData.phase3SpreadCount - 1);
        float startAngle = -bossData.phase3SpreadAngle / 2f;
        
        for (int i = 0; i < bossData.phase3SpreadCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.down;
            
            if (firePoints[0] != null)
            {
                GameObject projectile = Instantiate(bossData.phase3ProjectilePrefab, 
                                                  firePoints[0].position, Quaternion.identity);
                
                EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
                if (projectileScript != null)
                {
                    projectileScript.damage = bossData.phase3ProjectileDamage;
                    projectileScript.speed = bossData.phase3ProjectileSpeed;
                    projectileScript.SetDirection(direction);
                    
                    Debug.Log($"Boss Phase 3: Firing projectile {i} with direction {direction} (angle: {angle}°)");
                }
            }
        }
    }
    
    private void FireProjectile(Transform firePoint, GameObject projectilePrefab, int damage, float speed)
    {
        if (projectilePrefab != null && firePoint != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBossShoot();
            }
            
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
            
            if (projectileScript != null)
            {
                projectileScript.damage = damage;
                projectileScript.speed = speed;
                projectileScript.SetDirection(Vector3.down);
            }
        }
    }
} 