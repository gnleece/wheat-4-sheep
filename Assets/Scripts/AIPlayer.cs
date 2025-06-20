using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AIPlayer : IPlayer
{
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
        await PlaceSettlementAsync();
        await PlaceRoadAsync();
        boardManager.EndPlayerTurn(this);
    }

    public async Task PlaceSecondSettlementAndRoadAsync()
    {
        boardManager.BeginPlayerTurn(this, PlayerTurnType.InitialPlacement);
        await PlaceSettlementAsync();
        await PlaceRoadAsync();
        boardManager.EndPlayerTurn(this);
    }

    public async Task PlayTurnAsync()
    {
        boardManager.BeginPlayerTurn(this, PlayerTurnType.RegularTurn);
        await Task.Delay(500); // Simulate thinking
        await boardManager.RollDice(this);
        await Task.Delay(500); // Simulate thinking
        boardManager.EndPlayerTurn(this);
    }

    private async Task PlaceSettlementAsync()
    {
        // Find all selectable settlements
        var selectable = new List<HexVertex>();
        foreach (var vertex in boardManager.VertexMap.Values)
        {
            if (vertex.AvailableForBuilding(this, false))
            {
                selectable.Add(vertex);
            }
        }
        if (selectable.Count > 0)
        {
            var choice = selectable[random.Next(selectable.Count)];
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
        var selectable = new List<HexEdge>();
        foreach (var edge in boardManager.EdgeMap.Values)
        {
            if (edge.AvailableForBuilding(this))
            {
                selectable.Add(edge);
            }
        }
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
