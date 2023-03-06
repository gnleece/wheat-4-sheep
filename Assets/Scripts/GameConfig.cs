using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    public int WoodTileCount;
    public int ClayTileCount;
    public int SheepTileCount;
    public int WheatTileCount;
    public int OreTileCount;
    public int DesertTileCount;

    public int GetTileCount(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Wood:     return WoodTileCount;
            case ResourceType.Clay:     return ClayTileCount;
            case ResourceType.Sheep:    return SheepTileCount;
            case ResourceType.Wheat:    return WheatTileCount;
            case ResourceType.Ore:      return OreTileCount;
            case ResourceType.None:     return DesertTileCount;
            default:                    return 0;
        }
    }

    public Dictionary<ResourceType, int> GetTileCounts()
    {
        var counts = new Dictionary<ResourceType, int>();
        foreach (var resourceType in (ResourceType[])Enum.GetValues(typeof(ResourceType)))
        {
            counts[resourceType] = GetTileCount(resourceType);
        }
        return counts;
    }
}