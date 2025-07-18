using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class ScreenAdapter : MonoBehaviour
{
    [Header("Reference Resolution")]
    [Tooltip("Your target resolution (what you designed the game for)")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    
    [Header("Camera Settings")]
    [Tooltip("Base orthographic size for reference resolution")]
    public float baseOrthographicSize = 5f;
    
    [Header("Adaptation Mode")]
    public CameraAdaptationMode adaptationMode = CameraAdaptationMode.Letterbox;
    
    [Header("UI References")]
    [Tooltip("Assign your main Canvas here")]
    public Canvas mainCanvas;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private Camera cam;
    private CanvasScaler canvasScaler;
    private Vector2 currentScreenSize;
    
    public enum CameraAdaptationMode
    {
        [Tooltip("Maintains aspect ratio with black bars if needed")]
        Letterbox,
        [Tooltip("Fills entire screen, may crop top/bottom on wide screens")]
        FillScreen,
        [Tooltip("Shows more content on wider screens")]
        Expand
    }
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        
        // Find Canvas if not assigned
        if (mainCanvas == null)
        {
            mainCanvas = FindFirstObjectByType<Canvas>();
        }
        
        if (mainCanvas != null)
        {
            canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                canvasScaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
            }
        }
    }
    
    void Start()
    {
        ApplyScreenAdaptation();
    }
    
    void Update()
    {
        // Check if screen size changed (for window resizing in editor/PC)
        Vector2 newScreenSize = new Vector2(Screen.width, Screen.height);
        if (newScreenSize != currentScreenSize)
        {
            currentScreenSize = newScreenSize;
            ApplyScreenAdaptation();
        }
    }
    
    void ApplyScreenAdaptation()
    {
        if (cam == null) return;
        
        float screenAspect = (float)Screen.width / Screen.height;
        float referenceAspect = referenceResolution.x / referenceResolution.y;
        
        AdaptCamera(screenAspect, referenceAspect);
        AdaptUI();
        
        if (showDebugInfo)
        {
            Debug.Log($"ScreenAdapter: Resolution {Screen.width}x{Screen.height}, " +
                     $"Aspect {screenAspect:F2}, Mode: {adaptationMode}");
        }
    }
    
    void AdaptCamera(float screenAspect, float referenceAspect)
    {
        switch (adaptationMode)
        {
            case CameraAdaptationMode.Letterbox:
                // Maintain exact game area, add letterboxing if needed
                if (screenAspect >= referenceAspect)
                {
                    // Screen is wider than reference - letterbox on sides
                    cam.orthographicSize = baseOrthographicSize;
                }
                else
                {
                    // Screen is taller than reference - letterbox on top/bottom
                    cam.orthographicSize = baseOrthographicSize / (screenAspect / referenceAspect);
                }
                break;
                
            case CameraAdaptationMode.FillScreen:
                // Fill entire screen, may crop content
                if (screenAspect >= referenceAspect)
                {
                    // Screen is wider - show more horizontally
                    cam.orthographicSize = baseOrthographicSize;
                }
                else
                {
                    // Screen is taller - crop top/bottom to fit
                    cam.orthographicSize = baseOrthographicSize * (referenceAspect / screenAspect);
                }
                break;
                
            case CameraAdaptationMode.Expand:
                // Show more content on wider screens
                if (screenAspect >= referenceAspect)
                {
                    // Wider screen - keep same height, show more width
                    cam.orthographicSize = baseOrthographicSize;
                }
                else
                {
                    // Taller screen - show more height
                    cam.orthographicSize = baseOrthographicSize / (screenAspect / referenceAspect);
                }
                break;
        }
        
        // Ensure minimum size for very extreme aspect ratios
        cam.orthographicSize = Mathf.Max(cam.orthographicSize, 3f);
        cam.orthographicSize = Mathf.Min(cam.orthographicSize, 10f);
    }
    
    void AdaptUI()
    {
        if (canvasScaler == null) return;
        
        // Configure Canvas Scaler for proper UI scaling
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = referenceResolution;
        
        // Choose match mode based on screen aspect ratio
        float screenAspect = (float)Screen.width / Screen.height;
        float referenceAspect = referenceResolution.x / referenceResolution.y;
        
        if (screenAspect > referenceAspect)
        {
            // Wider screen - match height to keep UI elements properly sized
            canvasScaler.matchWidthOrHeight = 1f; // Match height
        }
        else
        {
            // Taller screen - match width to keep UI elements properly sized  
            canvasScaler.matchWidthOrHeight = 0f; // Match width
        }
        
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    }
    
    // Public method to change adaptation mode at runtime
    public void SetAdaptationMode(CameraAdaptationMode newMode)
    {
        adaptationMode = newMode;
        ApplyScreenAdaptation();
    }
    
    // Get the current viewport bounds in world space
    public Bounds GetCameraWorldBounds()
    {
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        return new Bounds(cam.transform.position, new Vector3(width, height, 0));
    }
    
    // Helper method for other scripts to get screen boundaries
    public static Vector4 GetScreenBounds()
    {
        if (Camera.main == null) return Vector4.zero;
        
        float height = Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;
        
        return new Vector4(
            -width,  // left
            width,   // right  
            -height, // bottom
            height   // top
        );
    }
    
    void OnDrawGizmosSelected()
    {
        if (cam == null) return;
        
        // Draw camera bounds
        Bounds bounds = GetCameraWorldBounds();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        // Draw reference resolution area
        float refHeight = baseOrthographicSize * 2f;
        float refWidth = refHeight * (referenceResolution.x / referenceResolution.y);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(cam.transform.position, new Vector3(refWidth, refHeight, 0));
    }
} 