using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    [Header("UI Button References")]
    public Button startButton;
    public Button leaderboardsButton;
    public Button exitButton;
    
    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene";
    public string leaderboardSceneName = "LeaderBoardsScreen";
    
    void Start()
    {
        SetupButtons();
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log("StartScreenManager: Start screen initialized");
    }
    
    void SetupButtons()
    {
        if (startButton == null)
        {
            startButton = GameObject.Find("StartButton")?.GetComponent<Button>();
        }
        
        if (leaderboardsButton == null)
        {
            leaderboardsButton = GameObject.Find("LeaderboardsButton")?.GetComponent<Button>();
        }
        
        if (exitButton == null)
        {
            exitButton = GameObject.Find("ExitButton")?.GetComponent<Button>();
        }
        
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
            Debug.Log("StartScreenManager: START button connected");
        }
        else
        {
            Debug.LogWarning("StartScreenManager: START button not found! Please assign it in the inspector or name it 'StartButton'");
        }
        
        if (leaderboardsButton != null)
        {
            leaderboardsButton.onClick.AddListener(ShowLeaderboards);
            Debug.Log("StartScreenManager: LEADERBOARDS button connected");
        }
        else
        {
            Debug.LogWarning("StartScreenManager: LEADERBOARDS button not found! Please assign it in the inspector or name it 'LeaderboardsButton'");
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
            Debug.Log("StartScreenManager: EXIT button connected");
        }
        else
        {
            Debug.LogWarning("StartScreenManager: EXIT button not found! Please assign it in the inspector or name it 'ExitButton'");
        }
    }
    
    public void StartGame()
    {
        Debug.Log("StartScreenManager: Starting game - Loading " + gameSceneName);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void ShowLeaderboards()
    {
        Debug.Log("StartScreenManager: Loading leaderboard scene - " + leaderboardSceneName);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        SceneManager.LoadScene(leaderboardSceneName);
    }
    
    public void ExitGame()
    {
        Debug.Log("StartScreenManager: Exiting game");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
        }
        
        if (leaderboardsButton != null)
        {
            leaderboardsButton.onClick.RemoveListener(ShowLeaderboards);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(ExitGame);
        }
    }
    
    [ContextMenu("Test Start Game")]
    public void TestStartGame()
    {
        StartGame();
    }
    
    [ContextMenu("Test Exit Game")]
    public void TestExitGame()
    {
        ExitGame();
    }
} 