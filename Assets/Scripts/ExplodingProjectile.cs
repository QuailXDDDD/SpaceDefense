using UnityEngine;
using System.Collections;

public class ExplodingProjectile : EnemyProjectile
{
    [Header("Explosion Properties")]
    public GameObject explosionEffectPrefab;
    public float explosionRadius = 2f;
    public int explosionDamage = 20;

    [Header("Split Projectile Properties")]
    public GameObject smallProjectilePrefab;
    public int numberOfSplitProjectiles = 4;
    public float splitProjectileSpeed = 7f;
    public int splitProjectileDamage = 5;
    public float splitSpreadAngle = 90f;

    void DoExplosion()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                Debug.Log($"Explosion hit Player: {hitCollider.name} for {explosionDamage} damage!");
            }
            else if (hitCollider.CompareTag("Enemy") && hitCollider.gameObject != transform.parent.gameObject)
            {
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(explosionDamage);
                    Debug.Log($"Explosion hit Enemy: {hitCollider.name} for {explosionDamage} damage!");
                }
            }
        }

        if (smallProjectilePrefab != null && numberOfSplitProjectiles > 0)
        {
            float angleStep = splitSpreadAngle / (numberOfSplitProjectiles - 1);
            if (numberOfSplitProjectiles == 1) angleStep = 0;

            float currentAngle = -splitSpreadAngle / 2f;

            for (int i = 0; i < numberOfSplitProjectiles; i++)
            {
                Quaternion rotation = Quaternion.Euler(0, 0, currentAngle) * transform.rotation;

                GameObject smallProjectileGO = Instantiate(smallProjectilePrefab, transform.position, rotation);

                EnemyProjectile projectileScript = smallProjectileGO.GetComponent<EnemyProjectile>();
                if (projectileScript != null)
                {
                    projectileScript.speed = splitProjectileSpeed;
                    projectileScript.damage = splitProjectileDamage;
                }

                currentAngle += angleStep;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        if (numberOfSplitProjectiles > 0)
        {
            Gizmos.color = Color.cyan;
            float angleStep = splitSpreadAngle / (numberOfSplitProjectiles - 1);
            if (numberOfSplitProjectiles == 1) angleStep = 0;
            float startAngle = -splitSpreadAngle / 2f;

            for (int i = 0; i < numberOfSplitProjectiles; i++)
            {
                float currentAngle = startAngle + i * angleStep;
                Vector2 direction = Quaternion.Euler(0, 0, currentAngle) * Vector2.down;
                Gizmos.DrawRay(transform.position, direction * 2f);
            }
        }
    }
}
