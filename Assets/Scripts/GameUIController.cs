using UnityEngine;

[System.Serializable]
public class GameUIController : MonoBehaviour
{
    [Header("UI Setup")]
    public bool createUIOnStart = true;
    
    private UISetup uiSetup;
    
    private void Awake()
    {
        Debug.Log("GameUIController Awake() called");
    }
    
    private void Start()
    {
        Debug.Log("GameUIController Start() called");
        if (createUIOnStart)
        {
            Debug.Log("Creating Game UI...");
            CreateGameUI();
        }
        else
        {
            Debug.Log("createUIOnStart is false, not creating UI");
        }
    }
    
    [ContextMenu("Create Game UI")]
    public void CreateGameUI()
    {
        Debug.Log("CreateGameUI() called");
        if (uiSetup == null)
        {
            Debug.Log("Creating UI Setup GameObject...");
            GameObject uiSetupObject = new GameObject("UI Setup");
            uiSetupObject.transform.SetParent(transform);
            uiSetup = uiSetupObject.AddComponent<UISetup>();
            Debug.Log("UI Setup created successfully");
        }
        else
        {
            Debug.Log("UI Setup already exists");
        }
    }
    
    [ContextMenu("Destroy Game UI")]
    public void DestroyGameUI()
    {
        if (uiSetup != null)
        {
            DestroyImmediate(uiSetup.gameObject);
            uiSetup = null;
        }
    }
}