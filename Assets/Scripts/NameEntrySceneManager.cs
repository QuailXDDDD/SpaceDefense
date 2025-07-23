using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class NameEntrySceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TMP_InputField playerNameInput;
    public Button saveButton;
    public Button mainMenuButton;
    
    [Header("Scene Settings")]
    public string startSceneName = "StartScreen";
    public string leaderboardSceneName = "LeaderBoardsScreen";
    
    [Header("Name Entry Settings")]
    public string defaultPlayerName = "PILOT";
    public string titleTextContent = "Enter Your Name";
    
    [Header("Visual Settings")]
    public Color titleColor = Color.white;
    
    private bool scoreIsSaved = false;
    private int finalScore;
    private int finalWave;
    private float survivalTime;
    private bool wasVictory;
    
    void Start()
    {
        GetGameData();
        
        SetupUI();
        SetupButtons();
        UpdateDisplay();
        
        if (playerNameInput != null)
        {
            playerNameInput.Select();
            playerNameInput.ActivateInputField();
        }
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log("NameEntrySceneManager: Name entry scene initialized");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SaveScore();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }
    }
    
    void GetGameData()
    {
        finalScore = GameOverManager.LastFinalScore;
        finalWave = GameOverManager.LastFinalWave;
        survivalTime = GameOverManager.LastSurvivalTime;
        wasVictory = GameOverManager.LastWasVictory;
        
        Debug.Log($"NameEntrySceneManager: Retrieved game data - Score: {finalScore}, Wave: {finalWave}, Time: {survivalTime:F1}s, Victory: {wasVictory}");
    }
    
    void SetupUI()
    {
        AutoFindComponents();
        
        if (titleText != null)
        {
            titleText.color = titleColor;
        }
        
        if (playerNameInput != null)
        {
            playerNameInput.text = defaultPlayerName;
            playerNameInput.characterLimit = 12;
        }
    }
    
    void AutoFindComponents()
    {
        if (titleText == null)
        {
            GameObject titleObj = GameObject.Find("TitleText");
            if (titleObj != null)
                titleText = titleObj.GetComponent<TextMeshProUGUI>();
        }
        
        if (playerNameInput == null)
        {
            GameObject inputObj = GameObject.Find("PlayerNameInput");
            if (inputObj != null)
                playerNameInput = inputObj.GetComponent<TMP_InputField>();
        }
        
        if (saveButton == null)
        {
            GameObject saveObj = GameObject.Find("SaveButton");
            if (saveObj != null)
                saveButton = saveObj.GetComponent<Button>();
        }
        
        if (mainMenuButton == null)
        {
            GameObject menuObj = GameObject.Find("MainMenuButton");
            if (menuObj != null)
                mainMenuButton = menuObj.GetComponent<Button>();
        }
    }
    
    void SetupButtons()
    {
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveScore);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }
    
    void UpdateDisplay()
    {
        if (titleText != null)
        {
            titleText.text = titleTextContent;
        }
    }
    
    public void SaveScore()
    {
        Debug.Log("NameEntrySceneManager: SaveScore method called");
        
        if (scoreIsSaved)
        {
            Debug.Log("NameEntrySceneManager: Score already saved, skipping");
            return;
        }
        
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("NameEntrySceneManager: LeaderboardManager.Instance is null!");
            return;
        }
        
        string playerName = playerNameInput != null ? playerNameInput.text : defaultPlayerName;
        if (string.IsNullOrEmpty(playerName.Trim()))
        {
            playerName = defaultPlayerName;
        }
        
        Debug.Log($"NameEntrySceneManager: Saving score - Name: {playerName}, Score: {finalScore}, Wave: {finalWave}, Time: {survivalTime}");
        
        try
        {
            LeaderboardManager.Instance.AddScore(playerName, finalScore, finalWave, survivalTime);
            scoreIsSaved = true;
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPowerUp();
            }
            
            Debug.Log($"NameEntrySceneManager: Score saved successfully - {playerName}: {finalScore} points");
            Debug.Log($"NameEntrySceneManager: Attempting to load scene: '{leaderboardSceneName}'");
            
            bool sceneExists = false;
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                Debug.Log($"NameEntrySceneManager: Build scene {i}: {sceneName}");
                if (sceneName == leaderboardSceneName)
                {
                    sceneExists = true;
                    break;
                }
            }
            
            if (!sceneExists)
            {
                Debug.LogError($"NameEntrySceneManager: Scene '{leaderboardSceneName}' not found in build settings!");
                Debug.LogError("NameEntrySceneManager: Going to start screen instead");
                SceneManager.LoadScene(startSceneName);
                return;
            }
            
            Debug.Log($"NameEntrySceneManager: Loading scene '{leaderboardSceneName}' now...");
            SceneManager.LoadScene(leaderboardSceneName);
            Debug.Log($"NameEntrySceneManager: SceneManager.LoadScene called successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"NameEntrySceneManager: Error in SaveScore: {e.Message}");
            Debug.LogError($"NameEntrySceneManager: Stack trace: {e.StackTrace}");
            
            Debug.Log("NameEntrySceneManager: Falling back to start screen");
            SceneManager.LoadScene(startSceneName);
        }
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("NameEntrySceneManager: Returning to main menu");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        SceneManager.LoadScene(startSceneName);
    }
    
    [ContextMenu("Test Save Score")]
    public void TestSaveScore()
    {
        finalScore = 9999;
        finalWave = 3;
        survivalTime = 120f;
        SaveScore();
    }
    
    [ContextMenu("Test Scene Transition")]
    public void TestSceneTransition()
    {
        Debug.Log($"NameEntrySceneManager: Testing direct scene transition to '{leaderboardSceneName}'");
        try
        {
            SceneManager.LoadScene(leaderboardSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"NameEntrySceneManager: Direct scene transition failed: {e.Message}");
        }
    }
    
    [ContextMenu("List All Scenes")]
    public void ListAllScenes()
    {
        Debug.Log("NameEntrySceneManager: Listing all scenes in build settings:");
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"  Scene {i}: {sceneName} (Path: {scenePath})");
        }
    }
} 