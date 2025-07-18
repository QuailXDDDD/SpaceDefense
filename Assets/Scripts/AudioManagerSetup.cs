using UnityEngine;

[System.Serializable]
public class AudioManagerSetup : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(3, 10)]
    public string setupInstructions = 
        "AUDIO MANAGER SETUP:\n" +
        "1. Drag audio clips (MP3, WAV, OGG) into the Audio Clips folder\n" +
        "2. Assign clips to the AudioManager script below\n" +
        "3. Assign the Explosion_FX prefab to all enemy and player explosion prefab fields\n" +
        "4. Test sounds by playing the game!\n\n" +
        "SOUND EFFECTS NEEDED:\n" +
        "- Player shoot sound (rapid fire)\n" +
        "- Player explosion sound (death)\n" +
        "- Player hit sound (taking damage)\n" +
        "- Enemy shoot sound (various enemies)\n" +
        "- Enemy explosion sound (destruction)\n" +
        "- Enemy hit sound (taking damage)\n" +
        "- Boss shoot sound (powerful)\n" +
        "- Boss explosion sound (epic death)\n" +
        "- Boss hit sound (heavy impact)\n" +
        "- Projectile hit sound (bullets hitting targets)";
    
    void Start()
    {
        // Check if AudioManager instance exists
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManagerSetup: No AudioManager found in scene! Please add one.");
        }
        else
        {
            Debug.Log("AudioManagerSetup: AudioManager found successfully!");
        }
        
        // Check for explosion prefabs on enemies and player
        CheckExplosionPrefabs();
    }
    
    void CheckExplosionPrefabs()
    {
        // Check player explosion prefab
        PlayerShip player = Object.FindAnyObjectByType<PlayerShip>();
        if (player != null && player.explosionPrefab == null)
        {
            Debug.LogWarning("AudioManagerSetup: PlayerShip is missing explosion prefab! Please assign Explosion_FX prefab.");
        }
        
        // Check enemy explosion prefabs
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            if (enemy.explosionPrefab == null)
            {
                Debug.LogWarning($"AudioManagerSetup: Enemy '{enemy.name}' is missing explosion prefab! Please assign Explosion_FX prefab.");
            }
        }
        
        Debug.Log($"AudioManagerSetup: Checked {enemies.Length} enemies for explosion prefabs.");
    }
} 