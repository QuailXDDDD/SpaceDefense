using UnityEngine;

[CreateAssetMenu(fileName = "NewBossEnemyData", menuName = "Enemy/Boss Enemy Data")]
public class BossEnemyData : EnemyData
{
    [Header("Boss Specific Stats")]
    public int bossScoreValue = 1000;
    public float entryMoveDuration = 3f;
    public Vector3 entryTargetPosition = new Vector3(0, 3, 0);
    
    [Header("Phase Configuration")]
    public float phase1HealthThreshold = 0.7f;
    public float phase2HealthThreshold = 0.3f;
    
    [Header("Phase 1 - Basic Attacks")]
    public float phase1FireRate = 1.5f;
    public GameObject phase1ProjectilePrefab;
    public int phase1ProjectileDamage = 15;
    public float phase1ProjectileSpeed = 6f;
    
    [Header("Phase 2 - Enhanced Attacks")]
    public float phase2FireRate = 2.0f;
    public GameObject phase2ProjectilePrefab;
    public int phase2ProjectileDamage = 20;
    public float phase2ProjectileSpeed = 7f;
    public int phase2BurstCount = 3;
    public float phase2BurstDelay = 0.2f;
    
    [Header("Phase 3 - Final Phase")]
    public float phase3FireRate = 3.0f;
    public GameObject phase3ProjectilePrefab;
    public int phase3ProjectileDamage = 25;
    public float phase3ProjectileSpeed = 8f;
    public int phase3SpreadCount = 5;
    public float phase3SpreadAngle = 30f;
    
    [Header("Special Abilities")]
    public bool hasShieldAbility = true;
    public float shieldDuration = 5f;
    public float shieldCooldown = 15f;
    public bool hasTeleportAbility = false;
    public float teleportCooldown = 10f;
    
    [Header("Visual Effects")]
    public GameObject phaseTransitionEffect;
    public GameObject deathEffect;
    public Color phase1Color = Color.white;
    public Color phase2Color = Color.red;
    public Color phase3Color = Color.magenta;
} 