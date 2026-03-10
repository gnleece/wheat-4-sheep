using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "DebugSettings", menuName = "Wheat4Sheep/Debug Settings")]
public class DebugSettings : ScriptableObject
{
    private static DebugSettings s_instance;
    
    public static DebugSettings Instance
    {
        get
        {
            if (s_instance == null)
            {
                var guids = AssetDatabase.FindAssets("t:DebugSettings");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    s_instance = AssetDatabase.LoadAssetAtPath<DebugSettings>(path);
                }
            }
            return s_instance;
        }
    }

    [SerializeField]
    private bool showHexCoordinates = true;

    public bool ShowHexCoordinates
    {
        get => showHexCoordinates;
        set
        {
            if (showHexCoordinates != value)
            {
                showHexCoordinates = value;
                OnSettingsChanged?.Invoke();
            }
        }
    }

    public event System.Action OnSettingsChanged;
} 