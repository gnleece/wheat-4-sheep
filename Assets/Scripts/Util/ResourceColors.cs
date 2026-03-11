using UnityEngine;

public static class ResourceColors
{
    // Colors match the corresponding hex tile materials exactly.
    // Values are in linear color space (matching project color space setting).
    private static readonly Color[] Colors =
    {
        new Color(0.2812592f, 0.5660378f, 0.19971521f), // Wood
        new Color(0.5647059f, 0.23793031f, 0.19999999f), // Clay
        new Color(0.46296984f, 0.8301887f, 0.16760412f), // Sheep
        new Color(0.9056604f, 0.75862134f, 0.123033054f), // Wheat
        new Color(0.4056604f, 0.4056604f, 0.4056604f), // Ore
    };

    public static Color GetColor(ResourceType resourceType)
    {
        int index = (int)resourceType;
        if (index < 0 || index >= Colors.Length)
        {
            return Color.white;
        }

        return Colors[index];
    }
}
