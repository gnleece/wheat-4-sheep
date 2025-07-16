using UnityEngine;

public class UIDebugHelper : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("UIDebugHelper: Start() called");
        
        // Try to find GameUIController and force UI creation
        GameUIController gameUI = FindObjectOfType<GameUIController>();
        if (gameUI != null)
        {
            Debug.Log("UIDebugHelper: Found GameUIController, forcing UI creation");
            gameUI.CreateGameUI();
        }
        else
        {
            Debug.Log("UIDebugHelper: GameUIController not found!");
        }
    }
    
    [ContextMenu("Force Create UI")]
    public void ForceCreateUI()
    {
        Debug.Log("UIDebugHelper: Force Create UI called");
        GameUIController gameUI = FindObjectOfType<GameUIController>();
        if (gameUI != null)
        {
            gameUI.CreateGameUI();
        }
    }
    
    [ContextMenu("Test Simple UI")]
    public void TestSimpleUI()
    {
        Debug.Log("UIDebugHelper: Creating simple test UI");
        
        // Create a simple canvas as a test
        GameObject canvasGO = new GameObject("Test Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Create a simple panel
        GameObject panel = new GameObject("Test Panel");
        panel.transform.SetParent(canvasGO.transform);
        UnityEngine.UI.Image panelImage = panel.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = Color.red;
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.7f, 0.5f);
        panelRect.anchorMax = new Vector2(0.9f, 0.9f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = Vector2.zero;
        
        Debug.Log("UIDebugHelper: Test UI created successfully");
    }
}