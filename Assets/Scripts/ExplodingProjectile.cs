// This is the code for ExplodingProjectile.cs
// Make sure to replace all existing content in the newly created file with this.

using UnityEngine;
using System.Collections; // Include this if you ever add coroutines to this script

public class ExplodingProjectile : EnemyProjectile // This line is crucial: it inherits from EnemyProjectile
{
    [Header("Explosion Properties")]
    public GameObject explosionEffectPrefab; // Assign the Explosion_FX prefab here
    public float explosionRadius = 2f;      // How far the explosion damages
    public int explosionDamage = 20;        // Damage dealt by the explosion

    [Header("Split Projectile Properties")]
    public GameObject smallProjectilePrefab; // Drag your SmallProjectile_Prefab here
    public int numberOfSplitProjectiles = 4; // How many smaller projectiles to spawn
    public float splitProjectileSpeed = 7f; // Speed of the smaller projectiles
    public int splitProjectileDamage = 5; // Damage of the smaller projectiles
    public float splitSpreadAngle = 90f; // Total angle to spread them (e.g., 90 degrees for a quarter-circle spread)

    // Override OnTriggerEnter2D to add explosion and splitting logic
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Only explode if we hit something that should trigger an explosion
        // (e.g., player, enemy, obstacle - define your collision layers/tags)
        if (other.CompareTag("Player") || other.CompareTag("Enemy") || other.CompareTag("Obstacle"))
        {
            DoExplosion();
            Destroy(gameObject); // Destroy the projectile itself after exploding
        }
        // If you want it to bounce off certain things, add else if conditions here without Destroy(gameObject)
    }

    void DoExplosion()
    {
        // 1. Instantiate the visual explosion effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. Damage nearby colliders
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            // Damage players
            if (hitCollider.CompareTag("Player"))
            {
                // TODO: Call damage method on Player (e.g., hitCollider.GetComponent<PlayerHealth>().TakeDamage(explosionDamage);)
                Debug.Log($"Explosion hit Player: {hitCollider.name} for {explosionDamage} damage!");
            }
            // Damage other enemies (optional, for chain reactions)
            else if (hitCollider.CompareTag("Enemy") && hitCollider.gameObject != transform.parent.gameObject) // Don't damage self or spawner if it's an enemy itself
            {
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(explosionDamage);
                    Debug.Log($"Explosion hit Enemy: {hitCollider.name} for {explosionDamage} damage!");
                }
            }
            // Add logic for damaging other destructible objects
        }

        // 3. Spawn smaller projectiles
        if (smallProjectilePrefab != null && numberOfSplitProjectiles > 0)
        {
            // Calculate the angle increment for each projectile
            float angleStep = splitSpreadAngle / (numberOfSplitProjectiles - 1);
            if (numberOfSplitProjectiles == 1) angleStep = 0; // Handle single projectile case

            // Calculate the starting angle for even spread (e.g., -45 for 90-degree spread)
            // We assume projectiles are initially pointing downwards, and we rotate them around Z-axis
            float currentAngle = -splitSpreadAngle / 2f;

            for (int i = 0; i < numberOfSplitProjectiles; i++)
            {
                // Create a rotation for the current projectile
                Quaternion rotation = Quaternion.Euler(0, 0, currentAngle) * transform.rotation; // Apply spread relative to original direction

                // Instantiate the small projectile
                GameObject smallProjectileGO = Instantiate(smallProjectilePrefab, transform.position, rotation);

                // Set its speed and damage (important, as it's a new instance)
                EnemyProjectile projectileScript = smallProjectileGO.GetComponent<EnemyProjectile>();
                if (projectileScript != null)
                {
                    projectileScript.speed = splitProjectileSpeed;
                    projectileScript.damage = splitProjectileDamage;
                }

                currentAngle += angleStep; // Increment angle for the next projectile
            }
        }
    }

    // Draw the explosion radius and split projectile directions in the editor for debugging
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
                // Convert angle to direction vector relative to original projectile's forward (down)
                Vector2 direction = Quaternion.Euler(0, 0, currentAngle) * Vector2.down;
                Gizmos.DrawRay(transform.position, direction * 2f); // Draw a short line
            }
        }
    }
}
