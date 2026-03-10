using UnityEngine;

public class PlayerColorManager : MonoBehaviour
{
    private static readonly Color[] PlayerColors = {
        new(0.8f, 0.2f, 0.2f), // Red
        new(0.2f, 0.2f, 0.8f), // Blue
        new(1.0f, 0.6f, 0.2f), // Orange
        new(0.8f, 0.8f, 0.2f)  // Yellow
    };

    public static Color GetPlayerColor(int playerId)
    {
        if (playerId < 0 || playerId >= PlayerColors.Length)
        {
            Debug.LogWarning($"Invalid player ID {playerId}, using default color");
            return Color.white;
        }
        return PlayerColors[playerId];
    }

    public static void ApplyColorToRenderer(Renderer renderer, int playerId)
    {
        if (renderer != null)
        {
            renderer.material.color = GetPlayerColor(playerId);
        }
    }
} 