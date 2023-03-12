using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class GridManager : MonoBehaviour
{
    #region Enums

    private enum VertexOrientation
    {
        North,
        South
    }

    private enum EdgeOrientation
    {
        West,
        NorthWest,
        NorthEast
    }

    #endregion

    #region Classes and structs

    private struct HexCoord
    {
        public int q;
        public int r;

        public int s => -q - r;

        public int Ring => Mathf.Max(Mathf.Abs(q), Mathf.Abs(r), Mathf.Abs(s));

        public HexCoord(int q, int r)
        {
            this.q = q;
            this.r = r;
        }

        public override string ToString() => $"({q},{r})";

        public override int GetHashCode() => (q, r).GetHashCode();
    }

    private struct VertexCoord
    {
        public HexCoord HexCoord;
        public VertexOrientation Orientation;

        public VertexCoord(HexCoord hexCoord, VertexOrientation orientation)
        {
            HexCoord = hexCoord;
            Orientation = orientation;
        }

        public override string ToString() => $"({HexCoord.q},{HexCoord.r},{Orientation})";

        public override int GetHashCode() => (HexCoord.q, HexCoord.r, Orientation).GetHashCode();
    }

    private struct EdgeCoord
    {
        public HexCoord HexCoord;
        public EdgeOrientation Orientation;

        public EdgeCoord(HexCoord hexCoord, EdgeOrientation edgeOrientation)
        {
            HexCoord = hexCoord;
            Orientation = edgeOrientation;
        }

        public override string ToString() => $"({HexCoord.q},{HexCoord.r},{Orientation})";

        public override int GetHashCode() => (HexCoord.q, HexCoord.r, Orientation).GetHashCode();
    }

    private class HexTile
    {
        public HexCoord HexCoordinates { get; private set; }
        public bool IsValidForBuilding { get; private set; }
        public int Ring => HexCoordinates.Ring;

        public HexTileObject TileObject { get; set; }

        public HexTile(HexCoord coord, bool isValidForBuilding)
        {
            HexCoordinates = coord;
            IsValidForBuilding = isValidForBuilding;
        }

        public void SetDebugText(string text)
        {
            if (TileObject != null)
            {
                TileObject.SetDebugText(text);
            }
        }
    }

    private class HexVertex
    {
        public VertexCoord VertexCoord { get; private set; }

        public GameObject VertexObject { get; set; }

        public List<HexTile> NeighborTiles { get; private set; }

        public HexVertex(VertexCoord coord)
        {
            VertexCoord = coord;
        }

        public override string ToString()
        {
            return $"Vertex {VertexCoord}";
        }

        public bool GetIsValidForBuilding()
        {
            if (NeighborTiles == null || NeighborTiles.Count == 0)
            {
                Debug.LogWarning($"No neighbor tiles initialized for {this}");
                return false;
            }

            foreach (var hex in NeighborTiles)
            {
                if (hex.IsValidForBuilding)
                {
                    return true;
                }
            }

            return false;
        }

        public void InitializeNeighborTiles(Dictionary<HexCoord,HexTile> hexMap)
        {
            NeighborTiles = new List<HexTile>();

            var q = VertexCoord.HexCoord.q;
            var r = VertexCoord.HexCoord.r;

            switch (VertexCoord.Orientation)
            {
                case VertexOrientation.North:
                {
                    TryAddNeighborHex(q, r, hexMap);
                    TryAddNeighborHex(q, r + 1, hexMap);
                    TryAddNeighborHex(q - 1, r + 1, hexMap);
                    break;
                }
                case VertexOrientation.South:
                {
                    TryAddNeighborHex(q, r, hexMap);
                    TryAddNeighborHex(q, r - 1, hexMap);
                    TryAddNeighborHex(q + 1, r - 1, hexMap);
                    break;
                }
                default:
                {
                    Debug.LogError($"Unknown vertex orientation in InitializeNeighborTiles for {this}");
                    break;
                }
            }
        }

        private void TryAddNeighborHex(int q, int r, Dictionary<HexCoord, HexTile> hexMap)
        {
            if (hexMap.TryGetValue(new HexCoord(q, r), out var hex))
            {
                NeighborTiles.Add(hex);
            }
        }
    }

    private class HexEdge
    {
        public EdgeCoord EdgeCoord { get; private set; }

        public GameObject EdgeObject { get; set; }

        public List<HexTile> NeighborTiles { get; private set; }

        public HexEdge(EdgeCoord edgeCoord)
        {
            EdgeCoord = edgeCoord;
        }

        public override string ToString()
        {
            return $"Edge {EdgeCoord}";
        }

        public bool GetIsValidForBuilding()
        {
            if (NeighborTiles == null || NeighborTiles.Count == 0)
            {
                Debug.LogWarning($"No neighbor tiles initialized for {this}");
                return false;
            }

            foreach (var hex in NeighborTiles)
            {
                if (hex.IsValidForBuilding)
                {
                    return true;
                }
            }

            return false;
        }

        public void InitializeNeighborTiles(Dictionary<HexCoord, HexTile> hexMap)
        {
            NeighborTiles = new List<HexTile>();

            var q = EdgeCoord.HexCoord.q;
            var r = EdgeCoord.HexCoord.r;

            switch (EdgeCoord.Orientation)
            {
                case EdgeOrientation.West:
                {
                    TryAddNeighborHex(q, r, hexMap);
                    TryAddNeighborHex(q - 1, r, hexMap);
                    break;
                }
                case EdgeOrientation.NorthWest:
                {
                    TryAddNeighborHex(q, r, hexMap);
                    TryAddNeighborHex(q - 1, r + 1, hexMap);
                    break;
                }
                case EdgeOrientation.NorthEast:
                {
                    TryAddNeighborHex(q, r, hexMap);
                    TryAddNeighborHex(q, r + 1, hexMap);
                    break;
                }
                default:
                {
                    Debug.LogError($"Unknown edge orientation in InitializeNeighborTiles for {this}");
                    break;
                }
            }
        }

        private void TryAddNeighborHex(int q, int r, Dictionary<HexCoord, HexTile> hexMap)
        {
            if (hexMap.TryGetValue(new HexCoord(q, r), out var hex))
            {
                NeighborTiles.Add(hex);
            }
        }
    }

    #endregion

    #region Consts

    private const int INNER_SHUFFLEABLE_GRID_SIZE = 2;
    private const int FULL_GRID_SIZE = 3;

    #endregion

    #region Serialized fields

    [SerializeField]
    private GameConfig gameConfig;

    [SerializeField]
    private float horizontalSpacing = 1.5f;

    [SerializeField]
    private float verticalSpacing = 1.5f;

    [SerializeField]
    private GameObject WoodTilePrefab;
    [SerializeField]
    private GameObject ClayTilePrefab;
    [SerializeField]
    private GameObject SheepTilePrefab;
    [SerializeField]
    private GameObject WheatTilePrefab;
    [SerializeField]
    private GameObject OreTilePrefab;
    [SerializeField]
    private GameObject DesertTilePrefab;
    [SerializeField]
    private GameObject WaterTilePrefab;

    [SerializeField]
    private GameObject HexVertexPrefab;
    [SerializeField]
    private GameObject HexEdgePrefab;

    #endregion

    private Dictionary<HexCoord, HexTile> hexMap = new Dictionary<HexCoord, HexTile>();
    private Dictionary<VertexCoord, HexVertex> vertexMap = new Dictionary<VertexCoord, HexVertex>();
    private Dictionary<EdgeCoord, HexEdge> edgeMap = new Dictionary<EdgeCoord, HexEdge>();

    private void Start()
    {
        InitializeGrid(INNER_SHUFFLEABLE_GRID_SIZE, FULL_GRID_SIZE);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ClearGrid();
            InitializeGrid(INNER_SHUFFLEABLE_GRID_SIZE, FULL_GRID_SIZE);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ClearGrid();
        }
    }
    
    private void InitializeGrid(int shuffleableGridSize, int fullGridSize)
    {
        // Initialize data structure for full grid
        for (int q = -fullGridSize; q <= fullGridSize; q++)
        {
            for (int r = -fullGridSize; r <= fullGridSize; r++)
            {
                if (q+r >= -fullGridSize && q+r <= fullGridSize)
                {
                    // Hex
                    var coord = new HexCoord(q, r);
                    var isValidForBuilding = coord.Ring <= shuffleableGridSize;
                    var hexTile = new HexTile(coord, isValidForBuilding);
                    hexMap.Add(coord, hexTile);

                    // Verts
                    var northVertexCoord = new VertexCoord(coord, VertexOrientation.North);
                    var northVertex = new HexVertex(northVertexCoord);
                    vertexMap.Add(northVertexCoord, northVertex);

                    var southVertexCoord = new VertexCoord(coord, VertexOrientation.South);
                    var southVertex = new HexVertex(southVertexCoord);
                    vertexMap.Add(southVertexCoord, southVertex);

                    // Edges
                    var westEdgeCoord = new EdgeCoord(coord, EdgeOrientation.West);
                    var westEdge = new HexEdge(westEdgeCoord);
                    edgeMap.Add(westEdgeCoord, westEdge);

                    var northWestEdgeCoord = new EdgeCoord(coord, EdgeOrientation.NorthWest);
                    var northWestEdge = new HexEdge(northWestEdgeCoord);
                    edgeMap.Add(northWestEdgeCoord, northWestEdge);

                    var northEastEdgeCoord = new EdgeCoord(coord, EdgeOrientation.NorthEast);
                    var northEastEdge = new HexEdge(northEastEdgeCoord);
                    edgeMap.Add(northEastEdgeCoord, northEastEdge);
                }
            }
        }

        // Spawn tile objects for the full grid.
        // Tiles within the inner shuffleable region will get randomized tiles.
        // Tiles outside the shuffleable area witll get water tiles.
        var tileTypesList = GetShuffledTileTypes();
        var shuffledTileCount = 0;
        foreach (var hex in hexMap.Values)
        {
            var offsetCoord = AxialHexToOffsetCoord(hex.HexCoordinates);
            
            var tilePosition = new Vector3(offsetCoord.x * horizontalSpacing, 0, offsetCoord.y * verticalSpacing);
            if (offsetCoord.y % 2 == 0)
            {
                tilePosition.x -= horizontalSpacing / 2;
            }

            var tilePrefab = WaterTilePrefab;
            if (hex.Ring <= shuffleableGridSize)
            {
                tilePrefab = GetTilePrefab(tileTypesList[shuffledTileCount]);
                shuffledTileCount++;
            }

            var tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
            var tileObjectComponent = tileObject.GetComponent<HexTileObject>();
            hex.TileObject = tileObjectComponent;

            tileObjectComponent.SetDebugText($"{hex.HexCoordinates}\n{hex.IsValidForBuilding}");
        }

        // Spawn vertex objects
        foreach (var vertex in vertexMap.Values)
        {
            if (hexMap.TryGetValue(vertex.VertexCoord.HexCoord, out var hex))
            {
                vertex.InitializeNeighborTiles(hexMap);
                if (!vertex.GetIsValidForBuilding())
                {
                    continue;
                }

                var vertexObject = Instantiate(HexVertexPrefab, Vector3.zero, Quaternion.identity);

                switch (vertex.VertexCoord.Orientation)
                {
                    case VertexOrientation.North:
                        vertexObject.transform.parent = hex.TileObject.NorthVertexTransform;
                        break;
                    case VertexOrientation.South:
                        vertexObject.transform.parent = hex.TileObject.SouthVertexTransform;
                        break;
                    default:
                        Debug.LogError($"Unknown vertex orientation for {vertex}");
                        break;
                }
                
                vertexObject.transform.localPosition = Vector3.zero;
                vertex.VertexObject  = vertexObject;
            }
            else
            {
                Debug.LogError($"Could not find parent hex for {vertex}");
            }
        }

        // Spawn edge objects
        foreach (var edge in edgeMap.Values)
        {
            if (hexMap.TryGetValue(edge.EdgeCoord.HexCoord, out var hex))
            {
                edge.InitializeNeighborTiles(hexMap);
                if (!edge.GetIsValidForBuilding())
                {
                    continue;
                }

                var edgeObject = Instantiate(HexEdgePrefab, Vector3.zero, Quaternion.identity);
                switch(edge.EdgeCoord.Orientation)
                {
                    case EdgeOrientation.West:
                        edgeObject.transform.parent = hex.TileObject.WestEdgeTransform;
                        break;
                    case EdgeOrientation.NorthWest:
                        edgeObject.transform.parent = hex.TileObject.NorthWestEdgeTransform;
                        break;
                    case EdgeOrientation.NorthEast:
                        edgeObject.transform.parent = hex.TileObject.NorthEastEdgeTransform;
                        break;
                    default:
                        Debug.LogError($"Unknown edge orientation for {edge}");
                        break;
                }

                edgeObject.transform.localPosition = Vector3.zero;
                edgeObject.transform.localRotation = Quaternion.identity;
                edge.EdgeObject = edgeObject;
            }
            else
            {
                Debug.LogError($"Could not find parent hex for {edge}");
            }
        }
    }

    private void ClearGrid()
    {
        foreach (var tile in hexMap.Values)
        {
            Destroy(tile.TileObject.gameObject);
        }
        hexMap.Clear();

        foreach (var vertex in vertexMap.Values)
        {
            Destroy(vertex.VertexObject);
        }
        vertexMap.Clear();

        foreach (var edge in edgeMap.Values)
        {
            Destroy(edge.EdgeObject);
        }
        edgeMap.Clear();
    }

    private List<TileType> GetShuffledTileTypes()
    {
        var tileCounts = gameConfig.GetShuffleableTileCounts();
        var tileTypesList = new List<TileType>();
        foreach (var type in tileCounts.Keys)
        {
            var count = tileCounts[type];
            tileTypesList.AddRange(Enumerable.Repeat(type, count));
        }

        Util.Shuffle(tileTypesList);

        return tileTypesList;
    }

    private GameObject GetTilePrefab(TileType type)
    {
        switch (type)
        {
            case TileType.Wood:     return WoodTilePrefab;
            case TileType.Clay:     return ClayTilePrefab;
            case TileType.Sheep:    return SheepTilePrefab;
            case TileType.Wheat:    return WheatTilePrefab;
            case TileType.Ore:      return OreTilePrefab;
            case TileType.Desert:   return DesertTilePrefab;
            case TileType.Water:    return WaterTilePrefab;
            default:                return null;
        }
    }

    private Vector2 AxialHexToOffsetCoord(HexCoord axialCoord)
    {
        var col = axialCoord.q + (axialCoord.r - (axialCoord.r & 1)) / 2;
        var row = axialCoord.r;
        return new Vector2(col, row);
    }
}
