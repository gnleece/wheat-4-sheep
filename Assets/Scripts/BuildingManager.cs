using Grid;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager
{
    private readonly IReadOnlyDictionary<VertexCoord, HexVertex> vertexMap;
    private readonly IReadOnlyDictionary<EdgeCoord, HexEdge> edgeMap;
    private readonly IReadOnlyDictionary<HexCoord, HexTile> hexTileMap;
    private readonly TurnManager turnManager;
    private readonly ResourceManager resourceManager;
    private readonly IGameManager gameManager;

    private HexTile currentRobberHexTile = null;
    private RobberObject robberObject = null;
    private readonly Dictionary<IPlayer, HexVertex> lastSettlementPlaced = new Dictionary<IPlayer, HexVertex>();

    public HexTile CurrentRobberHexTile => currentRobberHexTile;

    public BuildingManager(
        IReadOnlyDictionary<VertexCoord, HexVertex> vertexMap,
        IReadOnlyDictionary<EdgeCoord, HexEdge> edgeMap,
        IReadOnlyDictionary<HexCoord, HexTile> hexTileMap,
        TurnManager turnManager,
        ResourceManager resourceManager,
        IGameManager gameManager)
    {
        this.vertexMap = vertexMap;
        this.edgeMap = edgeMap;
        this.hexTileMap = hexTileMap;
        this.turnManager = turnManager;
        this.resourceManager = resourceManager;
        this.gameManager = gameManager;
    }

    public void InitializeRobber(RobberObject robberObject, HexTile initialTile)
    {
        this.robberObject = robberObject;
        currentRobberHexTile = initialTile;
    }

    private HexVertex GetLastSettlementPlaced(IPlayer player)
    {
        lastSettlementPlaced.TryGetValue(player, out var hexVertex);
        return hexVertex;
    }

    public bool BuildSettlement(IPlayer player, HexVertex hexVertex)
    {
        if (!turnManager.IsPlayerTurn(player))
        {
            Debug.LogError($"Board manager BuildSettlement: turn is not active for {player}");
            return false;
        }

        if (turnManager.CurrentTurnType == PlayerTurnType.RegularTurn && !turnManager.HasRolledDice)
        {
            Debug.LogError($"Player {player.PlayerId} must roll dice before building settlements");
            return false;
        }

        if (hexVertex == null)
        {
            Debug.LogError("Board manager BuildSettlement: hex vertex is null");
            return false;
        }

        var isBuildingFree = gameManager.CurrentGameState == GameManager.GameState.FirstSettlementPlacement ||
                             gameManager.CurrentGameState == GameManager.GameState.SecondSettlementPlacement;

        if (!isBuildingFree && !resourceManager.HasEnoughResources(player, BuildingCosts.SettlementCost))
        {
            Debug.LogError($"Player {player.PlayerId} does not have enough resources to build a settlement");
            return false;
        }

        var success = hexVertex.TryPlaceBuilding(Building.BuildingType.Settlement, player);
        if (success)
        {
            lastSettlementPlaced[player] = hexVertex;

            if (!isBuildingFree)
            {
                resourceManager.DeductResources(player, BuildingCosts.SettlementCost);
            }

            if (gameManager.CurrentGameState == GameManager.GameState.SecondSettlementPlacement)
            {
                resourceManager.GivePlayerResourcesForNeighborHexTiles(player, hexVertex);
            }

            Debug.Log($"PLACED SETTLEMENT: {hexVertex}");
        }
        else
        {
            Debug.Log("Invalid settlement position. Try again.");
        }

        return success;
    }

    public bool BuildRoad(IPlayer player, HexEdge hexEdge)
    {
        if (!turnManager.IsPlayerTurn(player))
        {
            Debug.LogError($"Board manager BuildRoad: turn is not active for {player}");
            return false;
        }

        if (turnManager.CurrentTurnType == PlayerTurnType.RegularTurn && !turnManager.HasRolledDice)
        {
            Debug.LogError($"Player {player.PlayerId} must roll dice before building roads");
            return false;
        }

        if (hexEdge == null)
        {
            Debug.LogError("Board manager BuildRoad: hex edge is null");
            return false;
        }

        var isBuildingFree = gameManager.CurrentGameState == GameManager.GameState.FirstSettlementPlacement ||
                             gameManager.CurrentGameState == GameManager.GameState.SecondSettlementPlacement;

        if (!isBuildingFree && !resourceManager.HasEnoughResources(player, BuildingCosts.RoadCost))
        {
            Debug.LogError($"Player {player.PlayerId} does not have enough resources to build a road");
            return false;
        }

        Debug.Log($"Trying to place road at {hexEdge}");

        var success = hexEdge.TryPlaceRoad(player);
        if (success)
        {
            if (!isBuildingFree)
            {
                resourceManager.DeductResources(player, BuildingCosts.RoadCost);
            }
            Debug.Log($"PLACED ROAD: {hexEdge}");
        }
        else
        {
            Debug.Log("Invalid road position. Try again.");
        }

        return success;
    }

    public bool UpgradeSettlementToCity(IPlayer player, HexVertex hexVertex)
    {
        if (!turnManager.IsPlayerTurn(player))
        {
            Debug.LogError($"Board manager UpgradeSettlementToCity: turn is not active for {player}");
            return false;
        }

        if (turnManager.CurrentTurnType == PlayerTurnType.RegularTurn && !turnManager.HasRolledDice)
        {
            Debug.LogError($"Player {player.PlayerId} must roll dice before upgrading settlements");
            return false;
        }

        if (hexVertex == null)
        {
            Debug.LogError("Board manager UpgradeSettlementToCity: hex vertex is null");
            return false;
        }

        if (hexVertex.Building == null || hexVertex.Building.Owner != player || hexVertex.Building.Type != Building.BuildingType.Settlement)
        {
            Debug.LogError($"Board manager UpgradeSettlementToCity: hex vertex does not contain a settlement owned by player {player.PlayerId}");
            return false;
        }

        if (!resourceManager.HasEnoughResources(player, BuildingCosts.CityCost))
        {
            Debug.LogError($"Player {player.PlayerId} does not have enough resources to upgrade settlement to city");
            return false;
        }

        var success = hexVertex.Building.Upgrade();
        if (success)
        {
            resourceManager.DeductResources(player, BuildingCosts.CityCost);

            // Refresh the visual representation // TODO ideally this would be driven by BoardStateChanged instead
            if (hexVertex.VertexObject != null)
            {
                hexVertex.VertexObject.Refresh();
            }

            Debug.Log($"UPGRADED SETTLEMENT TO CITY: {hexVertex}");
        }
        else
        {
            Debug.Log("Failed to upgrade settlement to city. Try again.");
        }

        return success;
    }

    public bool MoveRobber(IPlayer player, HexTile hexTile)
    {
        if (!turnManager.IsPlayerTurn(player))
        {
            Debug.LogError($"Board manager MoveRobber: turn is not active for {player}");
            return false;
        }

        if (hexTile == null)
        {
            Debug.LogError("Board manager MoveRobber: hex tile is null");
            return false;
        }

        if (hexTile == currentRobberHexTile)
        {
            Debug.LogError("Board manager MoveRobber: robber cannot be moved to the same tile where it is already located");
            return false;
        }

        if (hexTile.TileType == TileType.Water)
        {
            Debug.LogError("Board manager MoveRobber: robber cannot be placed on a water tile");
            return false;
        }

        currentRobberHexTile = hexTile;
        currentRobberHexTile.MoveRobberToTile(robberObject);

        return true;
    }

    public int GetPlayerScore(IPlayer player)
    {
        var score = 0;

        foreach (var vertex in vertexMap.Values)
        {
            if (vertex.Building != null && vertex.Building.Owner == player)
            {
                score += vertex.Building.VictoryPoints;
            }
        }

        // TODO: 1 point for each victory card
        // TODO: 2 points for longest road
        // TODO: 2 points for largest army

        return score;
    }

    public List<HexVertex> GetAvailableSettlementLocations(IPlayer player)
    {
        var mustConnectToRoad = gameManager.SettlementsMustConnectToRoad;

        var locations = new List<HexVertex>();
        foreach (var vertex in vertexMap.Values)
        {
            if (vertex.AvailableForBuilding(player, mustConnectToRoad))
            {
                locations.Add(vertex);
            }
        }
        return locations;
    }

    public List<HexEdge> GetAvailableRoadLocations(IPlayer player)
    {
        HexVertex requiredSettlement = null;
        if (gameManager.CurrentGameState == GameManager.GameState.SecondSettlementPlacement)
        {
            requiredSettlement = GetLastSettlementPlaced(turnManager.CurrentPlayer);
        }

        var locations = new List<HexEdge>();
        foreach (var edge in edgeMap.Values)
        {
            if (edge.AvailableForBuilding(player, requiredSettlement))
            {
                locations.Add(edge);
            }
        }
        return locations;
    }

    public List<HexVertex> GetAvailableSettlementsToUpgrade(IPlayer player)
    {
        var locations = new List<HexVertex>();
        foreach (var vertex in vertexMap.Values)
        {
            if (vertex.Building != null &&
                vertex.Building.Owner == player &&
                vertex.Building.Type == Building.BuildingType.Settlement)
            {
                locations.Add(vertex);
            }
        }
        return locations;
    }

    public List<HexTile> GetAvailableRobberLocations(IPlayer player)
    {
        var locations = new List<HexTile>();
        foreach (var tile in hexTileMap.Values)
        {
            if (tile.TileType != TileType.Water && tile != currentRobberHexTile)
            {
                locations.Add(tile);
            }
        }
        return locations;
    }

    public List<IPlayer> GetPlayersWithBuildingsOnHexTile(HexTile hexTile)
    {
        var players = new List<IPlayer>();
        if (hexTile == null) return players;

        foreach (var vertex in hexTile.NeighborVertices)
        {
            if (vertex.Building != null && vertex.Building.Owner != null)
            {
                if (!players.Contains(vertex.Building.Owner))
                {
                    players.Add(vertex.Building.Owner);
                }
            }
        }
        return players;
    }

    public bool CanBuildSettlement(IPlayer player)
    {
        if (!turnManager.IsPlayerTurn(player)) return false;
        if (!turnManager.HasRolledDice) return false;
        if (!resourceManager.HasEnoughResources(player, BuildingCosts.SettlementCost)) return false;

        var availableLocations = GetAvailableSettlementLocations(player);
        return availableLocations != null && availableLocations.Count > 0;
    }

    public bool CanBuildRoad(IPlayer player)
    {
        if (!turnManager.IsPlayerTurn(player)) return false;
        if (!turnManager.HasRolledDice) return false;
        if (!resourceManager.HasEnoughResources(player, BuildingCosts.RoadCost)) return false;

        var availableLocations = GetAvailableRoadLocations(player);
        return availableLocations != null && availableLocations.Count > 0;
    }

    public bool CanUpgradeSettlement(IPlayer player)
    {
        if (!turnManager.IsPlayerTurn(player)) return false;
        if (!turnManager.HasRolledDice) return false;
        if (!resourceManager.HasEnoughResources(player, BuildingCosts.CityCost)) return false;

        var availableSettlements = GetAvailableSettlementsToUpgrade(player);
        return availableSettlements != null && availableSettlements.Count > 0;
    }
}
