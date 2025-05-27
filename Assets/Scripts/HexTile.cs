using Grid;
using UnityEngine;

public class HexTile
{
    public HexCoord HexCoordinates { get; private set; }
    public bool CanHaveBuildingsAndRoads { get; private set; }
    public int Ring => HexCoordinates.Ring;

    public HexTileObject TileObject { get; set; }

    public HexTile(HexCoord coord, bool isValidForBuilding)
    {
        HexCoordinates = coord;
        CanHaveBuildingsAndRoads = isValidForBuilding;
    }

    public void SetDebugText(string text)
    {
        if (TileObject != null)
        {
            TileObject.SetDebugText(text);
        }
    }
}