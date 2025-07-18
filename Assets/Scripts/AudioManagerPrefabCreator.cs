using UnityEngine;

[System.Serializable]
public class AudioManagerPrefabCreator : MonoBehaviour
{
    [Header("Audio Manager Setup")]
    [TextArea(5, 15)]
    public string instructions = 
        "AUDIO SETUP INSTRUCTIONS:\n\n" +
        "1. Click 'Create AudioManager' button below\n" +
        "2. Drag audio files (.mp3, .wav, .ogg) into Assets/Audio folder\n" +
        "3. Assign audio clips to the AudioManager in the scene\n" +
        "4. Test by playing the game!\n\n" +
        "REQUIRED SOUNDS:\n" +
        "• Player Shoot (rapid fire laser)\n" +
        "• Player Hit (damage taken)\n" +
        "• Player Explosion (death)\n" +
        "• Enemy Shoot (enemy laser)\n" +
        "• Enemy Hit (enemy damaged)\n" +
        "• Enemy Explosion (enemy destroyed)\n" +
        "• Boss Shoot (powerful sound)\n" +
        "• Boss Hit (heavy impact)\n" +
        "• Boss Explosion (epic destruction)\n" +
        "• Projectile Hit (bullet impact)";

    [Header("Quick Setup")]
    public bool createAudioManager = false;
    public bool createAudioFolder = false;

    void Start()
    {
        // Only run once
        if (createAudioManager)
        {
            CreateAudioManagerInScene();
            createAudioManager = false;
        }
        
        if (createAudioFolder)
        {
            CreateAudioFolder();
            createAudioFolder = false;
        }
    }

    public void CreateAudioManagerInScene()
    {
        // Check if AudioManager already exists
        if (AudioManager.Instance != null)
        {
            Debug.Log("AudioManager already exists in scene!");
            return;
        }

        // Create AudioManager GameObject
        GameObject audioManagerObj = new GameObject("AudioManager");
        AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();
        
        // Add AudioManagerSetup for convenience
        audioManagerObj.AddComponent<AudioManagerSetup>();
        
        Debug.Log("AudioManager created in scene! Please assign audio clips in the inspector.");
        
        // Make it persist across scenes
        DontDestroyOnLoad(audioManagerObj);
    }

    void CreateAudioFolder()
    {
#if UNITY_EDITOR
        // Create Audio folder in Assets
        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Audio"))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Audio");
            Debug.Log("Created Assets/Audio folder for your audio files!");
        }
        else
        {
            Debug.Log("Assets/Audio folder already exists!");
        }
#endif
    }

    // Method that can be called from inspector button
    [ContextMenu("Create AudioManager")]
    public void CreateAudioManagerButton()
    {
        CreateAudioManagerInScene();
    }

    [ContextMenu("Create Audio Folder")]
    public void CreateAudioFolderButton()
    {
        CreateAudioFolder();
    }
} 