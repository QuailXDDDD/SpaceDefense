using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardRowUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    public Image backgroundImage;
    
    [Header("Settings")]
    public bool showRank = true;
    
    void Awake()
    {
        AutoFindComponents();
    }
    
    void AutoFindComponents()
    {
        Transform rankTransform = transform.Find("RankText");
        if (rankTransform != null && rankText == null)
        {
            rankText = rankTransform.GetComponent<TextMeshProUGUI>();
        }
        
        Transform nameTransform = transform.Find("NameText");
        if (nameTransform != null && nameText == null)
        {
            nameText = nameTransform.GetComponent<TextMeshProUGUI>();
        }
        
        Transform scoreTransform = transform.Find("ScoreText");
        if (scoreTransform != null && scoreText == null)
        {
            scoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
        }
        
        if (nameText == null || scoreText == null)
        {
            TextMeshProUGUI[] textComponents = GetComponentsInChildren<TextMeshProUGUI>();
            Debug.Log($"LeaderboardRowUI: Found {textComponents.Length} text components in children");
            
            if (textComponents.Length >= 2)
            {
                if (nameText == null) nameText = textComponents[0];
                if (scoreText == null) scoreText = textComponents[textComponents.Length - 1];
                if (rankText == null && textComponents.Length >= 3) rankText = textComponents[0];
            }
        }
        
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
        
        Debug.Log($"LeaderboardRowUI: Components found - Rank: {rankText != null}, Name: {nameText != null}, Score: {scoreText != null}, Background: {backgroundImage != null}");
    }
    
    public void SetupRow(int rank, string playerName, int score, Color backgroundColor)
    {
        Debug.Log($"LeaderboardRowUI: Setting up row - Rank: {rank}, Name: {playerName}, Score: {score}");
        
        if (rankText != null && showRank)
        {
            if (rank > 0)
            {
                rankText.text = rank.ToString();
            }
            else
            {
                rankText.text = "-";
            }
            rankText.color = Color.white;
            Debug.Log($"LeaderboardRowUI: Set rank text to: {rankText.text}");
        }
        
        if (nameText != null)
        {
            nameText.text = playerName;
            nameText.color = Color.white;
            Debug.Log($"LeaderboardRowUI: Set name text to: {nameText.text}");
        }
        else
        {
            Debug.LogError("LeaderboardRowUI: NameText component is null!");
        }
        
        if (scoreText != null)
        {
            if (score > 0)
            {
                scoreText.text = score.ToString("N0");
            }
            else
            {
                scoreText.text = "-";
            }
            scoreText.color = Color.white;
            Debug.Log($"LeaderboardRowUI: Set score text to: {scoreText.text}");
        }
        else
        {
            Debug.LogError("LeaderboardRowUI: ScoreText component is null!");
        }
        
        if (backgroundImage != null)
        {
            Color bgColor = backgroundColor;
            bgColor.a = 0.3f;
            backgroundImage.color = bgColor;
        }
    }
    
    public void SetupRowWithDetails(int rank, string playerName, int score, int wave, float survivalTime, Color backgroundColor)
    {
        SetupRow(rank, playerName, score, backgroundColor);
    }
    
    public void HighlightRow(Color highlightColor)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = highlightColor;
        }
    }
} 