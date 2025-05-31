using Grid;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HumanPlayer : IPlayer
{
    public int PlayerId => playerId;

    public Color PlayerColor => PlayerColorManager.GetPlayerColor(playerId);

    private int playerId;
    private IBoardManager boardManager;

    public void Initialize(int playerId, IBoardManager boardManager)
    {
        this.playerId = playerId;
        this.boardManager = boardManager;
    }

    public async Task PlaceFirstSettlementAndRoadAsync()
    {
        await boardManager.ClaimBoardForPlayerActionAsync(this, BoardMode.BuildSettlement);

        await boardManager.ClaimBoardForPlayerActionAsync(this, BoardMode.BuildRoad);
    }

    public async Task PlaceSecondSettlementAndRoadAsync()
    {
        await boardManager.ClaimBoardForPlayerActionAsync(this, BoardMode.BuildSettlement);

        await boardManager.ClaimBoardForPlayerActionAsync(this, BoardMode.BuildRoad);
    }
}