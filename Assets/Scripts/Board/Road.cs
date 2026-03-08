using Grid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road
{
    public HexEdge HexEdge { get; private set; }

    public IPlayer Owner { get; private set; }

    public Road(HexEdge hexEdge, IPlayer owner)
    {
        HexEdge = hexEdge;
        Owner = owner;
    }
}
