using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Heart UI System - Displays player health as 4 hearts (25 health each)
/// 
/// MANUAL SETUP INSTRUCTIONS:
/// 1. Create a Canvas in your scene (UI > Canvas)
/// 2. Create an empty GameObject as child of Canvas
/// 3. Add this HeartUI component to that GameObject
/// 4. Position the GameObject where you want hearts to appear (top-left recommended)
/// 5. Hearts will be created automatically when you play the scene
/// 
/// The system automatically:
/// - Creates 4 heart UI elements
/// - Updates when player takes damage (25 damage = 1 heart lost)
/// - Creates default heart sprites if none are assigned
/// - Animates heart loss
/// </summary>
public class HeartUI : MonoBehaviour
{
    [Header("Heart UI References")]
    public List<Image> heartImages = new List<Image>();
    
    [Header("Heart Sprites")]
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;
    
    [Header("Heart Settings")]
    public int maxHearts = 4;
    public int healthPerHeart = 25;
    public int maxHealth = 100;
    
    [Header("Animation Settings")]
    public bool enableHeartAnimation = true;
    public float heartLossAnimationDuration = 0.5f;
    public AnimationCurve heartLossAnimationCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    private PlayerShip playerShip;
    private int currentHearts;
    private int previousHealth;
    
    public static HeartUI Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
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
        // Find the player ship
        playerShip = FindFirstObjectByType<PlayerShip>();
        if (playerShip == null)
        {
            Debug.LogError("HeartUI: PlayerShip not found in scene!");
            enabled = false;
            return;
        }
        
        // Create default sprites if none are assigned
        if (fullHeartSprite == null || emptyHeartSprite == null)
        {
            CreateDefaultHeartSprites();
        }
        
        // Initialize the heart display
        InitializeHearts();
        
        // Set initial health
        previousHealth = playerShip.currentHealth;
        UpdateHeartDisplay(playerShip.currentHealth);
        
        Debug.Log("HeartUI: System initialized with 4 hearts representing 100 health");
    }
    
    void Update()
    {
        // Check for health changes
        if (playerShip != null && playerShip.currentHealth != previousHealth)
        {
            UpdateHeartDisplay(playerShip.currentHealth);
            previousHealth = playerShip.currentHealth;
        }
    }
    
    void InitializeHearts()
    {
        // If no heart images are assigned, try to find them automatically
        if (heartImages.Count == 0)
        {
            CreateHeartImages();
        }
        
        // Make sure we have the right number of hearts
        while (heartImages.Count < maxHearts)
        {
            CreateSingleHeartImage(heartImages.Count);
        }
        
        // Initialize all hearts as full
        currentHearts = maxHearts;
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].color = Color.white;
            }
        }
        
        Debug.Log($"HeartUI: Initialized with {heartImages.Count} hearts");
    }
    
    void CreateHeartImages()
    {
        // Create heart images automatically if none exist
        GameObject heartContainer = new GameObject("HeartContainer");
        heartContainer.transform.SetParent(transform, false);
        
        // Add horizontal layout group for automatic positioning
        HorizontalLayoutGroup layoutGroup = heartContainer.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 10f;
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        
        // Set up container to fill parent
        RectTransform containerRect = heartContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(0, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = Vector2.zero;
        
        // Create individual heart images
        for (int i = 0; i < maxHearts; i++)
        {
            CreateSingleHeartImage(i, heartContainer);
        }
    }
    
    void CreateSingleHeartImage(int index, GameObject parent = null)
    {
        GameObject heartObj = new GameObject($"Heart_{index + 1}");
        
        if (parent != null)
        {
            heartObj.transform.SetParent(parent.transform, false);
        }
        else
        {
            heartObj.transform.SetParent(transform, false);
        }
        
        Image heartImage = heartObj.AddComponent<Image>();
        heartImage.sprite = fullHeartSprite;
        heartImage.preserveAspect = true;
        
        // Set heart size
        RectTransform heartRect = heartImage.GetComponent<RectTransform>();
        heartRect.sizeDelta = new Vector2(40, 40);
        
        heartImages.Add(heartImage);
        
        Debug.Log($"HeartUI: Created heart {index + 1}");
    }
    
    public void UpdateHeartDisplay(int currentHealth)
    {
        // Calculate how many hearts should be full
        int heartsToShow = Mathf.CeilToInt((float)currentHealth / healthPerHeart);
        heartsToShow = Mathf.Clamp(heartsToShow, 0, maxHearts);
        
        // Update heart sprites
        for (int i = 0; i < heartImages.Count && i < maxHearts; i++)
        {
            if (heartImages[i] != null)
            {
                if (i < heartsToShow)
                {
                    heartImages[i].sprite = fullHeartSprite;
                    heartImages[i].color = Color.white;
                }
                else
                {
                    heartImages[i].sprite = emptyHeartSprite;
                    heartImages[i].color = Color.white;
                }
            }
        }
        
        // Play heart loss animation if hearts decreased
        if (heartsToShow < currentHearts && enableHeartAnimation)
        {
            StartCoroutine(PlayHeartLossAnimation(currentHearts - 1));
        }
        
        currentHearts = heartsToShow;
        
        Debug.Log($"HeartUI: Updated display - Health: {currentHealth}, Hearts showing: {heartsToShow}");
    }
    
    System.Collections.IEnumerator PlayHeartLossAnimation(int heartIndex)
    {
        if (heartIndex >= 0 && heartIndex < heartImages.Count && heartImages[heartIndex] != null)
        {
            Image heartImage = heartImages[heartIndex];
            Vector3 originalScale = heartImage.transform.localScale;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < heartLossAnimationDuration)
            {
                float progress = elapsedTime / heartLossAnimationDuration;
                float animValue = heartLossAnimationCurve.Evaluate(progress);
                
                // Scale and color animation
                heartImage.transform.localScale = originalScale * (0.5f + animValue * 0.5f);
                heartImage.color = Color.Lerp(Color.red, Color.white, animValue);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Restore original scale and set final color
            heartImage.transform.localScale = originalScale;
            heartImage.color = Color.white;
        }
    }
    
    // Public methods for external health updates
    public void OnPlayerTakeDamage(int currentHealth)
    {
        UpdateHeartDisplay(currentHealth);
    }
    
    public void OnPlayerHeal(int currentHealth)
    {
        UpdateHeartDisplay(currentHealth);
    }
    
    // Method to create default heart sprites if none are assigned
    public void CreateDefaultHeartSprites()
    {
        if (fullHeartSprite == null)
        {
            // Create a simple red square as full heart (fallback)
            Texture2D fullHeartTexture = new Texture2D(32, 32);
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    fullHeartTexture.SetPixel(x, y, Color.red);
                }
            }
            fullHeartTexture.Apply();
            fullHeartSprite = Sprite.Create(fullHeartTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }
        
        if (emptyHeartSprite == null)
        {
            // Create a simple gray square as empty heart (fallback)
            Texture2D emptyHeartTexture = new Texture2D(32, 32);
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    emptyHeartTexture.SetPixel(x, y, Color.gray);
                }
            }
            emptyHeartTexture.Apply();
            emptyHeartSprite = Sprite.Create(emptyHeartTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }
    }
    
    void OnValidate()
    {
        // Ensure settings are valid
        maxHealth = maxHearts * healthPerHeart;
        
        if (healthPerHeart <= 0)
        {
            healthPerHeart = 25;
        }
        
        if (maxHearts <= 0)
        {
            maxHearts = 4;
        }
    }
} 