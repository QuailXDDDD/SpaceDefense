using UnityEngine;

/// <summary>
/// HEART UI SETUP GUIDE
/// 
/// This guide shows you how to manually set up the Heart UI system in your scene.
/// The Heart UI displays player health as 4 hearts, where each heart = 25 health points.
/// 
/// STEP-BY-STEP SETUP:
/// 
/// 1. CREATE CANVAS (if you don't have one):
///    - Right-click in Hierarchy > UI > Canvas
///    - This creates a Canvas with proper UI settings
/// 
/// 2. CREATE HEART UI OBJECT:
///    - Right-click on Canvas > Create Empty
///    - Rename it to "HeartUI"
/// 
/// 3. ADD HEART UI COMPONENT:
///    - Select the HeartUI GameObject
///    - In Inspector, click "Add Component"
///    - Search for "HeartUI" and add it
/// 
/// 4. POSITION THE HEART UI:
///    - With HeartUI selected, set RectTransform values:
///      * Anchor: Top-Left (click the square with 9 dots, hold Shift+Alt, click top-left)
///      * Pos X: 20
///      * Pos Y: -20
///      * Width: 200
///      * Height: 60
/// 
/// 5. PLAY AND TEST:
///    - Press Play
///    - Hearts will automatically appear
///    - Take damage from enemies to see hearts disappear!
/// 
/// OPTIONAL CUSTOMIZATION:
/// - Assign custom heart sprites in the HeartUI component
/// - Adjust animation settings
/// - Change heart size and spacing
/// 
/// The system works automatically with your existing PlayerShip damage system!
/// </summary>
public class HeartUI_README : MonoBehaviour
{
    [Header("Quick Reference")]
    [TextArea(3, 5)]
    public string quickSteps = 
        "1. Create Canvas (UI > Canvas)\n" +
        "2. Create empty child GameObject\n" +
        "3. Add HeartUI component\n" +
        "4. Position in top-left corner\n" +
        "5. Play to test!";
    
    [Header("Recommended Settings")]
    public Vector2 recommendedPosition = new Vector2(20, -20);
    public Vector2 recommendedSize = new Vector2(200, 60);
    
    void Start()
    {
        Debug.Log("HeartUI_README: Check this component's tooltip for detailed setup instructions!");
    }
} 