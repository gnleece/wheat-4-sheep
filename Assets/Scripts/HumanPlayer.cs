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
        boardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);
        await PlaceInitialSettlementAndRoad();
        boardManager.EndPlayerTurn(this);
    }

    public async Task PlaceSecondSettlementAndRoadAsync()
    {
        boardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);
        await PlaceInitialSettlementAndRoad();
        boardManager.EndPlayerTurn(this);
    }

    private async Task PlaceInitialSettlementAndRoad()
    {
        var settlementPlaced = false;
        while (!settlementPlaced)
        {
            var chosenSettlmentLocation = await boardManager.GetManualSelectionForSettlementLocation(this);
            settlementPlaced = boardManager.BuildSettlement(this, chosenSettlmentLocation);
        }

        var roadPlaced = false;
        while (!roadPlaced)
        {
            var chosenRoadLocation = await boardManager.GetManualSelectionForRoadLocation(this);
            roadPlaced = boardManager.BuildRoad(this, chosenRoadLocation);
        }
    }

    public async Task PlayTurnAsync()
    {
        boardManager.BeginPlayerTurn(this, PlayerTurnType.RegularTurn);

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
                var chosenSettlementLocation = await boardManager.GetManualSelectionForSettlementLocation(this);
                boardManager.BuildSettlement(this, chosenSettlementLocation);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                // Build Road
                var chosenRoadLocation = await boardManager.GetManualSelectionForRoadLocation(this);
                boardManager.BuildRoad(this, chosenRoadLocation);
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