using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class SpreadAttacker : MonoBehaviour
{
    private Enemy enemy;
    private GameObject projectilePrefab;
    private int projectileDamage;
    private float projectileSpeed;

    public Transform firePoint;
    public int numberOfProjectiles = 3;
    public float spreadAngle = 30f;
    public float fireRate = 2f;

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
        nextFireTime = Time.time + (1f / fireRate);
    }

    public void EnableImmediateShooting()
    {
        nextFireTime = Time.time;
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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyShoot();
        }

        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadAngle / (numberOfProjectiles - 1);

        if (numberOfProjectiles == 1)
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
