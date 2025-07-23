using UnityEngine;

public class WaveSystemSetupGuide : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(15, 30)]
    public string setupInstructions = @"
WAVE SYSTEM SETUP GUIDE:

=== REQUIRED COMPONENTS ===

1. WAVE MANAGER:
   - Create empty GameObject named 'WaveManager'
   - Add WaveManager script
   - Configure wave settings in inspector:
     * Wave 1 Enemy Prefab (Basic enemy for straight row)
     * Wave 2 Enemy Prefab (Enemy for zigzag formation)
     * Wave 3 Enemy Prefab (Enemy for circle formation)
     * Boss Prefab (Boss for center of circle)

2. ENEMY PREFABS:
   - Make sure enemy prefabs have:
     * EnemyBehaviour or Enemy script
     * EnemyData ScriptableObject assigned
     * Collider2D (Is Trigger = true)
     * Rigidbody2D (Is Kinematic = true)
     * SpriteRenderer with enemy sprite

3. BOSS PREFAB:
   - Should have BossEnemy script
   - BossEnemyData ScriptableObject assigned
   - Attack components (BossPhaseAttacker, etc.)

=== OPTIONAL COMPONENTS ===

4. WAVE UI (Optional):
   - Create UI Canvas if not exists
   - Add UI Text elements for:
     * Wave Info Text
     * Enemy Count Text
     * Wave Progress Text
   - Create GameObject with WaveUI script
   - Assign UI Text references

=== SETUP STEPS ===

1. PREPARE PREFABS:
   a) Set up your enemy prefabs with proper scripts
   b) Create enemy data assets (Right-click > Create > Enemy > Enemy Data)
   c) Set up boss prefab with BossEnemy script
   d) Create boss data asset (Right-click > Create > Enemy > Boss Enemy Data)

2. CREATE WAVE MANAGER:
   a) Create empty GameObject: 'WaveManager'
   b) Add WaveManager script
   c) Assign all required prefabs
   d) Configure wave settings:
      - Wave 1: 3 enemies, 2f spacing, 2f move speed
      - Wave 2: 2x5 grid formation, 1.5f spacing, 2f move speed
      - Wave 3: 4x6 formation, 1f spacing, 2f move speed
      - Wave 4: 5 enemies, 3f radius, 30f rotation speed

3. SETUP UI (Optional):
   a) Create Canvas (UI > Canvas)
   b) Add Text elements for wave info
   c) Create GameObject with WaveUI script
   d) Assign Text references

4. CONFIGURE SCENE:
   a) Position WaveManager at (0, 0, 0)
   b) Ensure Camera is properly positioned
   c) Remove any existing enemy spawners
   d) Test the system

=== WAVE BEHAVIORS ===

WAVE 1 - Straight Row Formation:
- 3 enemies spawn in horizontal line
- Move straight down from top of screen
- No side-to-side movement
- Basic enemy shooting patterns

WAVE 2 - Grid Formation:
- 2 rows of 5 enemies (10 total)
- Spawns in neat grid pattern
- Moves straight down as formation
- Coordinated enemy attacks

WAVE 3 - ZigZag Formation:
- 4x6 grid of enemies (24 total)
- Side-to-side zigzag movement
- Move down while zigzagging
- Formation stays together

WAVE 4 - Circle Formation + Boss:
- 5 enemies in circle formation
- Boss enemy in center
- Enemies rotate around boss
- Whole formation moves down
- Boss becomes active when formation enters screen

=== CUSTOMIZATION ===

- Adjust enemy counts in WaveManager
- Modify movement speeds and patterns
- Change formation spacing and sizes
- Add custom enemy behaviors
- Modify wave progression timing

=== TROUBLESHOOTING ===

Common Issues:
1. Enemies not spawning: Check prefab assignments
2. UI not updating: Verify WaveUI script has proper references
3. Boss not activating: Check boss prefab has BossEnemy script
4. Formation positioning: Adjust spawn positions in WaveManager
5. Enemy movement issues: Check EnemyData move speeds

For more help, check the console for debug messages from:
- WaveManager
- StraightRowFormation
- ZigZagFormation1
- CircleFormation
";

    [Header("Quick Setup")]
    [SerializeField] private bool autoFindComponents = true;
    
    void Start()
    {
        if (autoFindComponents)
        {
            ValidateSetup();
        }
    }
    
    [ContextMenu("Validate Wave System Setup")]
    public void ValidateSetup()
    {
        Debug.Log("=== WAVE SYSTEM SETUP VALIDATION ===");
        
        bool isValid = true;
        
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager == null)
        {
            Debug.LogError("❌ WaveManager not found in scene!");
            isValid = false;
        }
        else
        {
            Debug.Log("✅ WaveManager found");
            
            if (waveManager.wave1EnemyPrefab == null)
            {
                Debug.LogError("❌ Wave 1 Enemy Prefab not assigned!");
                isValid = false;
            }
            else
            {
                Debug.Log("✅ Wave 1 Enemy Prefab assigned");
            }
            
            if (waveManager.wave2EnemyPrefab == null)
            {
                Debug.LogError("❌ Wave 2 Enemy Prefab not assigned!");
                isValid = false;
            }
            else
            {
                Debug.Log("✅ Wave 2 Enemy Prefab assigned");
            }
            
            if (waveManager.wave3EnemyPrefab == null)
            {
                Debug.LogError("❌ Wave 3 Enemy Prefab not assigned!");
                isValid = false;
            }
            else
            {
                Debug.Log("✅ Wave 3 Enemy Prefab assigned");
            }
            
            if (waveManager.bossPrefab == null)
            {
                Debug.LogError("❌ Boss Prefab not assigned!");
                isValid = false;
            }
            else
            {
                Debug.Log("✅ Boss Prefab assigned");
            }
        }
        
        WaveUI waveUI = FindFirstObjectByType<WaveUI>();
        if (waveUI == null)
        {
            Debug.LogWarning("⚠️ WaveUI not found (optional component)");
        }
        else
        {
            Debug.Log("✅ WaveUI found");
        }
        
        AudioManager audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogWarning("⚠️ AudioManager not found (optional for sound effects)");
        }
        else
        {
            Debug.Log("✅ AudioManager found");
        }
        
        if (isValid)
        {
            Debug.Log("🎉 Wave System setup is valid! Ready to play!");
        }
        else
        {
            Debug.Log("❌ Wave System setup has issues. Please fix the errors above.");
        }
    }
    
    [ContextMenu("Create Basic UI Setup")]
    public void CreateBasicUISetup()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        GameObject waveUIObj = new GameObject("WaveUI");
        waveUIObj.transform.SetParent(canvas.transform, false);
        
        WaveUI waveUI = waveUIObj.AddComponent<WaveUI>();
        
        Debug.Log("Basic Wave UI setup created. You still need to manually create and assign Text components.");
    }
} 