using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }
    
    [Header("Scene Settings")]
    public string victorySceneName = "VictoryScene";
    public string gameOverSceneName = "GameOverScene";
    public string startSceneName = "StartScreen";
    
    [Header("Game Over Settings")]
    public float sceneTransitionDelay = 2f;
    
    private bool gameIsOver = false;
    private float gameStartTime;
    private int finalScore;
    private int finalWave;
    private float survivalTime;
    private bool isVictory = false;
    
    public static int LastFinalScore { get; private set; }
    public static int LastFinalWave { get; private set; }
    public static float LastSurvivalTime { get; private set; }
    public static bool LastWasVictory { get; private set; }
    
    public static System.Action OnGameOver;
    public static System.Action OnScoreSaved;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (GameUI.Instance != null)
        {
            GameUI.Instance.ResetForNewGame();
        }
        
        StartCoroutine(ResetHeartUIAfterDelay());
        
        gameStartTime = Time.time;
        
        PlayerShip.OnPlayerReady += OnPlayerReady;
        WaveManager.OnAllWavesCompleted += OnAllWavesCompleted;
        
        Debug.Log("GameOverManager: Initialized for scene-based game over handling");
    }
    
    IEnumerator ResetHeartUIAfterDelay()
    {
        yield return null;
        
        if (HeartUI.Instance != null)
        {
            HeartUI.Instance.ResetForNewGame();
        }
    }
    
    void OnDestroy()
    {
        PlayerShip.OnPlayerReady -= OnPlayerReady;
        WaveManager.OnAllWavesCompleted -= OnAllWavesCompleted;
    }
    
    void OnPlayerReady()
    {
        gameStartTime = Time.time;
        gameIsOver = false;
    }
    
    void OnAllWavesCompleted()
    {
        TriggerGameOver(true);
    }
    
    public void TriggerGameOver(bool victory = false)
    {
        if (gameIsOver) return;
        
        gameIsOver = true;
        isVictory = victory;
        
        CalculateFinalStats();
        
        LastFinalScore = finalScore;
        LastFinalWave = finalWave;
        LastSurvivalTime = survivalTime;
        LastWasVictory = isVictory;
        
        OnGameOver?.Invoke();
        
        if (AudioManager.Instance != null)
        {
            if (isVictory)
            {
                AudioManager.Instance.PlayPowerUp();
            }
            else
            {
                AudioManager.Instance.PlayPlayerExplosion();
            }
        }
        
        Debug.Log($"GameOverManager: Game over triggered. Victory: {isVictory}, Final Score: {finalScore}");
        
        StartCoroutine(TransitionToGameOverScene());
    }
    
    void CalculateFinalStats()
    {
        if (GameUI.Instance != null)
        {
            finalScore = GameUI.Instance.GetCurrentScore();
            finalWave = GameUI.Instance.GetCurrentWave();
        }
        else
        {
            finalScore = 0;
            finalWave = 1;
        }
        
        survivalTime = Time.time - gameStartTime;
        
        Debug.Log($"GameOverManager: Final stats - Score: {finalScore}, Wave: {finalWave}, Time: {survivalTime:F1}s");
    }
    
    IEnumerator TransitionToGameOverScene()
    {
        yield return new WaitForSeconds(sceneTransitionDelay);
        
        string sceneToLoad = isVictory ? victorySceneName : gameOverSceneName;
        
        Debug.Log($"GameOverManager: Loading {(isVictory ? "victory" : "game over")} scene - {sceneToLoad}");
        
        SceneManager.LoadScene(sceneToLoad);
    }
    
    public bool IsGameOver => gameIsOver;
    public bool IsVictory => isVictory;
    public int GetFinalScore() => finalScore;
    public int GetFinalWave() => finalWave;
    public float GetSurvivalTime() => survivalTime;
    
    public static void PlayerDied()
    {
        if (Instance != null)
        {
            Instance.TriggerGameOver(false);
        }
    }
    
    [ContextMenu("Trigger Game Over")]
    public void TriggerGameOverDebug()
    {
        TriggerGameOver(false);
    }
    
    [ContextMenu("Trigger Victory")]
    public void TriggerVictoryDebug()
    {
        TriggerGameOver(true);
    }
} 