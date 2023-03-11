using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class GridManager : MonoBehaviour
{
    private enum VertexOrientation
    {
        North,
        South
    }

    private struct AxialCoord
    {
        public int q;
        public int r;

        public int s => -q - r;

        public int Ring => Mathf.Max(Mathf.Abs(q), Mathf.Abs(r), Mathf.Abs(s));

        public AxialCoord(int q, int r)
        {
            this.q = q;
            this.r = r;
        }

        public override string ToString() => $"({q},{r})";

        public override int GetHashCode() => (q, r).GetHashCode();
    }

    private struct VertexCoord
    {
        public AxialCoord AxialCoord;
        public VertexOrientation Orientation;

        public VertexCoord(AxialCoord axialCoord, VertexOrientation orientation)
        {
            AxialCoord = axialCoord;
            Orientation = orientation;
        }

        public override string ToString() => $"({AxialCoord.q},{AxialCoord.r},{Orientation})";

        public override int GetHashCode() => (AxialCoord.q, AxialCoord.r, Orientation).GetHashCode();
    }

    private class HexTile
    {
        public AxialCoord AxialCoordinates { get; private set; }
        public int Ring => AxialCoordinates.Ring;

        public HexTileObject TileObject { get; set; }

        public HexTile(AxialCoord coord)
        {
            AxialCoordinates = coord;
        }
    }

    private class HexVertex
    {
        public VertexCoord VertexCoord { get; private set; }

        public GameObject VertexObject { get; set; }

        public HexVertex(VertexCoord coord)
        {
            this.VertexCoord = coord;
        }
    }

    private const int INNER_SHUFFLEABLE_GRID_SIZE = 2;
    private const int FULL_GRID_SIZE = 3;

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

    private Dictionary<AxialCoord, HexTile> hexMap = new Dictionary<AxialCoord, HexTile>();
    private Dictionary<VertexCoord, HexVertex> vertexMap = new Dictionary<VertexCoord, HexVertex>();

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
                    var coord = new AxialCoord(q, r);
                    var hexTile = new HexTile(coord);
                    hexMap.Add(coord, hexTile);

                    var northVertexCoord = new VertexCoord(coord, VertexOrientation.North);
                    var northVertex = new HexVertex(northVertexCoord);
                    vertexMap.Add(northVertexCoord, northVertex);

                    var southVertexCoord = new VertexCoord(coord, VertexOrientation.South);
                    var southVertex = new HexVertex(southVertexCoord);
                    vertexMap.Add(southVertexCoord, southVertex);
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
            var offsetCoord = AxialToOffsetCoord(hex.AxialCoordinates);
            
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
        }

        // Spawn vertex objects
        foreach (var vertex in vertexMap.Values)
        {
            if (hexMap.TryGetValue(vertex.VertexCoord.AxialCoord, out var hex))
            {
                var vertexObject = Instantiate(HexVertexPrefab, Vector3.zero, Quaternion.identity);
                if (vertex.VertexCoord.Orientation == VertexOrientation.North)
                {
                    vertexObject.transform.parent = hex.TileObject.NorthVertexTransform;
                }
                else
                {
                    vertexObject.transform.parent = hex.TileObject.SouthVertexTransform;
                }
                
                vertexObject.transform.localPosition = Vector3.zero;
                vertex.VertexObject  = vertexObject;
            }
            else
            {
                Debug.LogError($"Could not find parent hex for vertex {vertex}");
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

    private Vector2 AxialToOffsetCoord(AxialCoord axialCoord)
    {
        var col = axialCoord.q + (axialCoord.r - (axialCoord.r & 1)) / 2;
        var row = axialCoord.r;
        return new Vector2(col, row);
    }
}
