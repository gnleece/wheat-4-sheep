using Grid;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static GameManager;

public class HumanPlayer : IPlayer
{
    public int PlayerId => _playerId;

    public Color PlayerColor => PlayerColorManager.GetPlayerColor(_playerId);

    private IBoardManager _boardManager;
    
    private int _playerId;
    
    public void Initialize(int playerId, IBoardManager boardManager)
    {
        _playerId = playerId;
        _boardManager = boardManager;
    }

    public async Task PlaceFirstSettlementAndRoadAsync()
    {
        _boardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);
        await PlaceInitialSettlementAndRoad();
        _boardManager.EndPlayerTurn(this);
    }

    public async Task PlaceSecondSettlementAndRoadAsync()
    {
        _boardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);
        await PlaceInitialSettlementAndRoad();
        _boardManager.EndPlayerTurn(this);
    }

    private async Task PlaceInitialSettlementAndRoad()
    {
        var settlementPlaced = false;
        while (!settlementPlaced)
        {
            var chosenSettlmentLocation = await _boardManager.GetManualSelectionForSettlementLocation(this);
            settlementPlaced = _boardManager.BuildSettlement(this, chosenSettlmentLocation);
        }

        var roadPlaced = false;
        while (!roadPlaced)
        {
            var chosenRoadLocation = await _boardManager.GetManualSelectionForRoadLocation(this);
            roadPlaced = _boardManager.BuildRoad(this, chosenRoadLocation);
        }
    }

    public async Task PlayTurnAsync()
    {
        _boardManager.BeginPlayerTurn(this, PlayerTurnType.RegularTurn);

        while (_boardManager.IsPlayerTurn(this))
        {
            await Task.Yield();
        }
    }

    public async Task DiscardOnSevenRoll(ResourceHand hand, int cardsToDiscard)
    {
        Debug.Log($"Human Player {_playerId} must discard {cardsToDiscard} cards...");
        
        // Use BoardManager to show discard UI
        await _boardManager.GetManualDiscardOnSevenRoll(this, hand, cardsToDiscard);
    }

    public async Task MoveRobber()
    {
        var chosenRobberLocation = await _boardManager.GetManualSelectionForRobberLocation(this);
        _boardManager.MoveRobber(this, chosenRobberLocation);
    }

    public async Task<IPlayer> ChoosePlayerToStealFrom(List<IPlayer> availablePlayers)
    {
        if (availablePlayers == null || availablePlayers.Count == 0)
        {
            Debug.Log($"Human Player {_playerId} has no players to steal from");
            return null;
        }

        // Use BoardManager to show player selection UI
        return await _boardManager.GetManualSelectionForPlayerToStealFrom(this, availablePlayers);
    }

    public async Task<bool> ConsiderTradeOffer(TradeOffer offer)
    {
        return await _boardManager.GetManualTradeOfferResponse(this, offer);
    }
}