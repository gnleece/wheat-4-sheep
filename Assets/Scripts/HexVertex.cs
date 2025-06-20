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

    public IReadOnlyList<HexTile> NeighborHexTiles => neighborHexes;


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

    public void EnableSelection(bool enable, Color? hoverColor = null)
    {
        if (VertexObject != null)
        {
            VertexObject.EnableSelection(enable, hoverColor);
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

        // Check if vertex is on a valid hex tile
        foreach (var hex in neighborHexes)
        {
            if (hex.CanHaveBuildingsAndRoads)
            {
                return true;
            }
        }

        return false;
    }

    public bool AvailableForBuilding(IPlayer player, bool mustConnectToRoad)
    {
        if (!CanHaveBuildings())
        {
            return false;
        }

        // Check if vertex is already occupied
        if (IsOccupied)
        {
            return false;
        }

        // Check distance rule - no adjacent vertices can be occupied
        foreach (var neighbor in neighborVertices)
        {
            if (neighbor.IsOccupied)
            {
                return false;
            }
        }

        if (mustConnectToRoad)
        {
            // Check if vertex is connected to a road owned by the player
            foreach (var edge in neighborEdges)
            {
                if (edge.IsOccupied && edge.Road.Owner == player)
                {
                    return true;
                }
            }
            return false;
        }

        return true;
    }

    public void InitializeNeighbors(IBoardManager boardManager)
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
                    TryAddNeighborHex(new HexCoord(q, r), boardManager);
                    TryAddNeighborHex(new HexCoord(q, r + 1), boardManager);
                    TryAddNeighborHex(new HexCoord(q - 1, r + 1), boardManager);

                    TryAddNeighborVertex(new VertexCoord(q - 1, r + 1, VertexOrientation.South), boardManager);
                    TryAddNeighborVertex(new VertexCoord(q - 1, r + 2, VertexOrientation.South), boardManager);
                    TryAddNeighborVertex(new VertexCoord(q, r + 1, VertexOrientation.South), boardManager);

                    TryAddNeighborEdge(new EdgeCoord(q, r + 1, EdgeOrientation.NorthWest), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q - 1, r + 1, EdgeOrientation.NorthEast), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q, r + 1, EdgeOrientation.West), boardManager);
                    break;
                }
            case VertexOrientation.South:
                {
                    TryAddNeighborHex(new HexCoord(q, r), boardManager);
                    TryAddNeighborHex(new HexCoord(q, r - 1), boardManager);
                    TryAddNeighborHex(new HexCoord(q + 1, r - 1), boardManager);

                    TryAddNeighborVertex(new VertexCoord(q + 1, r - 2, VertexOrientation.North), boardManager);
                    TryAddNeighborVertex(new VertexCoord(q, r - 1, VertexOrientation.North), boardManager);
                    TryAddNeighborVertex(new VertexCoord(q + 1, r - 1, VertexOrientation.North), boardManager);

                    TryAddNeighborEdge(new EdgeCoord(q, r, EdgeOrientation.NorthEast), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q + 1, r - 1, EdgeOrientation.West), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q, r, EdgeOrientation.NorthWest), boardManager);
                    break;
                }
            default:
                {
                    Debug.LogError($"Unknown vertex orientation in InitializeNeighbors for {this}");
                    break;
                }
        }
    }

    private void TryAddNeighborHex(HexCoord hexCoord, IBoardManager boardManager)
    {
        if (boardManager.HexMap.TryGetValue(hexCoord, out var hex))
        {
            neighborHexes.Add(hex);
        }
    }

    private void TryAddNeighborVertex(VertexCoord vertexCoord, IBoardManager boardManager)
    {
        if (boardManager.VertexMap.TryGetValue(vertexCoord, out var vertex))
        {
            neighborVertices.Add(vertex);
        }
    }

    private void TryAddNeighborEdge(EdgeCoord edgeCoord, IBoardManager boardManager)
    {
        if (boardManager.EdgeMap.TryGetValue(edgeCoord, out var edge))
        {
            neighborEdges.Add(edge);
        }
    }
}
