using Grid;
using System.Collections;
using System.Collections.Generic;

public interface IGrid
{
    public IReadOnlyDictionary<HexCoord, HexTile> HexMap { get; }

    public IReadOnlyDictionary<VertexCoord, HexVertex> VertexMap { get; }

    public IReadOnlyDictionary<EdgeCoord, HexEdge> EdgeMap { get; }
}
