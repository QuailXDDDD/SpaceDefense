using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ScrollingTextManager : MonoBehaviour
{
    [Header("Scrolling Text Settings")]
    public TextMeshProUGUI scrollingText;
    public TextMeshProUGUI finishText;
    public float scrollSpeed = 50f;
    public float startDelay = 1f;
    public float finishTextDelay = 1f;
    public float endDelay = 2f;
    
    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene";
    
    [Header("Skip Settings")]
    public KeyCode skipKey = KeyCode.Space;
    public KeyCode skipKey2 = KeyCode.Escape;
    public Button skipButton;
    
    [Header("Text Content")]
    [TextArea(10, 15)]
    public string storyText = @"Năm 2157...

Trái Đất đang phải đối mặt với cuộc xâm lăng từ những kẻ thù bí ẩn từ vũ trụ sâu thẳm.

Các hạm đội phòng thủ của chúng ta đã bị tiêu diệt.

Bạn là phi công cuối cùng của Lực lượng Phòng thủ Trái Đất.

Số phận của nhân loại nằm trong tay bạn.

Hãy bảo vệ hành tinh chúng ta!

Chúc may mắn, Phi công...";
    
    [Header("Finish Text Settings")]
    [TextArea(2, 3)]
    public string finishMessage = "FINISH THE MISSION";
    public float finishTextSpeed = 80f;
    
    private RectTransform textRectTransform;
    private RectTransform finishTextRectTransform;
    private Canvas canvas;
    private float canvasHeight;
    private float textHeight;
    private float finishTextHeight;
    private bool isScrolling = false;
    private bool skipRequested = false;
    
    void Start()
    {
        InitializeScrollingText();
        StartCoroutine(ScrollingSequence());
    }
    
    void Update()
    {
        // Kiểm tra input để skip
        if ((Input.GetKeyDown(skipKey) || Input.GetKeyDown(skipKey2)) && !skipRequested)
        {
            SkipToGame();
        }
    }
    
    void InitializeScrollingText()
    {
        // Thiết lập story text content
        if (scrollingText != null)
        {
            scrollingText.text = storyText;
            textRectTransform = scrollingText.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("ScrollingTextManager: Scrolling text component not assigned!");
            return;
        }
        
                 // Thiết lập finish text content
         if (finishText != null)
         {
             finishText.text = finishMessage;
             finishTextRectTransform = finishText.GetComponent<RectTransform>();
             Debug.Log($"Finish text component found: {finishText.name}");
         }
         else
         {
             Debug.LogWarning("ScrollingTextManager: Finish text component not assigned! Creating one automatically...");
             CreateFinishTextAutomatically();
         }
        
        // Lấy canvas
        canvas = scrollingText.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasHeight = canvas.GetComponent<RectTransform>().sizeDelta.y;
        }
        
        // Tính toán kích thước text
        LayoutRebuilder.ForceRebuildLayoutImmediate(textRectTransform);
        textHeight = textRectTransform.sizeDelta.y;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(finishTextRectTransform);
        finishTextHeight = finishTextRectTransform.sizeDelta.y;
        
        // Đặt vị trí ban đầu cho story text (dưới màn hình)
        Vector3 startPosition = textRectTransform.anchoredPosition;
        startPosition.y = -(canvasHeight/2 + textHeight/2);
        textRectTransform.anchoredPosition = startPosition;
        
                 // Đặt vị trí ban đầu cho finish text (xa hơn xuống dưới)
         Vector3 finishStartPosition = finishTextRectTransform.anchoredPosition;
         finishStartPosition.y = -(canvasHeight + 100f); // Đơn giản hơn, chỉ cần xuống dưới canvas
         finishTextRectTransform.anchoredPosition = finishStartPosition;
         
         Debug.Log($"Finish text start position: {finishStartPosition.y}");
        
        // Thiết lập skip button
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipToGame);
        }
        
        Debug.Log($"ScrollingTextManager: Initialized. Canvas height: {canvasHeight}, Story text height: {textHeight}, Finish text height: {finishTextHeight}");
    }
    
    IEnumerator ScrollingSequence()
    {
        // Chờ trước khi bắt đầu
        yield return new WaitForSeconds(startDelay);
        
        isScrolling = true;
        
        // Phase 1: Scroll story text
        float storyTargetY = canvasHeight/2 + textHeight/2;
        
        while (textRectTransform.anchoredPosition.y < storyTargetY && !skipRequested)
        {
            Vector3 currentPos = textRectTransform.anchoredPosition;
            currentPos.y += scrollSpeed * Time.deltaTime;
            textRectTransform.anchoredPosition = currentPos;
            
            yield return null;
        }
        
        if (!skipRequested)
        {
            // Chờ một chút trước khi scroll finish text
            yield return new WaitForSeconds(finishTextDelay);
            
                         // Phase 2: Scroll finish text
             float finishTargetY = 0f; // Center của màn hình
             
             Debug.Log($"Starting finish text scroll from Y: {finishTextRectTransform.anchoredPosition.y} to target: {finishTargetY}");
             
             while (finishTextRectTransform.anchoredPosition.y < finishTargetY && !skipRequested)
             {
                 Vector3 currentPos = finishTextRectTransform.anchoredPosition;
                 currentPos.y += finishTextSpeed * Time.deltaTime;
                 finishTextRectTransform.anchoredPosition = currentPos;
                 
                 // Debug log mỗi vài frame
                 if (Time.frameCount % 30 == 0)
                 {
                     Debug.Log($"Finish text current Y: {currentPos.y}");
                 }
                 
                 yield return null;
             }
             
             Debug.Log($"Finish text scroll completed at Y: {finishTextRectTransform.anchoredPosition.y}");
            
            if (!skipRequested)
            {
                // Chờ một chút trước khi chuyển scene
                yield return new WaitForSeconds(endDelay);
                
                // Chuyển sang game scene
                TransitionToGame();
            }
        }
    }
    
    public void SkipToGame()
    {
        if (!skipRequested)
        {
            skipRequested = true;
            Debug.Log("ScrollingTextManager: Skip requested");
            
            // Play sound effect nếu có
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPowerUp();
            }
            
            TransitionToGame();
        }
    }
    
    void TransitionToGame()
    {
        Debug.Log($"ScrollingTextManager: Transitioning to game scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }
    
    void OnDestroy()
    {
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipToGame);
        }
    }
    
    [ContextMenu("Test Transition")]
    public void TestTransition()
    {
        TransitionToGame();
    }
    
    void CreateFinishTextAutomatically()
    {
        // Tạo finish text tự động nếu chưa có
        GameObject finishTextObj = new GameObject("FinishText");
        finishTextObj.transform.SetParent(scrollingText.transform.parent);
        
        finishText = finishTextObj.AddComponent<TextMeshProUGUI>();
        finishText.text = finishMessage;
        finishText.fontSize = 48;
        finishText.color = Color.yellow;
        finishText.alignment = TextAlignmentOptions.Center;
        finishText.fontStyle = FontStyles.Bold;
        
        finishTextRectTransform = finishText.GetComponent<RectTransform>();
        finishTextRectTransform.sizeDelta = new Vector2(800, 100);
        finishTextRectTransform.anchoredPosition = Vector2.zero;
        
        Debug.Log("✅ Auto-created finish text component");
    }
    
    [ContextMenu("Force Create Finish Text")]
    public void ForceCreateFinishText()
    {
        CreateFinishTextAutomatically();
    }
    
    [ContextMenu("Test Finish Text Scroll")]
    public void TestFinishTextScroll()
    {
        if (finishTextRectTransform != null)
        {
            StartCoroutine(TestFinishScrollCoroutine());
        }
        else
        {
            Debug.LogError("Finish text not found!");
        }
    }
    
    IEnumerator TestFinishScrollCoroutine()
    {
        Debug.Log("🧪 Testing finish text scroll...");
        
        // Reset vị trí
        Vector3 startPos = finishTextRectTransform.anchoredPosition;
        startPos.y = -1000f;
        finishTextRectTransform.anchoredPosition = startPos;
        
        float targetY = 0f;
        Debug.Log($"Scrolling from {startPos.y} to {targetY}");
        
        while (finishTextRectTransform.anchoredPosition.y < targetY)
        {
            Vector3 currentPos = finishTextRectTransform.anchoredPosition;
            currentPos.y += finishTextSpeed * Time.deltaTime;
            finishTextRectTransform.anchoredPosition = currentPos;
            
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"Test scroll Y: {currentPos.y}");
            }
            
            yield return null;
        }
        
        Debug.Log("🎉 Test scroll completed!");
    }
} 