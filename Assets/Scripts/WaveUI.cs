using UnityEngine;
using UnityEngine.UI;

public class WaveUI : MonoBehaviour
{
    [Header("UI References")]
    public Text waveInfoText;
    public Text enemyCountText;
    public Text waveProgressText;
    
    [Header("UI Settings")]
    public bool showDebugInfo = true;
    
    private WaveManager waveManager;
    
    void Start()
    {
        waveManager = FindFirstObjectByType<WaveManager>();
        
        if (waveManager == null)
        {
            Debug.LogError("WaveUI: WaveManager not found in scene!");
            enabled = false;
            return;
        }
        
        WaveManager.OnWaveStarted += OnWaveStarted;
        WaveManager.OnWaveCompleted += OnWaveCompleted;
        WaveManager.OnAllWavesCompleted += OnAllWavesCompleted;
        
        UpdateUI();
    }
    
    void OnDestroy()
    {
        WaveManager.OnWaveStarted -= OnWaveStarted;
        WaveManager.OnWaveCompleted -= OnWaveCompleted;
        WaveManager.OnAllWavesCompleted -= OnAllWavesCompleted;
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (waveManager == null) return;
        
        if (waveInfoText != null)
        {
            if (waveManager.waveInProgress)
            {
                waveInfoText.text = $"Wave {waveManager.currentWave}";
            }
            else if (waveManager.currentWave >= 3)
            {
                waveInfoText.text = "Victory!";
            }
            else
            {
                waveInfoText.text = "Get Ready...";
            }
        }
        
        if (enemyCountText != null)
        {
            if (waveManager.waveInProgress)
            {
                int enemyCount = waveManager.activeEnemies.Count;
                enemyCountText.text = $"Enemies: {enemyCount}";
            }
            else
            {
                enemyCountText.text = "";
            }
        }
        
        if (waveProgressText != null)
        {
            string progressText = GetWaveDescription();
            waveProgressText.text = progressText;
        }
    }
    
    string GetWaveDescription()
    {
        if (waveManager.currentWave == 0)
        {
            return "Preparing Wave 1...";
        }
        else if (waveManager.waveInProgress)
        {
            switch (waveManager.currentWave)
            {
                case 1:
                    return "Straight Line Formation";
                case 2:
                    return "Grid Formation - 2 Rows!";
                case 3:
                    return "ZigZag Formation";
                case 4:
                    return "Circle Formation - BOSS WAVE!";
                default:
                    return "Unknown Wave";
            }
        }
        else if (waveManager.currentWave >= 4)
        {
            return "All Waves Complete!";
        }
        else
        {
            return $"Wave {waveManager.currentWave} Complete! Next wave incoming...";
        }
    }
    
    void OnWaveStarted(int waveNumber)
    {
        Debug.Log($"WaveUI: Wave {waveNumber} started!");
        
        if (waveNumber == 3)
        {
            StartCoroutine(ShowBossWaveWarning());
        }
    }
    
    void OnWaveCompleted(int waveNumber)
    {
        Debug.Log($"WaveUI: Wave {waveNumber} completed!");
        
        StartCoroutine(ShowWaveCompleteMessage(waveNumber));
    }
    
    void OnAllWavesCompleted()
    {
        Debug.Log("WaveUI: All waves completed!");
        
        StartCoroutine(ShowVictoryMessage());
    }
    
    System.Collections.IEnumerator ShowBossWaveWarning()
    {
        if (waveProgressText != null)
        {
            string originalText = waveProgressText.text;
            Color originalColor = waveProgressText.color;
            
            for (int i = 0; i < 3; i++)
            {
                waveProgressText.text = "⚠️ BOSS WAVE! ⚠️";
                waveProgressText.color = Color.red;
                yield return new WaitForSeconds(0.5f);
                
                waveProgressText.text = "";
                yield return new WaitForSeconds(0.3f);
            }
            
            waveProgressText.color = originalColor;
        }
    }
    
    System.Collections.IEnumerator ShowWaveCompleteMessage(int waveNumber)
    {
        if (waveInfoText != null)
        {
            string originalText = waveInfoText.text;
            Color originalColor = waveInfoText.color;
            
            waveInfoText.text = $"Wave {waveNumber} Complete!";
            waveInfoText.color = Color.green;
            
            yield return new WaitForSeconds(2f);
            
            waveInfoText.color = originalColor;
        }
    }
    
    System.Collections.IEnumerator ShowVictoryMessage()
    {
        if (waveInfoText != null)
        {
            Color originalColor = waveInfoText.color;
            
            for (int i = 0; i < 5; i++)
            {
                waveInfoText.color = Color.yellow;
                yield return new WaitForSeconds(0.3f);
                waveInfoText.color = Color.green;
                yield return new WaitForSeconds(0.3f);
            }
            
            waveInfoText.color = originalColor;
        }
    }
    
    public void ForceUpdateUI()
    {
        UpdateUI();
    }
    
    public void ShowCustomMessage(string message, float duration = 2f)
    {
        if (waveProgressText != null)
        {
            StartCoroutine(ShowCustomMessageCoroutine(message, duration));
        }
    }
    
    System.Collections.IEnumerator ShowCustomMessageCoroutine(string message, float duration)
    {
        if (waveProgressText != null)
        {
            string originalText = waveProgressText.text;
            waveProgressText.text = message;
            yield return new WaitForSeconds(duration);
            waveProgressText.text = originalText;
        }
    }
} 