using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LeaderboardTableUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public Transform tableHeadersParent;
    public Transform tableContentParent;
    public GameObject leaderboardRowPrefab;
    public Button backButton;
    public Button clearButton;
    public ScrollRect scrollRect;
    
    [Header("Table Headers")]
    public TextMeshProUGUI nameHeaderText;
    public TextMeshProUGUI scoreHeaderText;
    
    [Header("Settings")]
    public string titleString = "HIGH SCORES";
    public int maxDisplayEntries = 10;
    public Color headerColor = Color.yellow;
    public Color evenRowColor = Color.white;
    public Color oddRowColor = Color.gray;
    
    [Header("Scene Navigation")]
    public string backSceneName = "StartScreen";
    
    private List<GameObject> currentRows = new List<GameObject>();
    
    void Start()
    {
        SetupUI();
        SetupButtons();
        
        StartCoroutine(InitializeLeaderboardWithDelay());
        
        LeaderboardManager.OnLeaderboardUpdated += OnLeaderboardUpdated;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log("LeaderboardTableUI: Table-based leaderboard initialized");
    }
    
    IEnumerator InitializeLeaderboardWithDelay()
    {
        yield return null;
        
        Debug.Log("LeaderboardTableUI: Populating leaderboard after delay");
        PopulateLeaderboard();
    }
    
    void OnDestroy()
    {
        LeaderboardManager.OnLeaderboardUpdated -= OnLeaderboardUpdated;
    }
    
    void SetupUI()
    {
        if (titleText != null)
        {
            titleText.text = titleString;
        }
        
        if (nameHeaderText != null)
        {
            nameHeaderText.text = "NAME";
            nameHeaderText.color = headerColor;
        }
        
        if (scoreHeaderText != null)
        {
            scoreHeaderText.text = "SCORE";
            scoreHeaderText.color = headerColor;
        }
        
        SetupTableContentParent();
        
        AutoFindComponents();
    }
    
    void SetupTableContentParent()
    {
        if (tableContentParent == null) return;
        
        Debug.Log("LeaderboardTableUI: Setting up table content parent");
        
        VerticalLayoutGroup verticalLayout = tableContentParent.GetComponent<VerticalLayoutGroup>();
        if (verticalLayout == null)
        {
            verticalLayout = tableContentParent.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        
        verticalLayout.childControlWidth = true;
        verticalLayout.childControlHeight = false;
        verticalLayout.childForceExpandWidth = true;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.spacing = 5f;
        verticalLayout.padding = new RectOffset(10, 10, 10, 10);
        
        ContentSizeFitter sizeFitter = tableContentParent.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = tableContentParent.gameObject.AddComponent<ContentSizeFitter>();
        }
        
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        Debug.Log("LeaderboardTableUI: Table content parent configured");
    }
    
    void AutoFindComponents()
    {
        if (titleText == null)
        {
            GameObject titleObj = GameObject.Find("LeaderboardTitle");
            if (titleObj != null)
                titleText = titleObj.GetComponent<TextMeshProUGUI>();
        }
        
        if (tableContentParent == null)
        {
            GameObject contentObj = GameObject.Find("TableContent");
            if (contentObj != null)
                tableContentParent = contentObj.transform;
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
    }
    
    void SetupButtons()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }
        
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearLeaderboard);
        }
    }
    
    void PopulateLeaderboard()
    {
        ClearTableRows();
        
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogWarning("LeaderboardTableUI: LeaderboardManager not found!");
            CreateSimpleTextRow("LeaderboardManager not found!");
            return;
        }
        
        List<LeaderboardEntry> entries = LeaderboardManager.Instance.GetTopScores(maxDisplayEntries);
        
        Debug.Log($"LeaderboardTableUI: Retrieved {entries.Count} leaderboard entries");
        
        if (entries.Count == 0)
        {
            CreateSimpleTextRow("No scores yet! Play the game to set records!");
            return;
        }
        
        for (int i = 0; i < entries.Count; i++)
        {
            Debug.Log($"LeaderboardTableUI: Entry {i}: {entries[i].playerName} - {entries[i].score}");
        }
        
        if (leaderboardRowPrefab != null)
        {
            Debug.Log("LeaderboardTableUI: Using prefab to create rows");
            for (int i = 0; i < entries.Count; i++)
            {
                CreateLeaderboardRow(i + 1, entries[i], i % 2 == 0);
            }
        }
        else
        {
            Debug.Log("LeaderboardTableUI: No prefab assigned, using simple text rows");
            for (int i = 0; i < entries.Count; i++)
            {
                string rowText = $"{i + 1}. {entries[i].playerName} - {entries[i].score:N0} points";
                CreateSimpleTextRow(rowText);
            }
        }
        
        ForceLayoutRebuild();
        
        Debug.Log($"LeaderboardTableUI: Created {currentRows.Count} total rows");
    }
    
    void CreateLeaderboardRow(int rank, LeaderboardEntry entry, bool isEvenRow)
    {
        if (leaderboardRowPrefab == null || tableContentParent == null)
        {
            Debug.LogError("LeaderboardTableUI: Row prefab or content parent not assigned!");
            return;
        }
        
        // Instantiate row
        GameObject rowObj = Instantiate(leaderboardRowPrefab, tableContentParent);
        currentRows.Add(rowObj);
        
        // Get row component
        LeaderboardRowUI rowUI = rowObj.GetComponent<LeaderboardRowUI>();
        if (rowUI != null)
        {
            Color rowColor = isEvenRow ? evenRowColor : oddRowColor;
            rowUI.SetupRow(rank, entry.playerName, entry.score, rowColor);
        }
        else
        {
            Debug.LogError("LeaderboardTableUI: Row prefab missing LeaderboardRowUI component!");
            
            SetupRowManually(rowObj, rank, entry.playerName, entry.score);
        }
    }
    
    void SetupRowManually(GameObject rowObj, int rank, string playerName, int score)
    {
        TextMeshProUGUI[] textComponents = rowObj.GetComponentsInChildren<TextMeshProUGUI>();
        
        if (textComponents.Length >= 2)
        {
            textComponents[0].text = playerName;
            textComponents[0].color = Color.white;
            
            textComponents[textComponents.Length - 1].text = score.ToString("N0");
            textComponents[textComponents.Length - 1].color = Color.white;
            
            Debug.Log($"LeaderboardTableUI: Manually set up row - {playerName}: {score}");
        }
        
        Image bgImage = rowObj.GetComponent<Image>();
        if (bgImage != null)
        {
            Color bgColor = Color.clear;
            bgImage.color = bgColor;
        }
    }
    
    void CreateNoDataRow()
    {
        if (leaderboardRowPrefab == null || tableContentParent == null) 
        {
            CreateSimpleTextRow("No scores yet! Play the game to set records!");
            return;
        }
        
        GameObject rowObj = Instantiate(leaderboardRowPrefab, tableContentParent);
        currentRows.Add(rowObj);
        
        LeaderboardRowUI rowUI = rowObj.GetComponent<LeaderboardRowUI>();
        if (rowUI != null)
        {
            rowUI.SetupRow(0, "No scores yet!", 0, evenRowColor);
        }
    }
    
    void CreateSimpleTextRow(string message)
    {
        if (tableContentParent == null)
        {
            Debug.LogError("LeaderboardTableUI: Table content parent is null!");
            return;
        }
        
        GameObject textObj = new GameObject("LeaderboardEntry_" + currentRows.Count);
        textObj.transform.SetParent(tableContentParent, false);
        
        currentRows.Add(textObj);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.color = Color.white;
        text.fontSize = 20;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(0, 35);
        
        LayoutElement layout = textObj.AddComponent<LayoutElement>();
        layout.preferredHeight = 35;
        layout.minHeight = 35;
        layout.flexibleWidth = 1;
        
        Debug.Log($"LeaderboardTableUI: Created simple text row {currentRows.Count}: {message}");
    }
    
    [ContextMenu("Test Simple Rows")]
    public void TestSimpleRows()
    {
        ClearTableRows();
        CreateSimpleTextRow("PILOT - 1,500 points");
        CreateSimpleTextRow("ACE - 3,200 points");
        CreateSimpleTextRow("COMMANDER - 5,000 points");
    }
    
    [ContextMenu("Create Test Entries")]
    public void CreateTestEntries()
    {
        Debug.Log("LeaderboardTableUI: Creating test entries manually");
        
        ClearTableRows();
        
        ForceLayoutRebuild();
        
        Debug.Log($"LeaderboardTableUI: Test entries created. Total rows: {currentRows.Count}");
    }
    
    void ForceLayoutRebuild()
    {
        if (tableContentParent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tableContentParent.GetComponent<RectTransform>());
            
            if (tableContentParent.parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(tableContentParent.parent.GetComponent<RectTransform>());
            }
            
            Debug.Log("LeaderboardTableUI: Layout rebuild forced");
        }
    }
    
    void ClearTableRows()
    {
        foreach (GameObject row in currentRows)
        {
            if (row != null)
            {
                Destroy(row);
            }
        }
        currentRows.Clear();
        
        if (tableContentParent != null)
        {
            for (int i = tableContentParent.childCount - 1; i >= 0; i--)
            {
                Transform child = tableContentParent.GetChild(i);
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        Debug.Log("LeaderboardTableUI: All table rows and children cleared");
    }
    
    [ContextMenu("Clear All Rows")]
    public void ClearAllRows()
    {
        ClearTableRows();
    }
    
    void OnLeaderboardUpdated(List<LeaderboardEntry> entries)
    {
        PopulateLeaderboard();
    }
    
    public void GoBack()
    {
        Debug.Log("LeaderboardTableUI: Going back to " + backSceneName);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp();
        }
        
        SceneManager.LoadScene(backSceneName);
    }
    
    public void ClearLeaderboard()
    {
        if (LeaderboardManager.Instance != null)
        {
            Debug.Log("LeaderboardTableUI: Clearing leaderboard...");
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPowerUp();
            }
            
            LeaderboardManager.Instance.ClearLeaderboard();
        }
    }
    
    [ContextMenu("Refresh Leaderboard")]
    public void RefreshLeaderboard()
    {
        Debug.Log("LeaderboardTableUI: Manual refresh triggered");
        PopulateLeaderboard();
    }
    
    [ContextMenu("Force Simple Text Mode")]
    public void ForceSimpleTextMode()
    {
        Debug.Log("LeaderboardTableUI: Forcing simple text mode");
        
        GameObject tempPrefab = leaderboardRowPrefab;
        leaderboardRowPrefab = null;
        
        PopulateLeaderboard();
        
        leaderboardRowPrefab = tempPrefab;
    }

    [ContextMenu("Clear All Leaderboard Data")]
    public void ClearAllLeaderboardData()
    {
        Debug.Log("LeaderboardTableUI: Clearing all leaderboard data");
        
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.ClearLeaderboard();
            Debug.Log("LeaderboardTableUI: Leaderboard data cleared");
        }
        else
        {
            Debug.LogWarning("LeaderboardTableUI: LeaderboardManager not found");
        }
        
        ClearTableRows();
        
        CreateSimpleTextRow("No scores yet! Play the game to set records!");
        
        ForceLayoutRebuild();
        
        Debug.Log("LeaderboardTableUI: Display cleared and updated");
    }

    [ContextMenu("COMPLETE RESET - Delete All Saved Scores")]
    public void CompleteResetFromTableUI()
    {
        Debug.Log("LeaderboardTableUI: Performing complete reset of all leaderboard data");
        
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.CompleteReset();
        }
        else
        {
            Debug.LogWarning("LeaderboardTableUI: LeaderboardManager not found, clearing PlayerPrefs directly");
            
            string LEADERBOARD_KEY = "SpaceDefense_Leaderboard";
            string ENTRY_COUNT_KEY = "SpaceDefense_EntryCount";
            
            for (int i = 0; i < 50; i++)
            {
                string entryKey = LEADERBOARD_KEY + "_Entry_" + i;
                if (PlayerPrefs.HasKey(entryKey))
                {
                    PlayerPrefs.DeleteKey(entryKey);
                    Debug.Log($"Deleted PlayerPrefs key: {entryKey}");
                }
            }
            PlayerPrefs.DeleteKey(ENTRY_COUNT_KEY);
            PlayerPrefs.Save();
            
            Debug.Log("LeaderboardTableUI: PlayerPrefs cleared directly");
        }
        
        ClearTableRows();
        CreateSimpleTextRow("All scores cleared! Play the game to set new records!");
        ForceLayoutRebuild();
        
        Debug.Log("LeaderboardTableUI: Complete reset finished");
    }
} 