using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AIPlayer : IPlayer
{
    private const int MAX_ACTIONS_PER_TURN = 10;

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
        await PlaceRandomSettlementAsync(mustConnectToRoad: false);
        await PlaceRoadAsync();
        boardManager.EndPlayerTurn(this);
    }

    public async Task PlaceSecondSettlementAndRoadAsync()
    {
        boardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);
        await PlaceRandomSettlementAsync(mustConnectToRoad: false);
        await PlaceRoadAsync();
        boardManager.EndPlayerTurn(this);
    }

    public async Task PlayTurnAsync()
    {
        boardManager.BeginPlayerTurn(this, PlayerTurnType.RegularTurn);
        await Task.Delay(500); // Simulate thinking
        await boardManager.RollDice(this);
        await Task.Delay(500); // Simulate thinking

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
            var settlementLocations = GetAvailableSettlementLocations(mustConnectToRoad: true);
            var roadLocations = GetAvailableRoadLocations();
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
        }

        boardManager.EndPlayerTurn(this);
    }

    private List<HexVertex> GetAvailableSettlementLocations(bool mustConnectToRoad)
    {
        var locations = new List<HexVertex>();
        foreach (var vertex in boardManager.VertexMap.Values)
        {
            if (vertex.AvailableForBuilding(this, mustConnectToRoad))
            {
                locations.Add(vertex);
            }
        }
        return locations;
    }

    private List<HexEdge> GetAvailableRoadLocations()
    {
        var locations = new List<HexEdge>();
        foreach (var edge in boardManager.EdgeMap.Values)
        {
            if (edge.AvailableForBuilding(this))
            {
                locations.Add(edge);
            }
        }
        return locations;
    }

    private async Task PlaceRandomSettlementAsync(bool mustConnectToRoad)
    {
        // Find all selectable settlements
        var locations = GetAvailableSettlementLocations(mustConnectToRoad);
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
        await Task.Delay(500); // Simulate thinking
    }

    private async Task PlaceRoadAsync()
    {
        // Find all selectable roads
        var selectable = GetAvailableRoadLocations();
        if (selectable.Count > 0)
        {
            var choice = selectable[random.Next(selectable.Count)];
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
        await Task.Delay(500); // Simulate thinking
    }
}
