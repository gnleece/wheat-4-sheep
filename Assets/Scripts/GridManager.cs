using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class GridManager : MonoBehaviour
{
    private struct AxialCoord
    {
        public int q;
        public int r;

        public AxialCoord(int q, int r)
        {
            this.q = q;
            this.r = r;
        }

        public override string ToString()
        {
            return $"({q},{r})";
        }

        public override int GetHashCode()
        {
            return (q, r).GetHashCode();
        }
    }

    private class HexTile
    {
        public AxialCoord AxialCoordinates { get; private set; }

        public GameObject TileObject { get; set; }

        public HexTile(AxialCoord coord)
        {
            AxialCoordinates = coord;
        }
    }

    private const int GRID_SIZE = 2;

    [SerializeField]
    private GameConfig gameConfig;

    [SerializeField]
    private float horizontalSpacing = 1.5f;

    [SerializeField]
    private float verticalSpacing = 1.5f;

    [SerializeField]
    private GameObject WoodTile;
    [SerializeField]
    private GameObject ClayTile;
    [SerializeField]
    private GameObject SheepTile;
    [SerializeField]
    private GameObject WheatTile;
    [SerializeField]
    private GameObject OreTile;
    [SerializeField]
    private GameObject DesertTile;

    private Dictionary<AxialCoord, HexTile> tileMap = new Dictionary<AxialCoord, HexTile>();

    private void Start()
    {
        InitializeGrid(GRID_SIZE);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ClearGrid();
            InitializeGrid(GRID_SIZE);
        }
    }
    
    private void InitializeGrid(int size)
    {
        for (int q = -size; q <= size; q++)
        {
            for (int r = -size; r <= size; r++)
            {
                if (q+r >= -size && q+r <= size)
                {
                    var coord = new AxialCoord(q, r);
                    var hexTile = new HexTile(coord);

                    tileMap.Add(coord, hexTile);
                }
            }
        }

        var tileTypesList = GetTileTypesList();
        var tileCount = 0;
        foreach (var tile in tileMap.Values)
        {
            var offsetCoord = AxialToOffsetCoord(tile.AxialCoordinates);
            
            var tilePosition = new Vector3(offsetCoord.x * horizontalSpacing, 0, offsetCoord.y * verticalSpacing);
            if (offsetCoord.y % 2 == 0)
            {
                tilePosition.x -= horizontalSpacing / 2;
            }

            var tilePrefab = GetTilePrefab(tileTypesList[tileCount]);

            var tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
            tile.TileObject = tileObject;

            tileCount++;
        }
    }

    private void ClearGrid()
    {
        foreach (var tile in tileMap.Values)
        {
            Destroy(tile.TileObject);
        }

        tileMap.Clear();
    }

    private List<ResourceType> GetTileTypesList()
    {
        var tileCounts = gameConfig.GetTileCounts();
        var tileTypesList = new List<ResourceType>();
        foreach (var type in tileCounts.Keys)
        {
            var count = tileCounts[type];
            tileTypesList.AddRange(Enumerable.Repeat(type, count));
        }

        Util.Shuffle(tileTypesList);

        return tileTypesList;
    }

    private GameObject GetTilePrefab(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood:     return WoodTile;
            case ResourceType.Clay:     return ClayTile;
            case ResourceType.Sheep:    return SheepTile;
            case ResourceType.Wheat:    return WheatTile;
            case ResourceType.Ore:      return OreTile;
            case ResourceType.None:     return DesertTile;
            default:                    return null;
        }
    }

    private Vector2 AxialToOffsetCoord(AxialCoord axialCoord)
    {
        var col = axialCoord.q + (axialCoord.r - (axialCoord.r & 1)) / 2;
        var row = axialCoord.r;
        return new Vector2(col, row);
    }
}
