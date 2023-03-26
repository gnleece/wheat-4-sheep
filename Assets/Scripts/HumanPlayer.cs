using Grid;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HumanPlayer : IPlayer
{
    private int playerId;
    private IBoard board;
    
    public int PlayerId => playerId;

    public void Initialize(int playerId, IBoard board)
    {
        this.playerId = playerId;
        this.board = board;
    }

    public async Task<(VertexCoord, EdgeCoord)> SelectFirstSettlementAndRoadPositions()
    {
        await Task.Delay(1000);

        var vertexCoord = new VertexCoord();
        var edgeCoord = new EdgeCoord();

        return (vertexCoord, edgeCoord);
    }
}