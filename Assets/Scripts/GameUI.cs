using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;
    
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI waveText;
    
    [Header("Score Settings")]
    public string scorePrefix = "Score: ";
    public string wavePrefix = "Wave: ";
    
    private int currentScore = 0;
    private int currentWave = 1;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        UpdateAllUI();
    }
    
    public void UpdateScore(int newScore)
    {
        currentScore = newScore;
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + currentScore.ToString();
        }
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScore(currentScore);
        Debug.Log($"GameUI: Score increased by {points}. Total: {currentScore}");
    }
    
    public void UpdateWave(int newWave)
    {
        currentWave = newWave;
        if (waveText != null)
        {
            waveText.text = wavePrefix + newWave.ToString();
        }
    }
    
    public void UpdateAllUI()
    {
        UpdateScore(currentScore);
        UpdateWave(currentWave);
    }
    
    public int GetCurrentScore() => currentScore;
    public int GetCurrentWave() => currentWave;
    
    public void ResetForNewGame()
    {
        currentScore = 0;
        currentWave = 1;
        UpdateAllUI();
        Debug.Log("GameUI: Game state reset for new game");
    }
} 