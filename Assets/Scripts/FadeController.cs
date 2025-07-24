using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;
    
    [Header("Auto Fade Settings")]
    public bool fadeInOnStart = true;
    public bool fadeOutOnDestroy = false;
    
    private static FadeController instance;
    public static FadeController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<FadeController>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        SetupFadeImage();
    }
    
    void Start()
    {
        if (fadeInOnStart)
        {
            StartCoroutine(FadeIn());
        }
    }
    
    void SetupFadeImage()
    {
        if (fadeImage == null)
        {
            // Tạo fade image tự động nếu không có
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("FadeController: No Canvas found!");
                return;
            }
            
            GameObject fadeObj = new GameObject("FadeImage");
            fadeObj.transform.SetParent(canvas.transform);
            
            fadeImage = fadeObj.AddComponent<Image>();
            fadeImage.color = fadeColor;
            
            // Đặt làm full screen và ở layer cao nhất
            RectTransform rect = fadeImage.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.SetAsLastSibling();
        }
    }
    
    public void FadeInImmediate()
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }
    }
    
    public void FadeOutImmediate()
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }
    }
    
    public IEnumerator FadeIn(float duration = -1f)
    {
        if (duration < 0) duration = fadeDuration;
        
        if (fadeImage != null)
        {
            float elapsedTime = 0f;
            Color startColor = fadeImage.color;
            startColor.a = 1f;
            Color endColor = startColor;
            endColor.a = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                
                Color currentColor = fadeImage.color;
                currentColor.a = alpha;
                fadeImage.color = currentColor;
                
                yield return null;
            }
            
            fadeImage.color = endColor;
        }
    }
    
    public IEnumerator FadeOut(float duration = -1f)
    {
        if (duration < 0) duration = fadeDuration;
        
        if (fadeImage != null)
        {
            float elapsedTime = 0f;
            Color startColor = fadeImage.color;
            startColor.a = 0f;
            Color endColor = startColor;
            endColor.a = 1f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                
                Color currentColor = fadeImage.color;
                currentColor.a = alpha;
                fadeImage.color = currentColor;
                
                yield return null;
            }
            
            fadeImage.color = endColor;
        }
    }
    
    public void StartFadeIn()
    {
        StartCoroutine(FadeIn());
    }
    
    public void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }
} 