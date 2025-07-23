using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }
    
    [Header("Leaderboard Settings")]
    public int maxEntries = 10;
    public string defaultPlayerName = "Anonymous";
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    private List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>();
    private const string LEADERBOARD_KEY = "SpaceDefense_Leaderboard";
    private const string ENTRY_COUNT_KEY = "SpaceDefense_EntryCount";
    
    public static System.Action<List<LeaderboardEntry>> OnLeaderboardUpdated;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLeaderboard();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        Debug.Log("LeaderboardManager: Started with clean leaderboard");
    }
    
    [ContextMenu("Clear All Data and Disable Samples")]
    public void ClearAllDataAndDisableSamples()
    {
        Debug.Log("LeaderboardManager: Clearing all data and disabling sample entries");
        
        ClearLeaderboard();
        
        enableDebugLogs = false;
        
        Debug.Log("LeaderboardManager: All data cleared, sample entries disabled");
    }
    
    [ContextMenu("Complete Reset - Delete All Saved Data")]
    public void CompleteReset()
    {
        Debug.Log("LeaderboardManager: Performing complete reset - deleting all saved data");
        
        leaderboardEntries.Clear();
        
        int entryCount = PlayerPrefs.GetInt(ENTRY_COUNT_KEY, 0);
        for (int i = 0; i < 50; i++)
        {
            string entryKey = LEADERBOARD_KEY + "_Entry_" + i;
            if (PlayerPrefs.HasKey(entryKey))
            {
                PlayerPrefs.DeleteKey(entryKey);
            }
        }
        PlayerPrefs.DeleteKey(ENTRY_COUNT_KEY);
        PlayerPrefs.Save();
        
        enableDebugLogs = false;
        
        OnLeaderboardUpdated?.Invoke(leaderboardEntries);
        
        Debug.Log("LeaderboardManager: Complete reset finished - all data deleted");
    }
    
    public void AddScore(string playerName, int score, int waveReached, float survivalTime)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = defaultPlayerName;
        }
        
        LeaderboardEntry newEntry = new LeaderboardEntry(playerName, score, waveReached, survivalTime);
        
        leaderboardEntries.Add(newEntry);
        
        SortAndTrimLeaderboard();
        
        SaveLeaderboard();
        
        OnLeaderboardUpdated?.Invoke(leaderboardEntries);
        
        if (enableDebugLogs)
        {
            Debug.Log($"LeaderboardManager: Added score - {playerName}: {score} points (Wave {waveReached})");
        }
    }
    
    public bool IsHighScore(int score)
    {
        if (leaderboardEntries.Count < maxEntries)
        {
            return true;
        }
        
        return score > leaderboardEntries[leaderboardEntries.Count - 1].score;
    }
    
    public int GetScoreRank(int score)
    {
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            if (score >= leaderboardEntries[i].score)
            {
                return i + 1;
            }
        }
        return leaderboardEntries.Count + 1;
    }
    
    public List<LeaderboardEntry> GetTopScores(int count = -1)
    {
        if (count <= 0) count = maxEntries;
        return leaderboardEntries.Take(count).ToList();
    }
    
    public string GetFormattedLeaderboard(int count = -1)
    {
        var topScores = GetTopScores(count);
        if (topScores.Count == 0)
        {
            return "No scores yet!\nPlay the game to set your first record!";
        }
        
        string result = "=== HIGH SCORES ===\n\n";
        for (int i = 0; i < topScores.Count; i++)
        {
            result += topScores[i].GetFormattedEntry(i + 1) + "\n";
        }
        
        return result.TrimEnd();
    }
    
    void SortAndTrimLeaderboard()
    {
        leaderboardEntries.Sort();
        
        if (leaderboardEntries.Count > maxEntries)
        {
            leaderboardEntries = leaderboardEntries.Take(maxEntries).ToList();
        }
    }
    
    void SaveLeaderboard()
    {
        try
        {
            PlayerPrefs.SetInt(ENTRY_COUNT_KEY, leaderboardEntries.Count);
            
            for (int i = 0; i < leaderboardEntries.Count; i++)
            {
                var entry = leaderboardEntries[i];
                string entryKey = LEADERBOARD_KEY + "_Entry_" + i;
                
                string entryData = $"{entry.playerName}|{entry.score}|{entry.waveReached}|{entry.survivalTime}|{entry.dateTime}";
                PlayerPrefs.SetString(entryKey, entryData);
            }
            
            PlayerPrefs.Save();
            
            if (enableDebugLogs)
            {
                Debug.Log($"LeaderboardManager: Saved {leaderboardEntries.Count} entries to PlayerPrefs");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LeaderboardManager: Failed to save leaderboard - {e.Message}");
        }
    }
    
    void LoadLeaderboard()
    {
        try
        {
            leaderboardEntries.Clear();
            
            int entryCount = PlayerPrefs.GetInt(ENTRY_COUNT_KEY, 0);
            
            for (int i = 0; i < entryCount; i++)
            {
                string entryKey = LEADERBOARD_KEY + "_Entry_" + i;
                string entryData = PlayerPrefs.GetString(entryKey, "");
                
                if (!string.IsNullOrEmpty(entryData))
                {
                    string[] parts = entryData.Split('|');
                    if (parts.Length >= 5)
                    {
                        string playerName = parts[0];
                        int score = int.Parse(parts[1]);
                        int wave = int.Parse(parts[2]);
                        float time = float.Parse(parts[3]);
                        string date = parts[4];
                        
                        var entry = new LeaderboardEntry(playerName, score, wave, time);
                        entry.dateTime = date;
                        leaderboardEntries.Add(entry);
                    }
                }
            }
            
            SortAndTrimLeaderboard();
            
            if (enableDebugLogs)
            {
                Debug.Log($"LeaderboardManager: Loaded {leaderboardEntries.Count} entries from PlayerPrefs");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LeaderboardManager: Failed to load leaderboard - {e.Message}");
            leaderboardEntries.Clear();
        }
    }
    
    public void ClearLeaderboard()
    {
        leaderboardEntries.Clear();
        
        int entryCount = PlayerPrefs.GetInt(ENTRY_COUNT_KEY, 0);
        for (int i = 0; i < entryCount; i++)
        {
            string entryKey = LEADERBOARD_KEY + "_Entry_" + i;
            PlayerPrefs.DeleteKey(entryKey);
        }
        PlayerPrefs.DeleteKey(ENTRY_COUNT_KEY);
        PlayerPrefs.Save();
        
        OnLeaderboardUpdated?.Invoke(leaderboardEntries);
        
        Debug.Log("LeaderboardManager: Leaderboard cleared");
    }
    
    void CreateSampleEntries()
    {
        AddScore("ACE PILOT", 15000, 4, 185.5f);
        AddScore("COMMANDER", 12500, 4, 160.2f);
        AddScore("CAPTAIN", 10000, 3, 145.8f);
        AddScore("LIEUTENANT", 8500, 3, 125.4f);
        AddScore("SERGEANT", 6000, 2, 95.2f);
        AddScore("ROOKIE", 3500, 2, 75.1f);
        AddScore("CADET", 1500, 1, 45.8f);
        
        Debug.Log("LeaderboardManager: Created sample leaderboard entries");
    }
    
    [ContextMenu("Add Test Score")]
    public void AddTestScore()
    {
        int randomScore = Random.Range(1000, 20000);
        int randomWave = Random.Range(1, 5);
        float randomTime = Random.Range(30f, 200f);
        AddScore("TEST PLAYER", randomScore, randomWave, randomTime);
    }
    
    [ContextMenu("Clear All Scores")]
    public void ClearAllScores()
    {
        ClearLeaderboard();
    }
    
    [ContextMenu("Print Leaderboard")]
    public void PrintLeaderboard()
    {
        Debug.Log(GetFormattedLeaderboard());
    }
} 