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

    public IReadOnlyList<HexTile> NeighborHexTiles => _neighborHexes;
    public IReadOnlyList<HexVertex> NeighborVertices => _neighborVertices;
    public IReadOnlyList<HexEdge> NeighborEdges => _neighborEdges;

    private List<HexTile> _neighborHexes;
    private List<HexVertex> _neighborVertices;
    private List<HexEdge> _neighborEdges;

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
        if (_neighborHexes == null || _neighborHexes.Count == 0)
        {
            Debug.LogWarning($"No neighbor hex tiles initialized for {this}");
            return false;
        }

        foreach (var hex in _neighborHexes)
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
        _neighborHexes = new List<HexTile>();
        _neighborVertices = new List<HexVertex>();
        _neighborEdges = new List<HexEdge>();

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
            _neighborHexes.Add(hex);
        }
    }

    private void TryAddNeighborVertex(VertexCoord vertexCoord, IBoardManager boardManager)
    {
        if (boardManager.VertexMap.TryGetValue(vertexCoord, out var vertex))
        {
            _neighborVertices.Add(vertex);
        }
    }

    private void TryAddNeighborEdge(EdgeCoord edgeCoord, IBoardManager boardManager)
    {
        if (boardManager.EdgeMap.TryGetValue(edgeCoord, out var edge))
        {
            _neighborEdges.Add(edge);
        }
    }
}
