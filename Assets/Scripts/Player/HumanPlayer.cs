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

        while (boardManager.IsPlayerTurn(this))
        {
            await Task.Yield();
        }
    }

    public async Task DiscardOnSevenRoll(ResourceHand hand, int cardsToDiscard)
    {
        Debug.Log($"Human Player {playerId} must discard {cardsToDiscard} cards...");
        
        // Use BoardManager to show discard UI
        await boardManager.GetManualDiscardOnSevenRoll(this, hand, cardsToDiscard);
    }

    public async Task MoveRobber()
    {
        var chosenRobberLocation = await boardManager.GetManualSelectionForRobberLocation(this);
        boardManager.MoveRobber(this, chosenRobberLocation);
    }

    public async Task<IPlayer> ChoosePlayerToStealFrom(List<IPlayer> availablePlayers)
    {
        if (availablePlayers == null || availablePlayers.Count == 0)
        {
            Debug.Log($"Human Player {playerId} has no players to steal from");
            return null;
        }

        // Use BoardManager to show player selection UI
        return await boardManager.GetManualSelectionForPlayerToStealFrom(this, availablePlayers);
    }

    public async Task<bool> ConsiderTradeOffer(TradeOffer offer)
    {
        return await boardManager.GetManualTradeOfferResponse(this, offer);
    }
}