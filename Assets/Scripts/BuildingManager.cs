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

    // Returns true if the vertex is a valid, unoccupied location for player to build a settlement,
    // respecting the distance rule and (optionally) the road-connection requirement.
    private bool IsVertexAvailableForBuilding(HexVertex vertex, IPlayer player, bool mustConnectToRoad)
    {
        if (!vertex.CanHaveBuildings())
        {
            return false;
        }

        if (vertex.IsOccupied)
        {
            return false;
        }

        // Distance rule: no adjacent vertex may be occupied.
        foreach (var neighbor in vertex.NeighborVertices)
        {
            if (neighbor.IsOccupied)
            {
                return false;
            }
        }

        if (mustConnectToRoad)
        {
            foreach (var edge in vertex.NeighborEdges)
            {
                if (edge.IsOccupied && edge.Road.Owner == player)
                {
                    return true;
                }
            }
            return false;
        }

        return true;
    }

    // Returns true if the edge is a valid, unoccupied location for player to build a road.
    // When requiredNeighborVertex is non-null (initial road placement after second settlement),
    // the edge must be adjacent to that specific vertex.
    private bool IsEdgeAvailableForBuilding(HexEdge edge, IPlayer player, HexVertex requiredNeighborVertex)
    {
        if (!edge.CanHaveRoads())
        {
            return false;
        }

        if (edge.IsOccupied)
        {
            return false;
        }

        if (requiredNeighborVertex != null)
        {
            foreach (var vertex in edge.NeighborVertices)
            {
                if (vertex == requiredNeighborVertex)
                {
                    return true;
                }
            }
            return false;
        }

        // Road must connect to a building or road owned by the player.
        foreach (var vertex in edge.NeighborVertices)
        {
            if (vertex.IsOccupied && vertex.Owner == player)
            {
                return true;
            }
        }

        foreach (var neighborEdge in edge.NeighborEdges)
        {
            if (neighborEdge.IsOccupied && neighborEdge.Road.Owner == player)
            {
                return true;
            }
        }

        return false;
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

        // Validate placement: unoccupied vertex, distance rule satisfied.
        if (hexVertex.IsOccupied || player == null)
        {
            Debug.Log("Invalid settlement position. Try again.");
            return false;
        }

        foreach (var neighbor in hexVertex.NeighborVertices)
        {
            if (neighbor.IsOccupied)
            {
                Debug.Log("Invalid settlement position. Try again.");
                return false;
            }
        }

        hexVertex.PlaceBuilding(new Building(Building.BuildingType.Settlement, hexVertex, player));
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
        return true;
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

        var isInitialPlacement = gameManager.CurrentGameState == GameManager.GameState.FirstSettlementPlacement ||
                                 gameManager.CurrentGameState == GameManager.GameState.SecondSettlementPlacement;
        var hasFreeRoad = turnManager.FreeRoadsRemaining > 0;
        var isBuildingFree = isInitialPlacement || hasFreeRoad;

        if (!isBuildingFree && !resourceManager.HasEnoughResources(player, BuildingCosts.RoadCost))
        {
            Debug.LogError($"Player {player.PlayerId} does not have enough resources to build a road");
            return false;
        }

        Debug.Log($"Trying to place road at {hexEdge}");

        if (hexEdge.IsOccupied || player == null)
        {
            Debug.Log("Invalid road position. Try again.");
            return false;
        }

        // Road must connect to an existing building or road owned by the player.
        var connected = false;

        foreach (var vertex in hexEdge.NeighborVertices)
        {
            Debug.Log($"....neighbor vertex: {vertex}, occupied = {vertex.IsOccupied}, owner = {vertex.Owner}");
            if (vertex.IsOccupied && vertex.Owner == player)
            {
                connected = true;
                break;
            }
        }

        if (!connected)
        {
            foreach (var neighborEdge in hexEdge.NeighborEdges)
            {
                Debug.Log($"....neighbor edge: {neighborEdge}, occupied = {neighborEdge.IsOccupied}, owner = {neighborEdge.Road?.Owner}");
                if (neighborEdge.IsOccupied && neighborEdge.Road.Owner == player)
                {
                    connected = true;
                    break;
                }
            }
        }

        if (!connected)
        {
            Debug.Log("Invalid road position. Try again.");
            return false;
        }

        hexEdge.PlaceRoad(new Road(hexEdge, player));

        if (!isBuildingFree)
        {
            resourceManager.DeductResources(player, BuildingCosts.RoadCost);
        }
        else if (hasFreeRoad && !isInitialPlacement)
        {
            turnManager.UseOneRoad();
        }

        Debug.Log($"PLACED ROAD: {hexEdge}");
        return true;
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

    public int GetPlayerBuildingScore(IPlayer player)
    {
        var score = 0;

        foreach (var vertex in vertexMap.Values)
        {
            if (vertex.Building != null && vertex.Building.Owner == player)
            {
                score += vertex.Building.VictoryPoints;
            }
        }

        return score;
    }

    public List<HexVertex> GetAvailableSettlementLocations(IPlayer player)
    {
        var mustConnectToRoad = gameManager.SettlementsMustConnectToRoad;

        var locations = new List<HexVertex>();
        foreach (var vertex in vertexMap.Values)
        {
            if (IsVertexAvailableForBuilding(vertex, player, mustConnectToRoad))
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
            if (IsEdgeAvailableForBuilding(edge, player, requiredSettlement))
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

        var hasFreeRoad = turnManager.FreeRoadsRemaining > 0;
        if (!hasFreeRoad && !resourceManager.HasEnoughResources(player, BuildingCosts.RoadCost)) return false;

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
