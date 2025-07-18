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

    // Public method to enable immediate shooting (called by formations)
    public void EnableImmediateShooting()
    {
        nextFireTime = Time.time; // Allow shooting immediately
    }

    void Update()
    {
        // Only fire if fully inside the camera's viewport
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        bool fullyOnScreen = viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

        if (fullyOnScreen && Time.time >= nextFireTime)
        {
            Attack();
            nextFireTime = Time.time + (1f / fireRate);
        }
    }

    protected virtual void Attack()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // Play shooting sound effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyShoot();
            }
            
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
