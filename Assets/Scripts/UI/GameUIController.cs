using UnityEngine;

[System.Serializable]
public class GameUIController : MonoBehaviour
{
    [Header("UI Setup")]
    public bool createUIOnStart = true;
    
    [SerializeField]
    private UISetup uiSetup;
    
    private void Awake()
    {
        Debug.Log("GameUIController Awake() called");
    }
    
    private void Start()
    {
        /*
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
        */
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