using System.Collections.Generic;
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

    protected override HexCoord ChooseRobberLocation(List<HexCoord> availableLocations)
    {
        return availableLocations[Random.Next(availableLocations.Count)];
    }

    protected override List<ResourceType> ChooseResourcesToDiscard(ResourceHand hand, int cardsToDiscard)
    {
        var allResources = hand.GetAll();
        var resourcesList = new List<ResourceType>();

        foreach (var kvp in allResources)
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                resourcesList.Add(kvp.Key);
            }
        }

        var discarded = new List<ResourceType>();
        for (int i = 0; i < cardsToDiscard && resourcesList.Count > 0; i++)
        {
            var randomIndex = Random.Next(resourcesList.Count);
            discarded.Add(resourcesList[randomIndex]);
            resourcesList.RemoveAt(randomIndex);
        }

        return discarded;
    }

    protected override IPlayer SelectPlayerToStealFrom(List<IPlayer> availablePlayers)
    {
        return availablePlayers[Random.Next(availablePlayers.Count)];
    }

    protected override bool EvaluateTradeOffer(TradeOffer offer)
    {
        return false;
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
