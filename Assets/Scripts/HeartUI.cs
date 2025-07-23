using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
        playerShip = FindFirstObjectByType<PlayerShip>();
        if (playerShip == null)
        {
            Debug.LogError("HeartUI: PlayerShip not found in scene!");
            enabled = false;
            return;
        }
        
        if (fullHeartSprite == null || emptyHeartSprite == null)
        {
            CreateDefaultHeartSprites();
        }
        
        InitializeHearts();
        
        previousHealth = playerShip.currentHealth;
        UpdateHeartDisplay(playerShip.currentHealth);
        
        Debug.Log("HeartUI: System initialized with 4 hearts representing 100 health");
    }
    
    void Update()
    {
        if (playerShip != null && playerShip.currentHealth != previousHealth)
        {
            UpdateHeartDisplay(playerShip.currentHealth);
            previousHealth = playerShip.currentHealth;
        }
    }
    
    void InitializeHearts()
    {
        if (heartImages.Count == 0)
        {
            CreateHeartImages();
        }
        
        while (heartImages.Count < maxHearts)
        {
            CreateSingleHeartImage(heartImages.Count);
        }
        
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
        GameObject heartContainer = new GameObject("HeartContainer");
        heartContainer.transform.SetParent(transform, false);
        
        HorizontalLayoutGroup layoutGroup = heartContainer.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 10f;
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        
        RectTransform containerRect = heartContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(0, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = Vector2.zero;
        
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
        
        RectTransform heartRect = heartImage.GetComponent<RectTransform>();
        heartRect.sizeDelta = new Vector2(40, 40);
        
        heartImages.Add(heartImage);
        
        Debug.Log($"HeartUI: Created heart {index + 1}");
    }
    
    public void UpdateHeartDisplay(int currentHealth)
    {
        int heartsToShow = Mathf.CeilToInt((float)currentHealth / healthPerHeart);
        heartsToShow = Mathf.Clamp(heartsToShow, 0, maxHearts);
        
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
                
                heartImage.transform.localScale = originalScale * (0.5f + animValue * 0.5f);
                heartImage.color = Color.Lerp(Color.red, Color.white, animValue);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            heartImage.transform.localScale = originalScale;
            heartImage.color = Color.white;
        }
    }
    
    public void OnPlayerTakeDamage(int currentHealth)
    {
        UpdateHeartDisplay(currentHealth);
    }
    
    public void OnPlayerHeal(int currentHealth)
    {
        UpdateHeartDisplay(currentHealth);
    }
    
    public void CreateDefaultHeartSprites()
    {
        if (fullHeartSprite == null)
        {
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
    
    public void ResetForNewGame()
    {
        currentHearts = maxHearts;
        previousHealth = maxHealth;
        
        playerShip = FindFirstObjectByType<PlayerShip>();
        
        for (int i = 0; i < heartImages.Count && i < maxHearts; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].color = Color.white;
                heartImages[i].transform.localScale = Vector3.one;
            }
        }
        
        Debug.Log("HeartUI: Heart system reset for new game - all hearts restored to full");
    }
} 