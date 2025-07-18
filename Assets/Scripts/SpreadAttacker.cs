using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class SpreadAttacker : MonoBehaviour
{
    private Enemy enemy;
    private GameObject projectilePrefab;
    private int projectileDamage;
    private float projectileSpeed;

    public Transform firePoint;
    public int numberOfProjectiles = 3; // How many projectiles in the spread
    public float spreadAngle = 30f; // Total angle of the spread (e.g., 30 degrees)
    public float fireRate = 2f; // Cooldown before next spread shot

    private float nextFireTime;

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
            Debug.LogError("SpreadAttacker: Enemy or EnemyData not found!", this);
            enabled = false;
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("SpreadAttacker: FirePoint not assigned!", this);
            enabled = false;
            return;
        }
    }

    void Start()
    {
        nextFireTime = Time.time + (1f / fireRate); // Initial delay
    }

    // Public method to enable immediate shooting (called by formations)
    public void EnableImmediateShooting()
    {
        nextFireTime = Time.time; // Allow shooting immediately
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            FireSpread();
            nextFireTime = Time.time + (1f / fireRate);
        }
    }

    void FireSpread()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("SpreadAttacker: Projectile Prefab or Fire Point missing!", this);
            return;
        }

        // Play shooting sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyShoot();
        }

        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadAngle / (numberOfProjectiles - 1);

        if (numberOfProjectiles == 1) // Handle single projectile case to avoid division by zero
        {
            InstantiateAndSetProjectile(firePoint.position, firePoint.rotation);
            return;
        }

        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle) * firePoint.rotation;
            InstantiateAndSetProjectile(firePoint.position, rotation);
        }
    }

    void InstantiateAndSetProjectile(Vector3 position, Quaternion rotation)
    {
        GameObject projectileGO = Instantiate(projectilePrefab, position, rotation);
        EnemyProjectile projectile = projectileGO.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.damage = projectileDamage;
            projectile.speed = projectileSpeed;
        }
    }
}
