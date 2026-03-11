using System.Collections.Generic;
using System.Linq;
using Grid;
using UnityEngine;

public class AIPlayerSimple : AIPlayerBase
{
    public AIPlayerSimple(IRandomProvider random) : base(random)
    {
    }
    
    protected override VertexCoord ChooseFirstSettlementLocation()
    {
        // TODO: Consider turn order - if I'm the last player to place an initial settlement, I'll
        //  get to place my second one right away, which should affect my choice
        
        var allAvailableLocations = BoardManager.GetAvailableSettlementLocations(this);
        var rankedLocations = allAvailableLocations.OrderByDescending(GetVertexScore);
        return rankedLocations.First();
    }

    protected override EdgeCoord ChooseFirstRoadLocation()
    {
        throw new System.NotImplementedException();
    }

    protected override VertexCoord ChooseSecondSettlementLocation()
    {
        throw new System.NotImplementedException();
    }

    protected override EdgeCoord ChooseSecondRoadLocation()
    {
        throw new System.NotImplementedException();
    }
    
    // Numbers that are more common are better (assumes 2D6 dice)
    private readonly Dictionary<int, int> _tileDiceNumberScores = new()
    {
        { 0, 0 },
        { 1, 0 },
        { 2, 1 },
        { 3, 2 },
        { 4, 3 },
        { 5, 4 },
        { 6, 5 },
        { 7, 0 },
        { 8, 5 },
        { 9, 4 },
        { 10, 3 },
        { 11, 2 },
        { 12, 1 }
    };
    
    private int GetVertexScore(VertexCoord vertexCoord)
    {
        if (!BoardManager.VertexMap.TryGetValue(vertexCoord, out var vertex))
        {
            return 0;
        }
        
        var score = 0;
        var neighborTiles = vertex.NeighborHexTiles;
        foreach (var tile in neighborTiles)
        {
            var diceNumber = tile.DiceNumber ?? 0;
            score += _tileDiceNumberScores.GetValueOrDefault(diceNumber, 0);
            
            int numDisctinctTileTypes = neighborTiles.Select(x => x.ResourceType).Distinct().Count();
            score += numDisctinctTileTypes * 2;
        }

        return score;
    }
}
