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

        public static HexCoord[] GetVertexNeighborHexCoords(VertexCoord vc)
        {
            int q = vc.HexCoord.q;
            int r = vc.HexCoord.r;

            switch (vc.Orientation)
            {
                case VertexOrientation.North:
                    return new[]
                    {
                        new HexCoord(q, r),
                        new HexCoord(q, r + 1),
                        new HexCoord(q - 1, r + 1),
                    };
                case VertexOrientation.South:
                    return new[]
                    {
                        new HexCoord(q, r),
                        new HexCoord(q, r - 1),
                        new HexCoord(q + 1, r - 1),
                    };
                default:
                    Debug.LogError($"Unknown vertex orientation: {vc.Orientation}");
                    return System.Array.Empty<HexCoord>();
            }
        }

        public static VertexCoord[] GetVertexNeighborVertexCoords(VertexCoord vc)
        {
            int q = vc.HexCoord.q;
            int r = vc.HexCoord.r;

            switch (vc.Orientation)
            {
                case VertexOrientation.North:
                    return new[]
                    {
                        new VertexCoord(q - 1, r + 1, VertexOrientation.South),
                        new VertexCoord(q - 1, r + 2, VertexOrientation.South),
                        new VertexCoord(q, r + 1, VertexOrientation.South),
                    };
                case VertexOrientation.South:
                    return new[]
                    {
                        new VertexCoord(q + 1, r - 2, VertexOrientation.North),
                        new VertexCoord(q, r - 1, VertexOrientation.North),
                        new VertexCoord(q + 1, r - 1, VertexOrientation.North),
                    };
                default:
                    Debug.LogError($"Unknown vertex orientation: {vc.Orientation}");
                    return System.Array.Empty<VertexCoord>();
            }
        }

        public static EdgeCoord[] GetVertexNeighborEdgeCoords(VertexCoord vc)
        {
            int q = vc.HexCoord.q;
            int r = vc.HexCoord.r;

            switch (vc.Orientation)
            {
                case VertexOrientation.North:
                    return new[]
                    {
                        new EdgeCoord(q, r, EdgeOrientation.NorthWest),
                        new EdgeCoord(q, r, EdgeOrientation.NorthEast),
                        new EdgeCoord(q, r + 1, EdgeOrientation.West),
                    };
                case VertexOrientation.South:
                    return new[]
                    {
                        new EdgeCoord(q, r - 1, EdgeOrientation.NorthEast),
                        new EdgeCoord(q + 1, r - 1, EdgeOrientation.West),
                        new EdgeCoord(q + 1, r - 1, EdgeOrientation.NorthWest),
                    };
                default:
                    Debug.LogError($"Unknown vertex orientation: {vc.Orientation}");
                    return System.Array.Empty<EdgeCoord>();
            }
        }

        public static HexCoord[] GetEdgeNeighborHexCoords(EdgeCoord ec)
        {
            int q = ec.HexCoord.q;
            int r = ec.HexCoord.r;

            switch (ec.Orientation)
            {
                case EdgeOrientation.West:
                    return new[]
                    {
                        new HexCoord(q, r),
                        new HexCoord(q - 1, r),
                    };
                case EdgeOrientation.NorthWest:
                    return new[]
                    {
                        new HexCoord(q, r),
                        new HexCoord(q - 1, r + 1),
                    };
                case EdgeOrientation.NorthEast:
                    return new[]
                    {
                        new HexCoord(q, r),
                        new HexCoord(q, r + 1),
                    };
                default:
                    Debug.LogError($"Unknown edge orientation: {ec.Orientation}");
                    return System.Array.Empty<HexCoord>();
            }
        }

        public static VertexCoord[] GetEdgeNeighborVertexCoords(EdgeCoord ec)
        {
            int q = ec.HexCoord.q;
            int r = ec.HexCoord.r;

            switch (ec.Orientation)
            {
                case EdgeOrientation.West:
                    return new[]
                    {
                        new VertexCoord(q, r - 1, VertexOrientation.North),
                        new VertexCoord(q - 1, r + 1, VertexOrientation.South),
                    };
                case EdgeOrientation.NorthWest:
                    return new[]
                    {
                        new VertexCoord(q, r, VertexOrientation.North),
                        new VertexCoord(q - 1, r + 1, VertexOrientation.South),
                    };
                case EdgeOrientation.NorthEast:
                    return new[]
                    {
                        new VertexCoord(q, r, VertexOrientation.North),
                        new VertexCoord(q, r + 1, VertexOrientation.South),
                    };
                default:
                    Debug.LogError($"Unknown edge orientation: {ec.Orientation}");
                    return System.Array.Empty<VertexCoord>();
            }
        }

        public static EdgeCoord[] GetEdgeNeighborEdgeCoords(EdgeCoord ec)
        {
            int q = ec.HexCoord.q;
            int r = ec.HexCoord.r;

            switch (ec.Orientation)
            {
                case EdgeOrientation.West:
                    return new[]
                    {
                        new EdgeCoord(q - 1, r, EdgeOrientation.NorthEast),
                        new EdgeCoord(q, r, EdgeOrientation.NorthWest),
                        new EdgeCoord(q, r - 1, EdgeOrientation.NorthEast),
                        new EdgeCoord(q, r - 1, EdgeOrientation.NorthWest),
                    };
                case EdgeOrientation.NorthWest:
                    return new[]
                    {
                        new EdgeCoord(q - 1, r, EdgeOrientation.NorthEast),
                        new EdgeCoord(q, r, EdgeOrientation.West),
                        new EdgeCoord(q, r + 1, EdgeOrientation.West),
                        new EdgeCoord(q, r, EdgeOrientation.NorthEast),
                    };
                case EdgeOrientation.NorthEast:
                    return new[]
                    {
                        new EdgeCoord(q, r, EdgeOrientation.NorthWest),
                        new EdgeCoord(q, r + 1, EdgeOrientation.West),
                        new EdgeCoord(q + 1, r, EdgeOrientation.NorthWest),
                        new EdgeCoord(q + 1, r, EdgeOrientation.West),
                    };
                default:
                    Debug.LogError($"Unknown edge orientation: {ec.Orientation}");
                    return System.Array.Empty<EdgeCoord>();
            }
        }
    }

    #endregion
}