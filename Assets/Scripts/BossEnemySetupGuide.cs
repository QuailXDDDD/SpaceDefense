using UnityEngine;

/// <summary>
/// This script provides setup instructions and validation for BossEnemy prefabs.
/// It's meant to be attached to boss prefabs during development and can be removed for production.
/// </summary>
public class BossEnemySetupGuide : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(10, 20)]
    public string setupInstructions = @"
BOSS ENEMY SETUP GUIDE:

1. REQUIRED COMPONENTS:
   - BossEnemy script
   - BossPhaseAttacker script  
   - BossSpecialAbilities script
   - SpriteRenderer
   - Collider2D (for damage detection)
   - Rigidbody2D (set to Kinematic)

2. FIRE POINTS:
   - Fire points are automatically created if none are assigned
   - 3 default fire points: Center, Left, Right
   - You can manually create and assign custom fire points if needed
   - Fire points appear as child objects under the boss

3. REQUIRED ASSETS:
   - BossEnemyData ScriptableObject
   - Projectile prefabs for each phase
   - Visual effects (shield, teleport, death)

4. SETUP STEPS:
   a) Create BossEnemyData asset (Right-click > Create > Enemy > Boss Enemy Data)
   b) Configure boss stats, phases, and abilities in the data asset
   c) Assign the BossEnemyData to the BossEnemy component
   d) Fire points are automatically created (or assign custom ones)
   e) Configure special abilities in BossSpecialAbilities component
   f) Test the boss in the scene

5. PHASE SYSTEM:
   - Phase 1: Basic attacks (70%+ health)
   - Phase 2: Enhanced attacks (30-70% health)  
   - Phase 3: Final phase (0-30% health)

6. SPECIAL ABILITIES:
   - Shield: Activates at 30% health, blocks damage
   - Teleport: Activates at 20% health, moves to random position

7. VISUAL FEEDBACK:
   - Color changes per phase
   - Shield visual effects
   - Teleport effects
   - Death explosion
";

    [Header("Validation")]
    public bool validateOnStart = true;
    
    void Start()
    {
        if (validateOnStart)
        {
            ValidateBossSetup();
        }
    }
    
    [ContextMenu("Validate Boss Setup")]
    public void ValidateBossSetup()
    {
        Debug.Log("=== BOSS ENEMY SETUP VALIDATION ===");
        
        bool isValid = true;
        
        // Check required components
        BossEnemy bossEnemy = GetComponent<BossEnemy>();
        if (bossEnemy == null)
        {
            Debug.LogError("❌ Missing BossEnemy component!");
            isValid = false;
        }
        else
        {
            Debug.Log("✅ BossEnemy component found");
        }
        
        BossPhaseAttacker phaseAttacker = GetComponent<BossPhaseAttacker>();
        if (phaseAttacker == null)
        {
            Debug.LogError("❌ Missing BossPhaseAttacker component!");
            isValid = false;
        }
        else
        {
            Debug.Log("✅ BossPhaseAttacker component found");
        }
        
        BossSpecialAbilities specialAbilities = GetComponent<BossSpecialAbilities>();
        if (specialAbilities == null)
        {
            Debug.LogError("❌ Missing BossSpecialAbilities component!");
            isValid = false;
        }
        else
        {
            Debug.Log("✅ BossSpecialAbilities component found");
        }
        
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("❌ Missing SpriteRenderer component!");
            isValid = false;
        }
        else
        {
            Debug.Log("✅ SpriteRenderer component found");
        }
        
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogError("❌ Missing Collider2D component!");
            isValid = false;
        }
        else
        {
            Debug.Log("✅ Collider2D component found");
        }
        
        // Check BossEnemyData
        if (bossEnemy != null && bossEnemy.enemyData == null)
        {
            Debug.LogError("❌ BossEnemyData not assigned to BossEnemy!");
            isValid = false;
        }
        else if (bossEnemy != null && bossEnemy.enemyData != null)
        {
            if (bossEnemy.enemyData is BossEnemyData)
            {
                Debug.Log("✅ BossEnemyData assigned and is correct type");
            }
            else
            {
                Debug.LogError("❌ EnemyData is not BossEnemyData type!");
                isValid = false;
            }
        }
        
        // Check fire points
        if (phaseAttacker != null)
        {
            Transform[] firePoints = phaseAttacker.firePoints;
            if (firePoints == null || firePoints.Length == 0)
            {
                Debug.LogError("❌ No fire points assigned to BossPhaseAttacker!");
                isValid = false;
            }
            else
            {
                Debug.Log($"✅ {firePoints.Length} fire point(s) assigned");
                
                for (int i = 0; i < firePoints.Length; i++)
                {
                    if (firePoints[i] == null)
                    {
                        Debug.LogError($"❌ Fire point {i} is null!");
                        isValid = false;
                    }
                }
            }
        }
        
        // Final result
        if (isValid)
        {
            Debug.Log("🎉 Boss setup validation PASSED! Boss is ready for testing.");
        }
        else
        {
            Debug.LogError("❌ Boss setup validation FAILED! Please fix the issues above.");
        }
        
        Debug.Log("=== END VALIDATION ===");
    }
    
    [ContextMenu("Show Setup Instructions")]
    public void ShowSetupInstructions()
    {
        Debug.Log(setupInstructions);
    }
} 