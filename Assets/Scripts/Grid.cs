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

    }

    #endregion
}