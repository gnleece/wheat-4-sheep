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

    public IReadOnlyList<HexTile> NeighborHexTiles => _neighborHexes;
    public IReadOnlyList<HexVertex> NeighborVertices => _neighborVertices;
    public IReadOnlyList<HexEdge> NeighborEdges => _neighborEdges;

    private List<HexTile> _neighborHexes = null;
    private List<HexVertex> _neighborVertices = null;
    private List<HexEdge> _neighborEdges = null;

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

    public bool CanHaveRoads()
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

    public void PlaceRoad(Road road)
    {
        Road = road;
        if (EdgeObject != null)
        {
            EdgeObject.Refresh();
        }
    }

    public void InitializeNeighbors(IBoardManager boardManager)
    {
        _neighborHexes = new List<HexTile>();
        _neighborVertices = new List<HexVertex>();
        _neighborEdges = new List<HexEdge>();

        foreach (var hexCoord in GridHelpers.GetEdgeNeighborHexCoords(EdgeCoord))
        {
            TryAddNeighborHex(hexCoord, boardManager);
        }

        foreach (var vertexCoord in GridHelpers.GetEdgeNeighborVertexCoords(EdgeCoord))
        {
            TryAddNeighborVertex(vertexCoord, boardManager);
        }

        foreach (var edgeCoord in GridHelpers.GetEdgeNeighborEdgeCoords(EdgeCoord))
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
