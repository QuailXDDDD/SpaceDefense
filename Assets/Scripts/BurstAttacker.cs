using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Enemy))]
public class BurstAttacker : MonoBehaviour
{
    private Enemy enemy;
    private GameObject projectilePrefab;
    private int projectileDamage;
    private float projectileSpeed;

    public Transform firePoint;
    public int burstCount = 3; // How many projectiles in a burst
    public float burstDelay = 0.1f; // Delay between projectiles in a burst
    public float cooldownAfterBurst = 2f; // Cooldown before next burst sequence

    private float nextBurstTime;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (enemy != null && enemy.enemyData != null)
        {
            projectilePrefab = enemy.enemyData.projectilePrefab;
            projectileDamage = enemy.enemyData.projectileDamage;
            projectileSpeed = enemy.enemyData.projectileSpeed;
        }
        else
        {
            Debug.LogError("BurstAttacker: Enemy or EnemyData not found!", this);
            enabled = false;
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("BurstAttacker: FirePoint not assigned!", this);
            enabled = false;
            return;
        }
    }

    void Start()
    {
        nextBurstTime = Time.time + cooldownAfterBurst; // Initial delay
    }

    // Public method to enable immediate shooting (called by formations)
    public void EnableImmediateShooting()
    {
        nextBurstTime = Time.time; // Allow shooting immediately
    }

    void Update()
    {
        if (Time.time >= nextBurstTime)
        {
            StartCoroutine(FireBurst());
            nextBurstTime = Time.time + cooldownAfterBurst;
        }
    }

    IEnumerator FireBurst()
    {
        // Play shooting sound effect once for the burst
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyShoot();
        }
        
        for (int i = 0; i < burstCount; i++)
        {
            if (projectilePrefab != null && firePoint != null)
            {
                GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                EnemyProjectile projectile = projectileGO.GetComponent<EnemyProjectile>();
                if (projectile != null)
                {
                    projectile.damage = projectileDamage;
                    projectile.speed = projectileSpeed;
                }
            }
            else
            {
                Debug.LogWarning("BurstAttacker: Projectile Prefab or Fire Point missing!", this);
                break;
            }
            yield return new WaitForSeconds(burstDelay);
        }
    }
}
