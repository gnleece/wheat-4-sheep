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

    public Port Port { get; set; }

    public IReadOnlyList<HexTile> NeighborHexTiles => neighborHexes;
    public IReadOnlyList<HexVertex> NeighborVertices => neighborVertices;
    public IReadOnlyList<HexEdge> NeighborEdges => neighborEdges;

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

    public void PlaceBuilding(Building building)
    {
        Building = building;
        if (VertexObject != null)
        {
            VertexObject.Refresh();
        }
    }

    public void InitializeNeighbors(IBoardManager boardManager)
    {
        neighborHexes = new List<HexTile>();
        neighborVertices = new List<HexVertex>();
        neighborEdges = new List<HexEdge>();

        foreach (var hexCoord in GridHelpers.GetVertexNeighborHexCoords(VertexCoord))
        {
            TryAddNeighborHex(hexCoord, boardManager);
        }

        foreach (var vertexCoord in GridHelpers.GetVertexNeighborVertexCoords(VertexCoord))
        {
            TryAddNeighborVertex(vertexCoord, boardManager);
        }

        foreach (var edgeCoord in GridHelpers.GetVertexNeighborEdgeCoords(VertexCoord))
        {
            TryAddNeighborEdge(edgeCoord, boardManager);
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
