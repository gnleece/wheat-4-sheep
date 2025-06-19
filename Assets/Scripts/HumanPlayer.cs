using Grid;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static GameManager;

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

    public async Task PlayTurnAsync()
    {
        boardManager.BeginPlayerTurn(this);

        while (true)
        {
            // TODO: replace this with in-game UI
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                // Roll Dice
                var diceRoll = await boardManager.RollDice(this);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // Build Settlement
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                // Build Road
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                // End Turn
                var endTurnSuccess = boardManager.EndPlayerTurn(this);
                if (endTurnSuccess)
                {
                    break;
                }
            }

            await Task.Yield();
        }

        
    }
}