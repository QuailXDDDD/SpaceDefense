using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class ScreenAdapter : MonoBehaviour
{
    [Header("Reference Resolution")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    
    [Header("Camera Settings")]
    public float baseOrthographicSize = 5f;
    
    [Header("Adaptation Mode")]
    public CameraAdaptationMode adaptationMode = CameraAdaptationMode.Letterbox;
    
    [Header("UI References")]
    public Canvas mainCanvas;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private Camera cam;
    private CanvasScaler canvasScaler;
    private Vector2 currentScreenSize;
    
    public enum CameraAdaptationMode
    {
        Letterbox,
        FillScreen,
        Expand
    }
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        
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
                if (screenAspect >= referenceAspect)
                {
                    cam.orthographicSize = baseOrthographicSize;
                }
                else
                {
                    cam.orthographicSize = baseOrthographicSize / (screenAspect / referenceAspect);
                }
                break;
                
            case CameraAdaptationMode.FillScreen:
                if (screenAspect >= referenceAspect)
                {
                    cam.orthographicSize = baseOrthographicSize;
                }
                else
                {
                    cam.orthographicSize = baseOrthographicSize * (referenceAspect / screenAspect);
                }
                break;
                
            case CameraAdaptationMode.Expand:
                if (screenAspect >= referenceAspect)
                {
                    cam.orthographicSize = baseOrthographicSize;
                }
                else
                {
                    cam.orthographicSize = baseOrthographicSize / (screenAspect / referenceAspect);
                }
                break;
        }
        
        cam.orthographicSize = Mathf.Max(cam.orthographicSize, 3f);
        cam.orthographicSize = Mathf.Min(cam.orthographicSize, 10f);
    }
    
    void AdaptUI()
    {
        if (canvasScaler == null) return;
        
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = referenceResolution;
        
        float screenAspect = (float)Screen.width / Screen.height;
        float referenceAspect = referenceResolution.x / referenceResolution.y;
        
        if (screenAspect > referenceAspect)
        {
            canvasScaler.matchWidthOrHeight = 1f;
        }
        else
        {
            canvasScaler.matchWidthOrHeight = 0f;
        }
        
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    }
    
    public void SetAdaptationMode(CameraAdaptationMode newMode)
    {
        adaptationMode = newMode;
        ApplyScreenAdaptation();
    }
    
    public Bounds GetCameraWorldBounds()
    {
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        return new Bounds(cam.transform.position, new Vector3(width, height, 0));
    }
    
    public static Vector4 GetScreenBounds()
    {
        if (Camera.main == null) return Vector4.zero;
        
        float height = Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;
        
        return new Vector4(
            -width,
            width,
            -height,
            height
        );
    }
    
    void OnDrawGizmosSelected()
    {
        if (cam == null) return;
        
        Bounds bounds = GetCameraWorldBounds();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        float refHeight = baseOrthographicSize * 2f;
        float refWidth = refHeight * (referenceResolution.x / referenceResolution.y);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(cam.transform.position, new Vector3(refWidth, refHeight, 0));
    }
} 