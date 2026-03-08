using UnityEngine;

public class PlayerColorManager : MonoBehaviour
{
    private static readonly Color[] PlayerColors = new Color[]
    {
        new Color(0.8f, 0.2f, 0.2f), // Red
        new Color(0.2f, 0.2f, 0.8f), // Blue
        new Color(0.2f, 0.8f, 0.2f), // Green
        new Color(0.8f, 0.8f, 0.2f)  // Yellow
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