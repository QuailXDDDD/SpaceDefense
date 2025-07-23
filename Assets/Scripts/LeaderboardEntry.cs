using System;
using UnityEngine;

[System.Serializable]
public class LeaderboardEntry : IComparable<LeaderboardEntry>
{
    public string playerName;
    public int score;
    public int waveReached;
    public string dateTime;
    public float survivalTime;
    
    public LeaderboardEntry(string name, int playerScore, int wave, float time)
    {
        playerName = name;
        score = playerScore;
        waveReached = wave;
        survivalTime = time;
        dateTime = System.DateTime.Now.ToString("MM/dd/yyyy");
    }
    
    public int CompareTo(LeaderboardEntry other)
    {
        if (other == null) return 1;
        
        int scoreComparison = other.score.CompareTo(this.score);
        if (scoreComparison != 0) return scoreComparison;
        
        int waveComparison = other.waveReached.CompareTo(this.waveReached);
        if (waveComparison != 0) return waveComparison;
        
        return other.survivalTime.CompareTo(this.survivalTime);
    }
    
    public string GetFormattedEntry(int rank)
    {
        return $"{rank}. {playerName} - {score:N0} pts (Wave {waveReached})";
    }
    
    public string GetDetailedEntry(int rank)
    {
        return $"{rank}. {playerName}\nScore: {score:N0} | Wave: {waveReached} | Time: {survivalTime:F1}s\n{dateTime}";
    }
} 