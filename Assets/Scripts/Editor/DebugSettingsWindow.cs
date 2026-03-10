using UnityEngine;
using UnityEditor;

public class DebugSettingsWindow : EditorWindow
{
    private DebugSettings _debugSettings;
    private SerializedObject _serializedObject;
    private SerializedProperty _showHexCoordinatesProperty;

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
            _debugSettings = AssetDatabase.LoadAssetAtPath<DebugSettings>(path);
        }
        else
        {
            // Create new settings asset if none exists
            _debugSettings = CreateInstance<DebugSettings>();
            AssetDatabase.CreateAsset(_debugSettings, "Assets/Settings/DebugSettings.asset");
            AssetDatabase.SaveAssets();
        }

        _serializedObject = new SerializedObject(_debugSettings);
        _showHexCoordinatesProperty = _serializedObject.FindProperty("showHexCoordinates");
    }

    private void OnGUI()
    {
        if (_debugSettings == null)
        {
            LoadSettings();
            return;
        }

        _serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_showHexCoordinatesProperty, new GUIContent("Show Hex Coordinates"));
        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _debugSettings.ShowHexCoordinates = _showHexCoordinatesProperty.boolValue;
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

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Print Player Resource Hands"))
        {
            var boardManager = Object.FindFirstObjectByType<BoardManager>();
            if (boardManager != null)
            {
                Debug.Log(boardManager.GetAllPlayerResourceHandsDebugString());
            }
            else
            {
                Debug.LogWarning("BoardManager not found in scene.");
            }
        }

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Print Player Scores"))
        {
            var boardManager = Object.FindFirstObjectByType<BoardManager>();
            if (boardManager != null)
            {
                var gameManager = Object.FindFirstObjectByType<GameManager>();
                if (gameManager != null && gameManager.PlayerList != null)
                {
                    var scoreString = "Player Scores:\n";
                    foreach (var player in gameManager.PlayerList)
                    {
                        var score = boardManager.GetPlayerScore(player);
                        scoreString += $"Player {player.PlayerId}: {score} points\n";
                    }
                    Debug.Log(scoreString);
                }
                else
                {
                    Debug.LogWarning("GameManager or player list not found in scene.");
                }
            }
            else
            {
                Debug.LogWarning("BoardManager not found in scene.");
            }
        }
    }
} 