using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Audio Sources")]
    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource;
    
    [Header("Player Sound Effects")]
    public AudioClip playerShootSound;
    public AudioClip playerExplosionSound;
    public AudioClip playerHitSound;
    
    [Header("Enemy Sound Effects")]
    public AudioClip enemyShootSound;
    public AudioClip enemyExplosionSound;
    public AudioClip enemyHitSound;
    
    [Header("Boss Sound Effects")]
    public AudioClip bossShootSound;
    public AudioClip bossExplosionSound;
    public AudioClip bossHitSound;
    
    [Header("General Sound Effects")]
    public AudioClip projectileHitSound;
    public AudioClip powerUpSound;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    [Header("Individual Sound Effect Volumes")]
    [Range(0f, 1f)]
    public float playerShootVolume = 1f;
    [Range(0f, 1f)]
    public float playerExplosionVolume = 1f;
    [Range(0f, 1f)]
    public float playerHitVolume = 1f;
    
    [Range(0f, 1f)]
    public float enemyShootVolume = 1f;
    [Range(0f, 1f)]
    public float enemyExplosionVolume = 1f;
    [Range(0f, 1f)]
    public float enemyHitVolume = 1f;
    
    [Range(0f, 1f)]
    public float bossShootVolume = 1f;
    [Range(0f, 1f)]
    public float bossExplosionVolume = 1f;
    [Range(0f, 1f)]
    public float bossHitVolume = 1f;
    
    [Range(0f, 1f)]
    public float projectileHitVolume = 1f;
    [Range(0f, 1f)]
    public float powerUpVolume = 1f;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeAudioSources()
    {
        // Create audio sources if they don't exist
        if (musicAudioSource == null)
        {
            GameObject musicSource = new GameObject("MusicAudioSource");
            musicSource.transform.SetParent(transform);
            musicAudioSource = musicSource.AddComponent<AudioSource>();
            musicAudioSource.loop = true;
            musicAudioSource.playOnAwake = false;
        }
        
        if (sfxAudioSource == null)
        {
            GameObject sfxSource = new GameObject("SFXAudioSource");
            sfxSource.transform.SetParent(transform);
            sfxAudioSource = sfxSource.AddComponent<AudioSource>();
            sfxAudioSource.loop = false;
            sfxAudioSource.playOnAwake = false;
        }
        
        UpdateVolume();
    }
    
    void Update()
    {
        UpdateVolume();
    }
    
    void UpdateVolume()
    {
        if (musicAudioSource != null)
            musicAudioSource.volume = masterVolume * musicVolume;
        
        if (sfxAudioSource != null)
            sfxAudioSource.volume = masterVolume * sfxVolume;
    }
    
    // Play sound effect methods
    public void PlayPlayerShoot()
    {
        PlaySFX(playerShootSound, playerShootVolume);
    }
    
    public void PlayPlayerExplosion()
    {
        PlaySFX(playerExplosionSound, playerExplosionVolume);
    }
    
    public void PlayPlayerHit()
    {
        PlaySFX(playerHitSound, playerHitVolume);
    }
    
    public void PlayEnemyShoot()
    {
        PlaySFX(enemyShootSound, enemyShootVolume);
    }
    
    public void PlayEnemyExplosion()
    {
        PlaySFX(enemyExplosionSound, enemyExplosionVolume);
    }
    
    public void PlayEnemyHit()
    {
        PlaySFX(enemyHitSound, enemyHitVolume);
    }
    
    public void PlayBossShoot()
    {
        PlaySFX(bossShootSound, bossShootVolume);
    }
    
    public void PlayBossExplosion()
    {
        PlaySFX(bossExplosionSound, bossExplosionVolume);
    }
    
    public void PlayBossHit()
    {
        PlaySFX(bossHitSound, bossHitVolume);
    }
    
    public void PlayProjectileHit()
    {
        PlaySFX(projectileHitSound, projectileHitVolume);
    }
    
    public void PlayPowerUp()
    {
        PlaySFX(powerUpSound, powerUpVolume);
    }
    
    // Generic method to play any sound effect with individual volume
    public void PlaySFX(AudioClip clip, float individualVolume = 1f)
    {
        if (clip != null && sfxAudioSource != null)
        {
            float finalVolume = masterVolume * sfxVolume * individualVolume;
            sfxAudioSource.PlayOneShot(clip, finalVolume);
        }
    }
    
    // Overloaded method for backward compatibility
    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, 1f);
    }
    
    // Music control methods
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip != null && musicAudioSource != null)
        {
            musicAudioSource.clip = musicClip;
            musicAudioSource.Play();
        }
    }
    
    public void StopMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
        }
    }
    
    public void PauseMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.UnPause();
        }
    }
    
    // Volume control methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
    
    // Individual volume control methods
    public void SetPlayerShootVolume(float volume)
    {
        playerShootVolume = Mathf.Clamp01(volume);
    }
    
    public void SetPlayerExplosionVolume(float volume)
    {
        playerExplosionVolume = Mathf.Clamp01(volume);
    }
    
    public void SetPlayerHitVolume(float volume)
    {
        playerHitVolume = Mathf.Clamp01(volume);
    }
    
    public void SetEnemyShootVolume(float volume)
    {
        enemyShootVolume = Mathf.Clamp01(volume);
    }
    
    public void SetEnemyExplosionVolume(float volume)
    {
        enemyExplosionVolume = Mathf.Clamp01(volume);
    }
    
    public void SetEnemyHitVolume(float volume)
    {
        enemyHitVolume = Mathf.Clamp01(volume);
    }
    
    public void SetBossShootVolume(float volume)
    {
        bossShootVolume = Mathf.Clamp01(volume);
    }
    
    public void SetBossExplosionVolume(float volume)
    {
        bossExplosionVolume = Mathf.Clamp01(volume);
    }
    
    public void SetBossHitVolume(float volume)
    {
        bossHitVolume = Mathf.Clamp01(volume);
    }
    
    public void SetProjectileHitVolume(float volume)
    {
        projectileHitVolume = Mathf.Clamp01(volume);
    }
    
    public void SetPowerUpVolume(float volume)
    {
        powerUpVolume = Mathf.Clamp01(volume);
    }
} 