using Grid;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HexTile
{
    public HexCoord HexCoordinates { get; private set; }
    public bool CanHaveBuildingsAndRoads { get; private set; }
    public int Ring => HexCoordinates.Ring;

    public HexTileObject TileObject { get; set; }

    private List<HexVertex> _neighborVertices = new List<HexVertex>();
    public IReadOnlyList<HexVertex> NeighborVertices => _neighborVertices;

    public HexTile(HexCoord coord, bool isValidForBuilding)
    {
        HexCoordinates = coord;
        CanHaveBuildingsAndRoads = isValidForBuilding;
    }

    public ResourceType ResourceType
    {
        get
        {
            if (TileObject == null) return ResourceType.None;
            switch (TileObject.TileType)
            {
                case TileType.Wood: return ResourceType.Wood;
                case TileType.Clay: return ResourceType.Clay;
                case TileType.Sheep: return ResourceType.Sheep;
                case TileType.Wheat: return ResourceType.Wheat;
                case TileType.Ore: return ResourceType.Ore;
                default: return ResourceType.None;
            }
        }
    }

    public TileType TileType => TileObject != null ? TileObject.TileType : TileType.None;

    public int? DiceNumber => TileObject?.DiceNumber;

    public void InitializeNeighbors(IBoardManager boardManager)
    {
        _neighborVertices = new List<HexVertex>();
        foreach (var vertex in boardManager.VertexMap.Values)
        {
            if (vertex.NeighborHexTiles.Any(h => h.HexCoordinates.Equals(this.HexCoordinates)))
            {
                _neighborVertices.Add(vertex);
            }
        }
    }

    public void EnableSelection(bool enable, Color? hoverColor = null)
    {
        if (TileObject != null)
        {
            TileObject.EnableSelection(enable, hoverColor);
        }
    }

    public void MoveRobberToTile(RobberObject robberObject)
    {
        robberObject.transform.parent = TileObject.transform;
        robberObject.transform.localPosition = Vector3.zero;
        robberObject.transform.localRotation = Quaternion.identity;
    }
}