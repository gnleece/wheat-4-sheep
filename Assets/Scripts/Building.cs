using Grid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building
{
    public enum BuildingType
    {
        Settlement,
        City
    }

    public BuildingType Type { get; private set; }

    public HexVertex HexVertex { get; private set; }

    public IPlayer Owner { get; private set; }

    public int VictoryPoints => Type == BuildingType.City ? 2 : 1;

    public Building(BuildingType type, HexVertex hexVertex, IPlayer owner)
    {
        Type = type;
        HexVertex = hexVertex;
        Owner = owner;
    }

    public bool Upgrade()
    {
        if (Type != BuildingType.Settlement)
        {
            return false;
        }

        Type = BuildingType.City;
        return true;
    }
}
