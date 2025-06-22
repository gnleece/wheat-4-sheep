using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AIPlayer : IPlayer
{
    private const int MAX_ACTIONS_PER_TURN = 10;
    private const int THINKING_DELAY_TIME_MS = 500;

    public int PlayerId => playerId;

    public Color PlayerColor => PlayerColorManager.GetPlayerColor(playerId);

    private int playerId;
    private IBoardManager boardManager;
    private System.Random random = new System.Random();

    public void Initialize(int playerId, IBoardManager boardManager)
    {
        this.playerId = playerId;
        this.boardManager = boardManager;
    }

    public async Task PlaceFirstSettlementAndRoadAsync()
    {
        boardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);

        await PlaceRandomSettlementAsync();
        await PlaceRandomRoadAsync();

        boardManager.EndPlayerTurn(this);
    }

    public async Task PlaceSecondSettlementAndRoadAsync()
    {
        boardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);

        await PlaceRandomSettlementAsync();
        await PlaceRandomRoadAsync();

        boardManager.EndPlayerTurn(this);
    }

    public async Task PlayTurnAsync()
    {
        boardManager.BeginPlayerTurn(this, PlayerTurnType.RegularTurn);

        await Task.Delay(THINKING_DELAY_TIME_MS);
        await boardManager.RollDice(this);

        await Task.Delay(THINKING_DELAY_TIME_MS);

        var actionCount = 0;
        while (actionCount < MAX_ACTIONS_PER_TURN)  // Prevent infinite loops
        {
            // Check what I can afford to build
            var resources = boardManager.GetResourceHandForPlayer(this);
            var canAffordSettlement = ResourceHand.HasEnoughResources(resources, BuildingCosts.SettlementCost);
            var canAffordRoad = ResourceHand.HasEnoughResources(resources, BuildingCosts.RoadCost);

            if (!canAffordSettlement && !canAffordRoad)
            {
                // If I can't afford anything, end my turn
                Debug.Log("AI Player cannot afford any actions, ending turn.");
                break;
            }

            // Try to build something (priority: settlement > road)
            var settlementLocations = boardManager.GetAvailableSettlementLocations(this);
            var roadLocations = boardManager.GetAvailableRoadLocations(this);
            if (settlementLocations.Count > 0 && canAffordSettlement)
            {
                var success = boardManager.BuildSettlement(this, settlementLocations[0]);
                if (!success)
                {
                    Debug.LogWarning($"AI Player {playerId} failed to place settlement at {settlementLocations[0]}. This should not happen if the game is set up correctly.");
                    break;
                }
            }
            else if (roadLocations.Count > 0 && canAffordRoad)
            {
                var success = boardManager.BuildRoad(this, roadLocations[0]);
                if (!success)
                {
                    Debug.LogWarning($"AI Player {playerId} failed to place road at {roadLocations[0]}. This should not happen if the game is set up correctly.");
                    break;
                }
            }

            actionCount++;
            await Task.Delay(THINKING_DELAY_TIME_MS);
        }

        boardManager.EndPlayerTurn(this);
    }

    private async Task PlaceRandomSettlementAsync()
    {
        var locations = boardManager.GetAvailableSettlementLocations(this);

        if (locations.Count > 0)
        {
            var choice = locations[random.Next(locations.Count)];
            var success = boardManager.BuildSettlement(this, choice);
            if (!success)
            {
                Debug.LogWarning($"AI Player {playerId} failed to place settlement at {choice}. This should not happen if the game is set up correctly.");
            }
        }
        else
        {
            Debug.LogWarning($"AI Player {playerId} could not find a valid settlement location. This should not happen if the game is set up correctly.");
        }

        await Task.Delay(THINKING_DELAY_TIME_MS);
    }

    private async Task PlaceRandomRoadAsync()
    {
        var locations = boardManager.GetAvailableRoadLocations(this);

        if (locations.Count > 0)
        {
            var choice = locations[random.Next(locations.Count)];
            var success = boardManager.BuildRoad(this, choice);
            if (!success)
            {
                Debug.LogWarning($"AI Player {playerId} failed to place road at {choice}. This should not happen if the game is set up correctly.");
            }
        }
        else
        {
            Debug.LogWarning($"AI Player {playerId} could not find a valid road location. This should not happen if the game is set up correctly.");
        }

        await Task.Delay(THINKING_DELAY_TIME_MS);
    }
}
