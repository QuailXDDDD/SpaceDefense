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
    public int burstCount = 3;
    public float burstDelay = 0.1f;
    public float cooldownAfterBurst = 2f;

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
        nextBurstTime = Time.time + cooldownAfterBurst;
    }

    public void EnableImmediateShooting()
    {
        nextBurstTime = Time.time;
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
