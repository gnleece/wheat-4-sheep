using Grid;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HumanPlayer : IPlayer
{
    public int PlayerId => playerId;

    private int playerId;
    private IBoard board;

    public void Initialize(int playerId, IBoard board)
    {
        this.playerId = playerId;
        this.board = board;
    }

    public async Task PlaceFirstSettlementAndRoadAsync()
    {
        await board.ClaimBoardForPlayerActionAsync(this, BoardMode.BuildSettlement);

        //await board.ClaimBoardForPlayerActionAsync(this, BoardMode.BuildRoad);
    }

    public async Task PlaceSecondSettlementAndRoadAsync()
    {
        await board.ClaimBoardForPlayerActionAsync(this, BoardMode.BuildSettlement);

        //await board.ClaimBoardForPlayerActionAsync(this, BoardMode.BuildRoad);
    }
}