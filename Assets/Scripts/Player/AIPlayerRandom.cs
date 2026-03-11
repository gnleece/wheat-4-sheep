using Grid;
using UnityEngine;

public class AIPlayerRandom : AIPlayerBase
{
    public AIPlayerRandom(IRandomProvider random) : base(random)
    {
    }

    protected override VertexCoord ChooseFirstSettlementLocation()
    {
        return GetRandomSettlementLocation();
    }

    protected override EdgeCoord ChooseFirstRoadLocation()
    {
        return GetRandomRoadLocation();
    }

    protected override VertexCoord ChooseSecondSettlementLocation()
    {
        return GetRandomSettlementLocation();
    }
    
    protected override EdgeCoord ChooseSecondRoadLocation()
    {
        return GetRandomRoadLocation();
    }

    private VertexCoord GetRandomSettlementLocation()
    {
        var locations = BoardManager.GetAvailableSettlementLocations(this);
        if (locations.Count > 0)
        {
            return locations[Random.Next(locations.Count)];
        } 
        
        Debug.LogWarning($"AI Player {PlayerId} could not find any valid settlement locations.");
        return default;
    }

    private EdgeCoord GetRandomRoadLocation()
    {
        var locations = BoardManager.GetAvailableRoadLocations(this);
        if (locations.Count > 0)
        {
            return locations[Random.Next(locations.Count)];
        } 
        Debug.LogWarning($"AI Player {PlayerId} could not find any valid road locations.");
        return default;
    }
}
