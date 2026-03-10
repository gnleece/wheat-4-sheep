using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AIPlayer : IPlayer
{
    private const int MAX_ACTIONS_PER_TURN = 10;
    private const int THINKING_DELAY_TIME_MS = 100;

    public int PlayerId => _playerId;

    public Color PlayerColor => PlayerColorManager.GetPlayerColor(_playerId);

    public bool IsHuman => false;

    private IBoardManager _boardManager;
    private readonly IRandomProvider _random;
    
    private int _playerId;
    
    public AIPlayer(IRandomProvider random)
    {
        _random = random;
    }

    public void Initialize(int playerId, IBoardManager boardManager)
    {
        _playerId = playerId;
        _boardManager = boardManager;
    }

    public async Task PlaceFirstSettlementAndRoadAsync()
    {
        _boardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);

        await PlaceRandomSettlementAsync();
        await PlaceRandomRoadAsync();

        _boardManager.EndPlayerTurn(this);
    }

    public async Task PlaceSecondSettlementAndRoadAsync()
    {
        _boardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);

        await PlaceRandomSettlementAsync();
        await PlaceRandomRoadAsync();

        _boardManager.EndPlayerTurn(this);
    }

    public async Task PlayTurnAsync()
    {
        _boardManager.BeginPlayerTurn(this, PlayerTurnType.RegularTurn);

        await Task.Delay(THINKING_DELAY_TIME_MS);

        // Play a Knight card before rolling if one is available
        if (_boardManager.CanPlayAnyDevCard(this))
        {
            var hand = _boardManager.GetDevCardHandForPlayer(this);
            if (hand.TryGetValue(DevelopmentCardType.Knight, out int knightCount) && knightCount > 0)
            {
                await _boardManager.PlayDevelopmentCard(this, DevelopmentCardType.Knight);
                await Task.Delay(THINKING_DELAY_TIME_MS);
            }
        }

        await _boardManager.RollDice(this);

        await Task.Delay(THINKING_DELAY_TIME_MS);

        var actionCount = 0;
        while (actionCount < MAX_ACTIONS_PER_TURN)  // Prevent infinite loops
        {
            // Check what I can afford to build
            var resources = _boardManager.GetResourceHandForPlayer(this);
            var canAffordSettlement = ResourceHand.HasEnoughResources(resources, BuildingCosts.SettlementCost);
            var canAffordRoad = ResourceHand.HasEnoughResources(resources, BuildingCosts.RoadCost);
            var canAffordDevCard = _boardManager.CanBuyDevelopmentCard(this);

            if (!canAffordSettlement && !canAffordRoad && !canAffordDevCard)
            {
                // If I can't afford anything, end my turn
                Debug.Log("AI Player cannot afford any actions, ending turn.");
                break;
            }

            // Try to build something (priority: settlement > road > dev card)
            var settlementLocations = _boardManager.GetAvailableSettlementLocations(this);
            var roadLocations = _boardManager.GetAvailableRoadLocations(this);
            if (settlementLocations.Count > 0 && canAffordSettlement)
            {
                var success = _boardManager.BuildSettlement(this, settlementLocations[0]);
                if (!success)
                {
                    Debug.LogWarning($"AI Player {_playerId} failed to place settlement at {settlementLocations[0]}. This should not happen if the game is set up correctly.");
                    break;
                }
            }
            else if (roadLocations.Count > 0 && canAffordRoad)
            {
                var success = _boardManager.BuildRoad(this, roadLocations[0]);
                if (!success)
                {
                    Debug.LogWarning($"AI Player {_playerId} failed to place road at {roadLocations[0]}. This should not happen if the game is set up correctly.");
                    break;
                }
            }
            else if (canAffordDevCard)
            {
                _boardManager.BuyDevelopmentCard(this);
            }

            actionCount++;
            await Task.Delay(THINKING_DELAY_TIME_MS);
        }

        _boardManager.EndPlayerTurn(this);
    }

    public async Task DiscardOnSevenRoll(ResourceHand hand, int cardsToDiscard)
    {
        Debug.Log($"AI Player {_playerId} discarding {cardsToDiscard} cards randomly...");
        
        var allResources = hand.GetAll();
        var resourcesList = new List<ResourceType>();
        
        // Create a list of all resources (including duplicates)
        foreach (var kvp in allResources)
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                resourcesList.Add(kvp.Key);
            }
        }
        
        // Randomly select cards to discard
        for (int i = 0; i < cardsToDiscard && resourcesList.Count > 0; i++)
        {
            var randomIndex = _random.Next(resourcesList.Count);
            var resourceToDiscard = resourcesList[randomIndex];
            hand.Remove(resourceToDiscard, 1);
            resourcesList.RemoveAt(randomIndex);
            Debug.Log($"AI Player {_playerId} discarded 1 {resourceToDiscard}");
        }
        
        await Task.Delay(500); // Small delay for visual feedback
    }

    public async Task MoveRobber()
    {
        var locations = _boardManager.GetAvailableRobberLocations(this);

        if (locations.Count > 0)
        {
            var choice = locations[_random.Next(locations.Count)];
            var success = _boardManager.MoveRobber(this, choice);
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

        // Choose a random player to steal from
        var choice = availablePlayers[_random.Next(availablePlayers.Count)];
        Debug.Log($"AI Player {_playerId} chose to steal from Player {choice.PlayerId}");
        
        await Task.Delay(THINKING_DELAY_TIME_MS);
        return choice;
    }

    public Task<bool> ConsiderTradeOffer(TradeOffer offer)
    {
        Debug.Log($"AI Player {_playerId} declined trade offer from Player {offer.Initiator.PlayerId}");
        return Task.FromResult(false);
    }

    private async Task PlaceRandomSettlementAsync()
    {
        var locations = _boardManager.GetAvailableSettlementLocations(this);

        if (locations.Count > 0)
        {
            var choice = locations[_random.Next(locations.Count)];
            var success = _boardManager.BuildSettlement(this, choice);
            if (!success)
            {
                Debug.LogWarning($"AI Player {_playerId} failed to place settlement at {choice}. This should not happen if the game is set up correctly.");
            }
        }
        else
        {
            Debug.LogWarning($"AI Player {_playerId} could not find a valid settlement location. This should not happen if the game is set up correctly.");
        }

        await Task.Delay(THINKING_DELAY_TIME_MS);
    }

    private async Task PlaceRandomRoadAsync()
    {
        var locations = _boardManager.GetAvailableRoadLocations(this);

        if (locations.Count > 0)
        {
            var choice = locations[_random.Next(locations.Count)];
            var success = _boardManager.BuildRoad(this, choice);
            if (!success)
            {
                Debug.LogWarning($"AI Player {_playerId} failed to place road at {choice}. This should not happen if the game is set up correctly.");
            }
        }
        else
        {
            Debug.LogWarning($"AI Player {_playerId} could not find a valid road location. This should not happen if the game is set up correctly.");
        }

        await Task.Delay(THINKING_DELAY_TIME_MS);
    }
}
