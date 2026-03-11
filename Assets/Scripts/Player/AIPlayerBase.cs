using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grid;
using UnityEngine;

public abstract class AIPlayerBase : IPlayer
{
    private const int MAX_ACTIONS_PER_TURN = 10;
    private const int THINKING_DELAY_TIME_MS = 1000;

    public int PlayerId => _playerId;

    public Color PlayerColor => PlayerColorManager.GetPlayerColor(_playerId);

    public bool IsHuman => false;

    protected IBoardManager BoardManager;
    protected readonly IRandomProvider Random;
    
    private int _playerId;
    
    public AIPlayerBase(IRandomProvider random)
    {
        Random = random;
    }

    public void Initialize(int playerId, IBoardManager boardManager)
    {
        _playerId = playerId;
        BoardManager = boardManager;
    }

    #region Abstract methods

    protected abstract VertexCoord ChooseFirstSettlementLocation();
    protected abstract EdgeCoord ChooseFirstRoadLocation();

    protected abstract VertexCoord ChooseSecondSettlementLocation();
    protected abstract EdgeCoord ChooseSecondRoadLocation();

    protected abstract HexCoord ChooseRobberLocation(List<HexCoord> availableLocations);
    protected abstract List<ResourceType> ChooseResourcesToDiscard(ResourceHand hand, int cardsToDiscard);
    protected abstract IPlayer SelectPlayerToStealFrom(List<IPlayer> availablePlayers);
    protected abstract bool EvaluateTradeOffer(TradeOffer offer);

    #endregion
    
    public async Task PlaceFirstSettlementAndRoadAsync()
    {
        BoardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);

        var settlementLocation = ChooseFirstSettlementLocation();
        var buildSettlementSuccess = BoardManager.BuildSettlement(this, settlementLocation);
        if (!buildSettlementSuccess)
        {
            Debug.LogWarning($"AI Player {_playerId} failed to place first settlement at {settlementLocation}.");
        }
        await Task.Delay(THINKING_DELAY_TIME_MS);

        var roadLocation = ChooseFirstRoadLocation();
        var buildRoadSuccess = BoardManager.BuildRoad(this, roadLocation);
        if (!buildRoadSuccess)
        {
            Debug.LogWarning($"AI Player {_playerId} failed to place first road at {roadLocation}.");
        }
        await Task.Delay(THINKING_DELAY_TIME_MS);

        BoardManager.EndPlayerTurn(this);
    }

    public async Task PlaceSecondSettlementAndRoadAsync()
    {
        BoardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);

        var settlementLocation = ChooseSecondSettlementLocation();
        var buildSettlementSuccess = BoardManager.BuildSettlement(this, settlementLocation);
        if (!buildSettlementSuccess)
        {
            Debug.LogWarning($"AI Player {_playerId} failed to place second settlement at {settlementLocation}.");
        }
        await Task.Delay(THINKING_DELAY_TIME_MS);

        var roadLocation = ChooseSecondRoadLocation();
        var buildRoadSuccess = BoardManager.BuildRoad(this, roadLocation);
        if (!buildRoadSuccess)
        {
            Debug.LogWarning($"AI Player {_playerId} failed to place second road at {roadLocation}.");
        }
        await Task.Delay(THINKING_DELAY_TIME_MS);

        BoardManager.EndPlayerTurn(this);
    }

    public async Task PlayTurnAsync()
    {
        BoardManager.BeginPlayerTurn(this, PlayerTurnType.RegularTurn);

        await Task.Delay(THINKING_DELAY_TIME_MS);

        // Play a Knight card before rolling if one is available
        if (BoardManager.CanPlayAnyDevCard(this))
        {
            var hand = BoardManager.GetDevCardHandForPlayer(this);
            if (hand.TryGetValue(DevelopmentCardType.Knight, out int knightCount) && knightCount > 0)
            {
                await BoardManager.PlayDevelopmentCard(this, DevelopmentCardType.Knight);
                await Task.Delay(THINKING_DELAY_TIME_MS);
            }
        }

        await BoardManager.RollDice(this);

        await Task.Delay(THINKING_DELAY_TIME_MS);

        var actionCount = 0;
        while (actionCount < MAX_ACTIONS_PER_TURN)  // Prevent infinite loops
        {
            // Check what I can afford to build
            var resources = BoardManager.GetResourceHandForPlayer(this);
            var canAffordSettlement = ResourceHand.HasEnoughResources(resources, BuildingCosts.SettlementCost);
            var canAffordRoad = ResourceHand.HasEnoughResources(resources, BuildingCosts.RoadCost);
            var canAffordDevCard = BoardManager.CanBuyDevelopmentCard(this);

            if (!canAffordSettlement && !canAffordRoad && !canAffordDevCard)
            {
                // If I can't afford anything, end my turn
                Debug.Log("AI Player cannot afford any actions, ending turn.");
                break;
            }

            // Try to build something (priority: settlement > road > dev card)
            var settlementLocations = BoardManager.GetAvailableSettlementLocations(this);
            var roadLocations = BoardManager.GetAvailableRoadLocations(this);
            if (settlementLocations.Count > 0 && canAffordSettlement)
            {
                var success = BoardManager.BuildSettlement(this, settlementLocations[0]);
                if (!success)
                {
                    Debug.LogWarning($"AI Player {_playerId} failed to place settlement at {settlementLocations[0]}. This should not happen if the game is set up correctly.");
                    break;
                }
            }
            else if (roadLocations.Count > 0 && canAffordRoad)
            {
                var success = BoardManager.BuildRoad(this, roadLocations[0]);
                if (!success)
                {
                    Debug.LogWarning($"AI Player {_playerId} failed to place road at {roadLocations[0]}. This should not happen if the game is set up correctly.");
                    break;
                }
            }
            else if (canAffordDevCard)
            {
                BoardManager.BuyDevelopmentCard(this);
            }

            actionCount++;
            await Task.Delay(THINKING_DELAY_TIME_MS);
        }

        BoardManager.EndPlayerTurn(this);
    }

    public async Task DiscardOnSevenRoll(ResourceHand hand, int cardsToDiscard)
    {
        Debug.Log($"AI Player {_playerId} discarding {cardsToDiscard} cards...");

        var resourcesToDiscard = ChooseResourcesToDiscard(hand, cardsToDiscard);
        foreach (var resource in resourcesToDiscard)
        {
            hand.Remove(resource, 1);
            Debug.Log($"AI Player {_playerId} discarded 1 {resource}");
        }

        await Task.Delay(500); // Small delay for visual feedback
    }

    public async Task MoveRobber()
    {
        var locations = BoardManager.GetAvailableRobberLocations(this);

        if (locations.Count > 0)
        {
            var choice = ChooseRobberLocation(locations);
            var success = BoardManager.MoveRobber(this, choice);
            if (!success)
            {
                Debug.LogWarning($"AI Player {_playerId} failed to place robber at {choice}. This should not happen if the game is set up correctly.");
            }
        }
        else
        {
            Debug.LogWarning($"AI Player {_playerId} could not find a valid robber location. This should not happen if the game is set up correctly.");
        }

        await Task.Delay(THINKING_DELAY_TIME_MS);
    }

    public async Task<IPlayer> ChoosePlayerToStealFrom(List<IPlayer> availablePlayers)
    {
        if (availablePlayers == null || availablePlayers.Count == 0)
        {
            Debug.Log($"AI Player {_playerId} has no players to steal from");
            return null;
        }

        var choice = SelectPlayerToStealFrom(availablePlayers);
        Debug.Log($"AI Player {_playerId} chose to steal from Player {choice.PlayerId}");

        await Task.Delay(THINKING_DELAY_TIME_MS);
        return choice;
    }

    public Task<bool> ConsiderTradeOffer(TradeOffer offer)
    {
        var accepted = EvaluateTradeOffer(offer);
        Debug.Log($"AI Player {_playerId} {(accepted ? "accepted" : "declined")} trade offer from Player {offer.Initiator.PlayerId}");
        return Task.FromResult(accepted);
    }
}
