using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    #region Enums

    public enum VertexOrientation
    {
        North,
        South
    }

    public enum EdgeOrientation
    {
        West,
        NorthWest,
        NorthEast
    }

    #endregion

    #region Classes and structs

    public struct HexCoord
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

    public struct VertexCoord
    {
        public HexCoord HexCoord;
        public VertexOrientation Orientation;

        public VertexCoord(HexCoord hexCoord, VertexOrientation orientation)
        {
            HexCoord = hexCoord;
            Orientation = orientation;
        }

        public VertexCoord(int q, int r, VertexOrientation orientation)
        {
            HexCoord = new HexCoord(q, r);
            Orientation = orientation;
        }

        public override string ToString() => $"({HexCoord.q},{HexCoord.r},{Orientation})";

        public override int GetHashCode() => (HexCoord.q, HexCoord.r, Orientation).GetHashCode();
    }

    public struct EdgeCoord
    {
        public HexCoord HexCoord;
        public EdgeOrientation Orientation;

        public EdgeCoord(HexCoord hexCoord, EdgeOrientation edgeOrientation)
        {
            HexCoord = hexCoord;
            Orientation = edgeOrientation;
        }

        public EdgeCoord(int q, int r, EdgeOrientation orientation)
        {
            HexCoord = new HexCoord(q, r);
            Orientation = orientation;
        }

        public override string ToString() => $"({HexCoord.q},{HexCoord.r},{Orientation})";

        public override int GetHashCode() => (HexCoord.q, HexCoord.r, Orientation).GetHashCode();
    }

    public class HexTile
    {
        public HexCoord HexCoordinates { get; private set; }
        public bool CanHaveBuildingsAndRoads { get; private set; }
        public int Ring => HexCoordinates.Ring;

        public HexTileObject TileObject { get; set; }

        public HexTile(HexCoord coord, bool isValidForBuilding)
        {
            HexCoordinates = coord;
            CanHaveBuildingsAndRoads = isValidForBuilding;
        }

        public void SetDebugText(string text)
        {
            if (TileObject != null)
            {
                TileObject.SetDebugText(text);
            }
        }
    }

    public class HexVertex
    {
        public VertexCoord VertexCoord { get; private set; }

        public GameObject SelectionObject { get; set; }

        public Building Building { get; private set; }

        public bool IsOccupied => Building != null;

        public IPlayer Owner => Building?.Owner;


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
            return true;
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

        public void InitializeNeighbors(IGrid grid)
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
                        TryAddNeighborHex(new HexCoord(q,r), grid);
                        TryAddNeighborHex(new HexCoord(q, r + 1), grid);
                        TryAddNeighborHex(new HexCoord(q - 1, r + 1), grid);

                        TryAddNeighborVertex(new VertexCoord(q - 1, r + 1, VertexOrientation.South), grid);
                        TryAddNeighborVertex(new VertexCoord(q - 1, r + 2, VertexOrientation.South), grid);
                        TryAddNeighborVertex(new VertexCoord(q, r + 1, VertexOrientation.South), grid);

                        TryAddNeighborEdge(new EdgeCoord(q, r + 1, EdgeOrientation.NorthWest), grid);
                        TryAddNeighborEdge(new EdgeCoord(q - 1, r + 1, EdgeOrientation.NorthEast), grid);
                        TryAddNeighborEdge(new EdgeCoord(q, r + 1, EdgeOrientation.West), grid);
                        break;
                    }
                case VertexOrientation.South:
                    {
                        TryAddNeighborHex(new HexCoord(q, r), grid);
                        TryAddNeighborHex(new HexCoord(q, r - 1), grid);
                        TryAddNeighborHex(new HexCoord(q + 1, r - 1), grid);

                        TryAddNeighborVertex(new VertexCoord(q + 1, r - 2, VertexOrientation.North), grid);
                        TryAddNeighborVertex(new VertexCoord(q, r - 1, VertexOrientation.North), grid);
                        TryAddNeighborVertex(new VertexCoord(q + 1, r - 1, VertexOrientation.North), grid);

                        TryAddNeighborEdge(new EdgeCoord(q, r, EdgeOrientation.NorthEast), grid);
                        TryAddNeighborEdge(new EdgeCoord(q + 1, r - 1, EdgeOrientation.West), grid);
                        TryAddNeighborEdge(new EdgeCoord(q, r, EdgeOrientation.NorthWest), grid);
                        break;
                    }
                default:
                    {
                        Debug.LogError($"Unknown vertex orientation in InitializeNeighbors for {this}");
                        break;
                    }
            }
        }

        private void TryAddNeighborHex(HexCoord hexCoord, IGrid grid)
        {
            if (grid.HexMap.TryGetValue(hexCoord, out var hex))
            {
                neighborHexes.Add(hex);
            }
        }

        private void TryAddNeighborVertex(VertexCoord vertexCoord, IGrid grid)
        {
            if (grid.VertexMap.TryGetValue(vertexCoord, out var vertex))
            {
                neighborVertices.Add(vertex);
            }
        }

        private void TryAddNeighborEdge(EdgeCoord edgeCoord, IGrid grid)
        {
            if (grid.EdgeMap.TryGetValue(edgeCoord, out var edge))
            {
                neighborEdges.Add(edge);
            }
        }
    }

    public class HexEdge
    {
        public EdgeCoord EdgeCoord { get; private set; }

        public GameObject SelectionObject { get; set; }

        public Road Road { get; private set; }

        public bool IsOccupied => Road != null;

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

        public bool PlaceRoad(IPlayer owner)
        {
            if (IsOccupied || owner == null)
            {
                return false;
            }

            // Roads must be connected to an existing road or building with the same owner
            var success = false;
            foreach (var neighbor in neighborVertices)
            {
                Debug.Log($"....neighbor vertex: {neighbor}, occupied = {neighbor.IsOccupied}, owner = {neighbor.Owner}");
                if (neighbor.IsOccupied && neighbor.Owner == owner)
                {
                    success = true;
                    break;
                }
            }

            // TODO check for neighboring roads

            if (success)
            {
                Road = new Road(this, owner);
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

        public void InitializeNeighbors(IGrid grid)
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
                        TryAddNeighborHex(new HexCoord(q, r), grid);
                        TryAddNeighborHex(new HexCoord(q - 1, r), grid);

                        TryAddNeighborVertex(new VertexCoord(q, r - 1, VertexOrientation.North), grid);
                        TryAddNeighborVertex(new VertexCoord(q - 1, r + 1, VertexOrientation.South), grid);
                        break;
                    }
                case EdgeOrientation.NorthWest:
                    {
                        TryAddNeighborHex(new HexCoord(q, r), grid);
                        TryAddNeighborHex(new HexCoord(q - 1, r + 1), grid);

                        TryAddNeighborVertex(new VertexCoord(q, r, VertexOrientation.North), grid);
                        TryAddNeighborVertex(new VertexCoord(q - 1, r + 1, VertexOrientation.South), grid);
                        break;
                    }
                case EdgeOrientation.NorthEast:
                    {
                        TryAddNeighborHex(new HexCoord(q, r), grid);
                        TryAddNeighborHex(new HexCoord(q, r + 1), grid);

                        TryAddNeighborVertex(new VertexCoord(q, r, VertexOrientation.North), grid);
                        TryAddNeighborVertex(new VertexCoord(q, r + 1, VertexOrientation.South), grid);
                        break;
                    }
                default:
                    {
                        Debug.LogError($"Unknown edge orientation in InitializeNeighborHexTiles for {this}");
                        break;
                    }
            }
        }

        private void TryAddNeighborHex(HexCoord hexCoord, IGrid grid)
        {
            if (grid.HexMap.TryGetValue(hexCoord, out var hex))
            {
                neighborHexes.Add(hex);
            }
        }

        private void TryAddNeighborVertex(VertexCoord vertexCoord, IGrid grid)
        {
            if (grid.VertexMap.TryGetValue(vertexCoord, out var vertex))
            {
                neighborVertices.Add(vertex);
            }
        }

        private void TryAddNeighborEdge(EdgeCoord edgeCoord, IGrid grid)
        {
            if (grid.EdgeMap.TryGetValue(edgeCoord, out var edge))
            {
                neighborEdges.Add(edge);
            }
        }
    }

    #endregion

    #region Helper methods

    public static class GridHelpers
    {
        public static Vector2 AxialHexToOffsetCoord(HexCoord axialCoord)
        {
            var col = axialCoord.q + (axialCoord.r - (axialCoord.r & 1)) / 2;
            var row = axialCoord.r;
            return new Vector2(col, row);
        }

    }

    #endregion
}