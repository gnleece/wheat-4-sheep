using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private struct AxialCoord
    {
        public int p;
        public int q;

        public AxialCoord(int p, int q)
        {
            this.p = p;
            this.q = q;
        }

        public override string ToString()
        {
            return $"({p},{q})";
        }

        public override int GetHashCode()
        {
            return (p, q).GetHashCode();
        }
    }

    private class HexTile
    {
        private AxialCoord coord;

        public HexTile(AxialCoord coord)
        {
            this.coord = coord;
        }
    }

    private Dictionary<AxialCoord, HexTile> tileMap = new Dictionary<AxialCoord, HexTile>();

    private void Start()
    {
        InitializeGrid(2);
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
    }
}
