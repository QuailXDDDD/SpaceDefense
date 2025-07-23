using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class LeaderboardSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI leaderboardTitle;
    public TextMeshProUGUI leaderboardText;
    public Button backButton;
    public Button clearButton;
    
    [Header("Scene Settings")]
    public string startSceneName = "StartScreen";
    public string titleText = "HIGH SCORES";
    public bool showClearButton = true;
    
    [Header("Visual Settings")]
    public Color titleColor = Color.yellow;
    public Color textColor = Color.white;
    
    void Start()
    {
        SetupUI();
        SetupButtons();
        UpdateLeaderboardDisplay();
        
        LeaderboardManager.OnLeaderboardUpdated += OnLeaderboardUpdated;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log("LeaderboardSceneManager: Leaderboard scene initialized");
    }
    
    void OnDestroy()
    {
        LeaderboardManager.OnLeaderboardUpdated -= OnLeaderboardUpdated;
    }
    
    void SetupUI()
    {
        AutoFindComponents();
        
        if (leaderboardTitle != null)
        {
            leaderboardTitle.text = titleText;
            leaderboardTitle.color = titleColor;
        }
        
        if (leaderboardText != null)
        {
            leaderboardText.color = textColor;
        }
        
        if (clearButton != null)
        {
            clearButton.gameObject.SetActive(showClearButton);
        }
    }
    
    void AutoFindComponents()
    {
        if (leaderboardTitle == null)
        {
            GameObject titleObj = GameObject.Find("LeaderboardTitle");
            if (titleObj != null)
                leaderboardTitle = titleObj.GetComponent<TextMeshProUGUI>();
        }
        
        if (leaderboardText == null)
        {
            GameObject textObj = GameObject.Find("LeaderboardText");
            if (textObj != null)
                leaderboardText = textObj.GetComponent<TextMeshProUGUI>();
        }
        
        if (backButton == null)
        {
            GameObject backObj = GameObject.Find("BackButton");
            if (backObj != null)
                backButton = backObj.GetComponent<Button>();
        }
        
        if (clearButton == null)
        {
            GameObject clearObj = GameObject.Find("ClearButton");
            if (clearObj != null)
                clearButton = clearObj.GetComponent<Button>();
        }
        
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            if (leaderboardTitle == null)
                leaderboardTitle = canvas.GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    
    void SetupButtons()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBackToStartScreen);
            Debug.Log("LeaderboardSceneManager: Back button connected");
        }
        else
        {
            Debug.LogWarning("LeaderboardSceneManager: Back button not found! Please assign it or name it 'BackButton'");
        }
        
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearLeaderboard);
            Debug.Log("LeaderboardSceneManager: Clear button connected");
        }
    }
    
    void UpdateLeaderboardDisplay()
    {
        if (LeaderboardManager.Instance == null)
        {
            if (leaderboardText != null)
            {
                leaderboardText.text = "Leaderboard system not available\n\nPlease ensure LeaderboardManager\nis present in the scene.";
            }
            Debug.LogWarning("LeaderboardSceneManager: LeaderboardManager not found!");
            return;
        }
        
        string leaderboardContent = LeaderboardManager.Instance.GetFormattedLeaderboard();
        
        if (leaderboardText != null)
        {
            leaderboardText.text = leaderboardContent;
        }
        else
        {
            Debug.LogWarning("LeaderboardSceneManager: Leaderboard text component not assigned!");
        }
        
        Debug.Log("LeaderboardSceneManager: Leaderboard display updated");
    }
    
    void OnLeaderboardUpdated(List<LeaderboardEntry> entries)
    {
        UpdateLeaderboardDisplay();
    }
    
    public void GoBackToStartScreen()
    {
        Debug.Log("LeaderboardSceneManager: Returning to start screen - " + startSceneName);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        SceneManager.LoadScene(startSceneName);
    }
    
    public void ClearLeaderboard()
    {
        if (LeaderboardManager.Instance != null)
        {
            Debug.Log("LeaderboardSceneManager: Clearing leaderboard...");
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPowerUp();
            }
            
            LeaderboardManager.Instance.ClearLeaderboard();
        }
        else
        {
            Debug.LogWarning("LeaderboardSceneManager: Cannot clear leaderboard - LeaderboardManager not found!");
        }
    }
    
    [ContextMenu("Refresh Leaderboard Display")]
    public void RefreshDisplay()
    {
        UpdateLeaderboardDisplay();
    }
    
    [ContextMenu("Add Test Score")]
    public void AddTestScore()
    {
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.AddTestScore();
        }
    }
} 