using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class BasicAttacker : MonoBehaviour
{
    private Enemy enemy;
    private GameObject projectilePrefab;
    private float fireRate;
    private int projectileDamage;
    private float projectileSpeed;

    public Transform firePoint; // Assign a child GameObject here

    private float nextFireTime;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (enemy != null && enemy.enemyData != null)
        {
            projectilePrefab = enemy.enemyData.projectilePrefab;
            fireRate = enemy.enemyData.baseFireRate;
            projectileDamage = enemy.enemyData.projectileDamage;
            projectileSpeed = enemy.enemyData.projectileSpeed;
        }
        else
        {
            Debug.LogError("BasicAttacker: Enemy or EnemyData not found!", this);
            enabled = false; // Disable script if requirements aren't met
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("BasicAttacker: FirePoint not assigned! Create an empty child GameObject.", this);
            enabled = false;
            return;
        }
    }

    void Start()
    {
        nextFireTime = Time.time + (1f / fireRate); // Initial delay based on fire rate
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Attack();
            nextFireTime = Time.time + (1f / fireRate);
        }
    }

    protected virtual void Attack()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            // Ensure the projectile script has its damage/speed set
            EnemyProjectile projectile = projectileGO.GetComponent<EnemyProjectile>();
            if (projectile != null)
            {
                projectile.damage = projectileDamage;
                projectile.speed = projectileSpeed;
            }
        }
        else
        {
            Debug.LogWarning("BasicAttacker: Projectile Prefab or Fire Point missing!", this);
        }
    }
}
