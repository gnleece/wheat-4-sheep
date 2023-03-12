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
    public int WaterTileCount;

    public int[] TileDiceNumbers;

    public int GetTileCount(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Wood:     return WoodTileCount;
            case TileType.Clay:     return ClayTileCount;
            case TileType.Sheep:    return SheepTileCount;
            case TileType.Wheat:    return WheatTileCount;
            case TileType.Ore:      return OreTileCount;
            case TileType.Desert:   return DesertTileCount;
            case TileType.Water:    return WaterTileCount;
            default:                return 0;
        }
    }

    public Dictionary<TileType, int> GetShuffleableTileCounts()
    {
        var counts = new Dictionary<TileType, int>();
        foreach (var tileType in (TileType[])Enum.GetValues(typeof(TileType)))
        {
            if (tileType == TileType.Water)
            {
                continue;
            }

            counts[tileType] = GetTileCount(tileType);
        }
        return counts;
    }
}