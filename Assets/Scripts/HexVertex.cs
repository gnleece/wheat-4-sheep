using Grid;
using System.Collections.Generic;
using UnityEngine;

public class HexVertex
{
    public VertexCoord VertexCoord { get; private set; }

    public HexVertexObject VertexObject { get; set; }

    public Building Building { get; private set; }

    public bool IsOccupied => Building != null;

    public IPlayer Owner => Building?.Owner;


    private List<HexTile> neighborHexes = null;
    private List<HexVertex> neighborVertices = null;
    private List<HexEdge> neighborEdges = null;

    public HexVertex(VertexCoord coord)
    {
        VertexCoord = coord;
    }

    public override string ToString()
    {
        return $"Vertex {VertexCoord}";
    }

    public void EnableSelection(bool enable)
    {
        if (VertexObject != null)
        {
            VertexObject.EnableSelection(enable);
        }
    }

    public bool TryPlaceBuilding(Building.BuildingType type, IPlayer owner)
    {
        if (IsOccupied || owner == null)
        {
            return false;
        }

        foreach (var neighbor in neighborVertices)
        {
            if (neighbor.IsOccupied)
            {
                return false;
            }
        }

        Building = new Building(type, this, owner);
        if (VertexObject != null)
        {
            VertexObject.Refresh();
        }
        return true;
    }

    public bool CanHaveBuildings()
    {
        if (neighborHexes == null || neighborHexes.Count == 0)
        {
            Debug.LogWarning($"No neighbor hex tiles initialized for {this}");
            return false;
        }

        foreach (var hex in neighborHexes)
        {
            if (hex.CanHaveBuildingsAndRoads)
            {
                return true;
            }
        }

        return false;
    }

    public void InitializeNeighbors(IGrid grid)
    {
        neighborHexes = new List<HexTile>();
        neighborVertices = new List<HexVertex>();
        neighborEdges = new List<HexEdge>();

        var q = VertexCoord.HexCoord.q;
        var r = VertexCoord.HexCoord.r;

        switch (VertexCoord.Orientation)
        {
            case VertexOrientation.North:
                {
                    TryAddNeighborHex(new HexCoord(q, r), grid);
                    TryAddNeighborHex(new HexCoord(q, r + 1), grid);
                    TryAddNeighborHex(new HexCoord(q - 1, r + 1), grid);

                    TryAddNeighborVertex(new VertexCoord(q - 1, r + 1, VertexOrientation.South), grid);
                    TryAddNeighborVertex(new VertexCoord(q - 1, r + 2, VertexOrientation.South), grid);
                    TryAddNeighborVertex(new VertexCoord(q, r + 1, VertexOrientation.South), grid);

                    TryAddNeighborEdge(new EdgeCoord(q, r + 1, EdgeOrientation.NorthWest), grid);
                    TryAddNeighborEdge(new EdgeCoord(q - 1, r + 1, EdgeOrientation.NorthEast), grid);
                    TryAddNeighborEdge(new EdgeCoord(q, r + 1, EdgeOrientation.West), grid);
                    break;
                }
            case VertexOrientation.South:
                {
                    TryAddNeighborHex(new HexCoord(q, r), grid);
                    TryAddNeighborHex(new HexCoord(q, r - 1), grid);
                    TryAddNeighborHex(new HexCoord(q + 1, r - 1), grid);

                    TryAddNeighborVertex(new VertexCoord(q + 1, r - 2, VertexOrientation.North), grid);
                    TryAddNeighborVertex(new VertexCoord(q, r - 1, VertexOrientation.North), grid);
                    TryAddNeighborVertex(new VertexCoord(q + 1, r - 1, VertexOrientation.North), grid);

                    TryAddNeighborEdge(new EdgeCoord(q, r, EdgeOrientation.NorthEast), grid);
                    TryAddNeighborEdge(new EdgeCoord(q + 1, r - 1, EdgeOrientation.West), grid);
                    TryAddNeighborEdge(new EdgeCoord(q, r, EdgeOrientation.NorthWest), grid);
                    break;
                }
            default:
                {
                    Debug.LogError($"Unknown vertex orientation in InitializeNeighbors for {this}");
                    break;
                }
        }
    }

    private void TryAddNeighborHex(HexCoord hexCoord, IGrid grid)
    {
        if (grid.HexMap.TryGetValue(hexCoord, out var hex))
        {
            neighborHexes.Add(hex);
        }
    }

    private void TryAddNeighborVertex(VertexCoord vertexCoord, IGrid grid)
    {
        if (grid.VertexMap.TryGetValue(vertexCoord, out var vertex))
        {
            neighborVertices.Add(vertex);
        }
    }

    private void TryAddNeighborEdge(EdgeCoord edgeCoord, IGrid grid)
    {
        if (grid.EdgeMap.TryGetValue(edgeCoord, out var edge))
        {
            neighborEdges.Add(edge);
        }
    }
}
