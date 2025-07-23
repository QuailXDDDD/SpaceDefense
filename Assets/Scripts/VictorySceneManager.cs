using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictorySceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI victoryTitle;
    public TextMeshProUGUI congratulationsText;
    public TextMeshProUGUI finalScoreText;
    public Button saveScoreButton;
    public Button playAgainButton;
    public Button mainMenuButton;
    
    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene";
    public string startSceneName = "StartScreen";
    public string nameEntrySceneName = "NameEntryScene";
    
    [Header("Victory Settings")]
    public string victoryTitleText = "VICTORY";
    public string congratulationsTextContent = "YOU HAVE SAVED THE EARTH!";
    
    [Header("Visual Settings")]
    public Color victoryColor = Color.yellow;
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
        
        Debug.Log("VictorySceneManager: Victory scene initialized");
    }
    
    void GetGameData()
    {
        finalScore = GameOverManager.LastFinalScore;
        finalWave = GameOverManager.LastFinalWave;
        survivalTime = GameOverManager.LastSurvivalTime;
        
        Debug.Log($"VictorySceneManager: Retrieved game data - Score: {finalScore}, Wave: {finalWave}, Time: {survivalTime:F1}s");
    }
    
    void SetupUI()
    {
        AutoFindComponents();
        
        if (victoryTitle != null)
        {
            victoryTitle.color = victoryColor;
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.color = scoreColor;
        }
    }
    
    void AutoFindComponents()
    {
        if (victoryTitle == null)
        {
            GameObject titleObj = GameObject.Find("VictoryTitle");
            if (titleObj != null)
                victoryTitle = titleObj.GetComponent<TextMeshProUGUI>();
        }
        
        if (congratulationsText == null)
        {
            GameObject congratsObj = GameObject.Find("CongratulationsText");
            if (congratsObj != null)
                congratulationsText = congratsObj.GetComponent<TextMeshProUGUI>();
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
        
        if (playAgainButton == null)
        {
            GameObject playObj = GameObject.Find("PlayAgainButton");
            if (playObj != null)
                playAgainButton = playObj.GetComponent<Button>();
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
        
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }
    
    void UpdateDisplay()
    {
        if (victoryTitle != null)
        {
            victoryTitle.text = victoryTitleText;
        }
        
        if (congratulationsText != null)
        {
            congratulationsText.text = congratulationsTextContent;
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
                Debug.Log("VictorySceneManager: High score detected! Save button shown.");
            }
        }
    }
    
    public void GoToNameEntry()
    {
        Debug.Log("VictorySceneManager: Going to name entry screen");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        SceneManager.LoadScene(nameEntrySceneName);
    }
    
    public void PlayAgain()
    {
        Debug.Log("VictorySceneManager: Starting new game");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("VictorySceneManager: Returning to main menu");
        
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
} 