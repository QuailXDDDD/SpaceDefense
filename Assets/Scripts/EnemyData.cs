using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Base Stats")]
    public string enemyName = "Basic Enemy";
    public int maxHealth = 50;
    public int scoreValue = 10;
    public float moveSpeed = 0f; // Added: Speed at which the enemy moves downwards

    [Header("Visuals")]
    public Sprite enemySprite;
    public Vector3 scale = new Vector3(1, 1, 1); // Default scale

    [Header("Attack Properties")]
    public float baseFireRate = 1f; // Shots per second
    public GameObject projectilePrefab; // Default projectile for this enemy type
    public int projectileDamage = 10;
    public float projectileSpeed = 5f;

    // Add more properties here for different attack behaviors if needed
    // e.g.,
    // public int burstShotCount = 3;
    // public float burstDelay = 0.1f;
    // public float spreadAngle = 10f; // For multi-shot enemies
}