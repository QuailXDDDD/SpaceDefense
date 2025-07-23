using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI gameOverTitle;
    public TextMeshProUGUI finalScoreText;
    public Button saveScoreButton;
    public Button tryAgainButton;
    public Button mainMenuButton;
    
    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene";
    public string startSceneName = "StartScreen";
    public string nameEntrySceneName = "NameEntryScene";
    
    [Header("Game Over Settings")]
    public string gameOverTitleText = "GAME OVER";
    
    [Header("Visual Settings")]
    public Color gameOverColor = Color.red;
    public Color scoreColor = Color.white;
    
    private int finalScore;
    private int finalWave;
    private float survivalTime;
    
    void Start()
    {
        GetGameData();
        
        SetupUI();
        SetupButtons();
        UpdateDisplay();
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log("GameOverSceneManager: Game over scene initialized");
    }
    
    void GetGameData()
    {
        finalScore = GameOverManager.LastFinalScore;
        finalWave = GameOverManager.LastFinalWave;
        survivalTime = GameOverManager.LastSurvivalTime;
        
        Debug.Log($"GameOverSceneManager: Retrieved game data - Score: {finalScore}, Wave: {finalWave}, Time: {survivalTime:F1}s");
    }
    
    void SetupUI()
    {
        AutoFindComponents();
        
        if (gameOverTitle != null)
        {
            gameOverTitle.color = gameOverColor;
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.color = scoreColor;
        }
    }
    
    void AutoFindComponents()
    {
        if (gameOverTitle == null)
        {
            GameObject titleObj = GameObject.Find("GameOverTitle");
            if (titleObj != null)
                gameOverTitle = titleObj.GetComponent<TextMeshProUGUI>();
        }
        
        if (finalScoreText == null)
        {
            GameObject scoreObj = GameObject.Find("FinalScoreText");
            if (scoreObj != null)
                finalScoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        }
        
        if (saveScoreButton == null)
        {
            GameObject saveObj = GameObject.Find("SaveScoreButton");
            if (saveObj != null)
                saveScoreButton = saveObj.GetComponent<Button>();
        }
        
        if (tryAgainButton == null)
        {
            GameObject tryObj = GameObject.Find("TryAgainButton");
            if (tryObj != null)
                tryAgainButton = tryObj.GetComponent<Button>();
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
        if (saveScoreButton != null)
        {
            saveScoreButton.onClick.AddListener(GoToNameEntry);
        }
        
        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.AddListener(TryAgain);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }
    
    void UpdateDisplay()
    {
        if (gameOverTitle != null)
        {
            gameOverTitle.text = gameOverTitleText;
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"FINAL SCORE: {finalScore:N0}";
        }
        
        CheckHighScore();
    }
    
    void CheckHighScore()
    {
        if (saveScoreButton != null)
        {
            bool isHighScore = LeaderboardManager.Instance != null && 
                              LeaderboardManager.Instance.IsHighScore(finalScore);
            
            saveScoreButton.gameObject.SetActive(isHighScore);
            
            if (isHighScore)
            {
                Debug.Log("GameOverSceneManager: High score detected! Save button shown.");
            }
        }
    }
    
    public void GoToNameEntry()
    {
        Debug.Log("GameOverSceneManager: Going to name entry screen");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        SceneManager.LoadScene(nameEntrySceneName);
    }
    
    public void TryAgain()
    {
        Debug.Log("GameOverSceneManager: Starting new game");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("GameOverSceneManager: Returning to main menu");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        SceneManager.LoadScene(startSceneName);
    }
    
    [ContextMenu("Test High Score Display")]
    public void TestHighScoreDisplay()
    {
        finalScore = 50000;
        UpdateDisplay();
    }
    
    [ContextMenu("Test Low Score Display")]
    public void TestLowScoreDisplay()
    {
        finalScore = 500;
        UpdateDisplay();
    }
} 