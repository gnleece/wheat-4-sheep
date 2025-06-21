using Grid;
using System.Collections.Generic;
using UnityEngine;

public class HexEdge
{
    public EdgeCoord EdgeCoord { get; private set; }

    public RoadLocationSelectionObject SelectionObject { get; set; }

    public HexEdgeObject EdgeObject { get; set; }

    public Road Road { get; private set; }

    public bool IsOccupied => Road != null;
    public IPlayer Owner => Road?.Owner;

    private List<HexTile> neighborHexes = null;
    private List<HexVertex> neighborVertices = null;
    private List<HexEdge> neighborEdges = null;

    public HexEdge(EdgeCoord edgeCoord)
    {
        EdgeCoord = edgeCoord;
    }

    public override string ToString()
    {
        return $"Edge {EdgeCoord}";
    }

    public void EnableSelection(bool enable, Color? hoverColor = null)
    {
        if (EdgeObject != null)
        {
            EdgeObject.EnableSelection(enable, hoverColor);
        }
    }

    public bool TryPlaceRoad(IPlayer owner)
    {
        if (IsOccupied || owner == null)
        {
            return false;
        }

        // Roads must be connected to an existing road or building with the same owner
        var success = false;
        
        // Check if connected to a building (settlement/city) owned by the player
        foreach (var neighbor in neighborVertices)
        {
            Debug.Log($"....neighbor vertex: {neighbor}, occupied = {neighbor.IsOccupied}, owner = {neighbor.Owner}");
            if (neighbor.IsOccupied && neighbor.Owner == owner)
            {
                success = true;
                break;
            }
        }

        // Check if connected to a road owned by the player
        if (!success)
        {
            foreach (var edge in neighborEdges)
            {
                Debug.Log($"....neighbor edge: {edge}, occupied = {edge.IsOccupied}, owner = {edge.Road?.Owner}");
                if (edge.IsOccupied && edge.Road.Owner == owner)
                {
                    success = true;
                    break;
                }
            }
        }

        if (success)
        {
            Road = new Road(this, owner);
            if (EdgeObject != null)
            {
                EdgeObject.Refresh();
            }
        }
        return success;
    }

    public bool CanHaveRoads()
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

    public bool AvailableForBuilding(IPlayer player, HexVertex requiredNeighborVertex = null)
    {
        if (!CanHaveRoads())
        {
            return false;
        }

        if (IsOccupied)
        {
            return false;
        }

        if (requiredNeighborVertex != null)
        {
            // Check if edge is connected to the required neighbor vertex
            foreach (var vertex in neighborVertices)
            {
                if (vertex == requiredNeighborVertex)
                {
                    return true;
                }
            }
        }
        else
        {
            // Check if edge is connected to a building owned by the player
            foreach (var vertex in neighborVertices)
            {
                if (vertex.IsOccupied && vertex.Owner == player)
                {
                    return true;
                }
            }

            // Check if edge is connected to a road owned by the player
            foreach (var edge in neighborEdges)
            {
                if (edge.IsOccupied && edge.Road.Owner == player)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void InitializeNeighbors(IBoardManager boardManager)
    {
        neighborHexes = new List<HexTile>();
        neighborVertices = new List<HexVertex>();
        neighborEdges = new List<HexEdge>();

        var q = EdgeCoord.HexCoord.q;
        var r = EdgeCoord.HexCoord.r;

        switch (EdgeCoord.Orientation)
        {
            case EdgeOrientation.West:
                {
                    TryAddNeighborHex(new HexCoord(q, r), boardManager);
                    TryAddNeighborHex(new HexCoord(q - 1, r), boardManager);

                    TryAddNeighborVertex(new VertexCoord(q, r - 1, VertexOrientation.North), boardManager);
                    TryAddNeighborVertex(new VertexCoord(q - 1, r + 1, VertexOrientation.South), boardManager);

                    TryAddNeighborEdge(new EdgeCoord(q - 1, r, EdgeOrientation.NorthEast), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q, r, EdgeOrientation.NorthWest), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q, r - 1, EdgeOrientation.NorthEast), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q, r - 1, EdgeOrientation.NorthWest), boardManager);
                    break;
                }
            case EdgeOrientation.NorthWest:
                {
                    TryAddNeighborHex(new HexCoord(q, r), boardManager);
                    TryAddNeighborHex(new HexCoord(q - 1, r + 1), boardManager);

                    TryAddNeighborVertex(new VertexCoord(q, r, VertexOrientation.North), boardManager);
                    TryAddNeighborVertex(new VertexCoord(q - 1, r + 1, VertexOrientation.South), boardManager);

                    TryAddNeighborEdge(new EdgeCoord(q - 1, r, EdgeOrientation.NorthEast), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q, r, EdgeOrientation.West), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q, r + 1, EdgeOrientation.West), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q, r, EdgeOrientation.NorthEast), boardManager);
                    break;
                }
            case EdgeOrientation.NorthEast:
                {
                    TryAddNeighborHex(new HexCoord(q, r), boardManager);
                    TryAddNeighborHex(new HexCoord(q, r + 1), boardManager);

                    TryAddNeighborVertex(new VertexCoord(q, r, VertexOrientation.North), boardManager);
                    TryAddNeighborVertex(new VertexCoord(q, r + 1, VertexOrientation.South), boardManager);

                    TryAddNeighborEdge(new EdgeCoord(q, r, EdgeOrientation.NorthWest), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q, r + 1, EdgeOrientation.West), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q + 1, r, EdgeOrientation.NorthWest), boardManager);
                    TryAddNeighborEdge(new EdgeCoord(q + 1, r, EdgeOrientation.West), boardManager);
                    
                    break;
                }
            default:
                {
                    Debug.LogError($"Unknown edge orientation in InitializeNeighborHexTiles for {this}");
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