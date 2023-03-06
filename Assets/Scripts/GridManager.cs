using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private GameObject tilePrefab;

    [SerializeField]
    private float horizontalSpacing = 1.5f;

    [SerializeField]
    private float verticalSpacing = 1.5f;

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

        foreach (var tile in tileMap.Values)
        {
            var offsetCoord = AxialToOffsetCoord(tile.AxialCoordinates);
            
            var tilePosition = new Vector3(offsetCoord.x * horizontalSpacing, 0, offsetCoord.y * verticalSpacing);
            if (offsetCoord.y % 2 == 0)
            {
                tilePosition.x -= horizontalSpacing / 2;
            }
            var tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
            tile.TileObject = tileObject;
        }
    }

    private void ClearGrid()
    {
        foreach (var tile in tileMap.Values)
        {
            GameObject.Destroy(tile.TileObject);
        }

        tileMap.Clear();
    }

    private Vector2 AxialToOffsetCoord(AxialCoord axialCoord)
    {
        var col = axialCoord.q + (axialCoord.r - (axialCoord.r & 1)) / 2;
        var row = axialCoord.r;
        return new Vector2(col, row);
    }
}
