using UnityEngine;
using UnityEditor;

public class DebugSettingsWindow : EditorWindow
{
    private DebugSettings debugSettings;
    private SerializedObject serializedObject;
    private SerializedProperty showHexCoordinatesProperty;

    [MenuItem("Window/Wheat4Sheep/Debug Settings")]
    public static void ShowWindow()
    {
        var window = GetWindow<DebugSettingsWindow>("Debug Settings");
        window.minSize = new Vector2(300, 100);
    }

    private void OnEnable()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Try to find existing settings asset
        var guids = AssetDatabase.FindAssets("t:DebugSettings");
        if (guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            debugSettings = AssetDatabase.LoadAssetAtPath<DebugSettings>(path);
        }
        else
        {
            // Create new settings asset if none exists
            debugSettings = CreateInstance<DebugSettings>();
            AssetDatabase.CreateAsset(debugSettings, "Assets/Settings/DebugSettings.asset");
            AssetDatabase.SaveAssets();
        }

        serializedObject = new SerializedObject(debugSettings);
        showHexCoordinatesProperty = serializedObject.FindProperty("showHexCoordinates");
    }

    private void OnGUI()
    {
        if (debugSettings == null)
        {
            LoadSettings();
            return;
        }

        serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(showHexCoordinatesProperty, new GUIContent("Show Hex Coordinates"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            debugSettings.ShowHexCoordinates = showHexCoordinatesProperty.boolValue;
        }

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Refresh All Tiles"))
        {
            var tiles = FindObjectsByType<HexTileObject>(FindObjectsSortMode.None);
            foreach (var tile in tiles)
            {
                tile.RefreshDebugText();
            }
        }
    }
} 