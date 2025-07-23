using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardPanel;
    public TextMeshProUGUI leaderboardTitle;
    public TextMeshProUGUI leaderboardText;
    public Button closeButton;
    public Button clearButton;
    
    [Header("Layout Settings")]
    public string titleText = "HIGH SCORES";
    public bool showClearButton = true;
    
    [Header("Visual Settings")]
    public Color highlightColor = Color.yellow;
    public bool animateOpen = true;
    public float animationDuration = 0.3f;
    
    private bool isVisible = false;
    private CanvasGroup panelCanvasGroup;
    
    void Start()
    {
        SetupUI();
        SetupButtons();
        
        LeaderboardManager.OnLeaderboardUpdated += OnLeaderboardUpdated;
        
        HideLeaderboard(false);
    }
    
    void OnDestroy()
    {
        LeaderboardManager.OnLeaderboardUpdated -= OnLeaderboardUpdated;
    }
    
    void SetupUI()
    {
        panelCanvasGroup = leaderboardPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = leaderboardPanel.AddComponent<CanvasGroup>();
        }
        
        if (leaderboardTitle != null)
        {
            leaderboardTitle.text = titleText;
        }
        
        if (clearButton != null)
        {
            clearButton.gameObject.SetActive(showClearButton);
        }
        
        AutoFindComponents();
    }
    
    void AutoFindComponents()
    {
        if (leaderboardPanel == null)
        {
            leaderboardPanel = GameObject.Find("LeaderboardPanel");
            if (leaderboardPanel == null)
            {
                Debug.LogError("LeaderboardUI: LeaderboardPanel not found! Please assign it manually.");
                return;
            }
        }
        
        if (leaderboardTitle == null)
        {
            leaderboardTitle = leaderboardPanel.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        Transform titleTransform = leaderboardPanel.transform.Find("Title");
        if (titleTransform != null && leaderboardTitle == null)
        {
            leaderboardTitle = titleTransform.GetComponent<TextMeshProUGUI>();
        }
        
        Transform textTransform = leaderboardPanel.transform.Find("LeaderboardText");
        if (textTransform != null && leaderboardText == null)
        {
            leaderboardText = textTransform.GetComponent<TextMeshProUGUI>();
        }
        
        Transform closeTransform = leaderboardPanel.transform.Find("CloseButton");
        if (closeTransform != null && closeButton == null)
        {
            closeButton = closeTransform.GetComponent<Button>();
        }
        
        Transform clearTransform = leaderboardPanel.transform.Find("ClearButton");
        if (clearTransform != null && clearButton == null)
        {
            clearButton = clearTransform.GetComponent<Button>();
        }
    }
    
    void SetupButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideLeaderboard);
        }
        else
        {
            Debug.LogWarning("LeaderboardUI: Close button not assigned!");
        }
        
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearLeaderboard);
        }
    }
    
    public void ShowLeaderboard()
    {
        if (isVisible) return;
        
        isVisible = true;
        
        UpdateLeaderboardDisplay();
        
        leaderboardPanel.SetActive(true);
        
        if (animateOpen)
        {
            StartCoroutine(AnimatePanel(true));
        }
        else
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        Debug.Log("LeaderboardUI: Leaderboard shown");
    }
    
    public void HideLeaderboard()
    {
        HideLeaderboard(animateOpen);
    }
    
    void HideLeaderboard(bool animate)
    {
        if (!isVisible && !leaderboardPanel.activeInHierarchy) return;
        
        isVisible = false;
        
        if (animate)
        {
            StartCoroutine(AnimatePanel(false));
        }
        else
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
            leaderboardPanel.SetActive(false);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        Debug.Log("LeaderboardUI: Leaderboard hidden");
    }
    
    System.Collections.IEnumerator AnimatePanel(bool show)
    {
        float startAlpha = show ? 0f : 1f;
        float endAlpha = show ? 1f : 0f;
        float timer = 0f;
        
        panelCanvasGroup.interactable = show;
        panelCanvasGroup.blocksRaycasts = show;
        
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / animationDuration;
            
            progress = 1f - Mathf.Pow(1f - progress, 3f);
            
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            
            yield return null;
        }
        
        panelCanvasGroup.alpha = endAlpha;
        
        if (!show)
        {
            leaderboardPanel.SetActive(false);
        }
    }
    
    void UpdateLeaderboardDisplay()
    {
        if (LeaderboardManager.Instance == null)
        {
            if (leaderboardText != null)
            {
                leaderboardText.text = "Leaderboard system not available";
            }
            return;
        }
        
        string leaderboardContent = LeaderboardManager.Instance.GetFormattedLeaderboard();
        
        if (leaderboardText != null)
        {
            leaderboardText.text = leaderboardContent;
        }
        else
        {
            Debug.LogWarning("LeaderboardUI: Leaderboard text component not assigned!");
        }
    }
    
    void OnLeaderboardUpdated(List<LeaderboardEntry> entries)
    {
        if (isVisible)
        {
            UpdateLeaderboardDisplay();
        }
    }
    
    void ClearLeaderboard()
    {
        if (LeaderboardManager.Instance != null)
        {
            Debug.Log("LeaderboardUI: Clearing leaderboard...");
            LeaderboardManager.Instance.ClearLeaderboard();
            
            UpdateLeaderboardDisplay();
        }
    }
    
    public bool IsVisible => isVisible;
    
    public void ToggleLeaderboard()
    {
        if (isVisible)
        {
            HideLeaderboard();
        }
        else
        {
            ShowLeaderboard();
        }
    }
    
    public void HighlightScore(int score)
    {
        if (LeaderboardManager.Instance != null)
        {
            int rank = LeaderboardManager.Instance.GetScoreRank(score);
            Debug.Log($"LeaderboardUI: Player achieved rank {rank} with score {score}");
            
            ShowLeaderboard();
        }
    }
} 