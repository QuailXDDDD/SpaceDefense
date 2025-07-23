using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Base Stats")]
    public string enemyName = "Basic Enemy";
    public int maxHealth = 50;
    public int scoreValue = 10;
    public float moveSpeed = 0f;

    [Header("Visuals")]
    public Sprite enemySprite;
    public Vector3 scale = new Vector3(1, 1, 1);

    [Header("Attack Properties")]
    public float baseFireRate = 1f;
    public GameObject projectilePrefab;
    public int projectileDamage = 10;
    public float projectileSpeed = 5f;
}