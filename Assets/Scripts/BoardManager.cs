using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manages the state of the board.
/// 
/// </summary>

public class BoardManager : MonoBehaviour, IBoardManager
{
    #region Consts

    private const int INNER_SHUFFLEABLE_GRID_SIZE = 2;
    private const int FULL_GRID_SIZE = 3;

    #endregion

    #region Classes and structs

    private class PlayerTurn
    {
        public IPlayer Player;
        public PlayerTurnType TurnType;
        public bool HasRolledDice = false;

        public bool CanEndTurn
        {
            get
            {
                switch (TurnType)
                {
                    case PlayerTurnType.InitialPlacement:
                        // TODO: validate that player has successfully placed their settlement and road
                        return true;
                    case PlayerTurnType.RegularTurn:
                        // In regular turns, players must roll the dice before they can end their turn
                        return HasRolledDice;
                    default:
                        return false;
                }
            }
        }
    }

    #endregion

    #region Serialized fields

    [SerializeField]
    private GameConfig gameConfig;

    [SerializeField]
    private float horizontalSpacing = 1.5f;

    [SerializeField]
    private float verticalSpacing = 1.5f;

    [SerializeField]
    private GameObject WoodTilePrefab;
    [SerializeField]
    private GameObject ClayTilePrefab;
    [SerializeField]
    private GameObject SheepTilePrefab;
    [SerializeField]
    private GameObject WheatTilePrefab;
    [SerializeField]
    private GameObject OreTilePrefab;
    [SerializeField]
    private GameObject DesertTilePrefab;
    [SerializeField]
    private GameObject WaterTilePrefab;

    [SerializeField]
    private GameObject HexVertexPrefab;
    [SerializeField]
    private GameObject HexEdgePrefab;

    #endregion

    #region Properties

    public IReadOnlyDictionary<HexCoord, HexTile> HexMap => hexTileMap;
    public IReadOnlyDictionary<VertexCoord, HexVertex> VertexMap => vertexMap;
    public IReadOnlyDictionary<EdgeCoord, HexEdge> EdgeMap => edgeMap;

    #endregion

    #region Private members

    private System.Random random = new System.Random();   // TODO: use a single global random instance that can be seeded

    private IGameManager gameManager;

    private StateMachine<BoardMode> boardStateMachine;

    private Dictionary<HexCoord, HexTile> hexTileMap = new Dictionary<HexCoord, HexTile>();
    private Dictionary<VertexCoord, HexVertex> vertexMap = new Dictionary<VertexCoord, HexVertex>();
    private Dictionary<EdgeCoord, HexEdge> edgeMap = new Dictionary<EdgeCoord, HexEdge>();

    private Dictionary<IPlayer, ResourceHand> playerResourceHands = new Dictionary<IPlayer, ResourceHand>();

    private PlayerTurn currentPlayerTurn = null;

    private HexVertex manuallySelectedSettlementLocation = null;
    private HexEdge manuallySelectedRoadLocation = null;

    // Track the last settlement placed by each player for second settlement placement phase
    private Dictionary<IPlayer, HexVertex> lastSettlementPlaced = new Dictionary<IPlayer, HexVertex>();

    #endregion

    #region Public methods

    public void StartNewGame(IGameManager gameManager)
    {
        if (gameManager == null)
        {
            Debug.LogError("Game manager cannot be null");
            return;
        }
        this.gameManager = gameManager;

        ClearBoard();
        InitializeBoard(INNER_SHUFFLEABLE_GRID_SIZE, FULL_GRID_SIZE);

        // Clear settlement tracking for new game
        lastSettlementPlaced.Clear();

        boardStateMachine = new StateMachine<BoardMode>("BoardMode");

        boardStateMachine.AddState(BoardMode.Idle, OnEnterIdleMode, null, null);
        boardStateMachine.AddState(BoardMode.ChooseSettlementLocation, OnEnterSettlementLocationSelectionMode, null, OnExitSettlementLocationSelectionMode);
        boardStateMachine.AddState(BoardMode.ChooseRoadLocation, OnEnterRoadLocationSelectionMode, null, OnExitRoadLocationSelectionMode);

        boardStateMachine.GoToState(BoardMode.Idle);
    }

    
    public async Task<HexVertex> GetManualSelectionForSettlementLocation(IPlayer player)
    {
        if (boardStateMachine.CurrentState != BoardMode.Idle)
        {
            Debug.LogError($"Cannot get manual selection for settlement location: board is not in idle mode, current state is {boardStateMachine.CurrentState}");
            return null;
        }

        var validLocations = GetAvailableSettlementLocations(player);
        if (validLocations == null || validLocations.Count == 0)
        {
            Debug.LogWarning($"No valid settlement locations available for player {player.PlayerId}");
            return null;
        }

        manuallySelectedSettlementLocation = null;

        boardStateMachine.GoToState(BoardMode.ChooseSettlementLocation);

        while (manuallySelectedSettlementLocation == null)
        {
            await Task.Yield();
        }

        boardStateMachine.GoToState(BoardMode.Idle);

        return manuallySelectedSettlementLocation;
    }



    public async Task<HexEdge> GetManualSelectionForRoadLocation(IPlayer player)
    {
        if (boardStateMachine.CurrentState != BoardMode.Idle)
        {
            Debug.LogError($"Cannot get manual selection for road location: board is not in idle mode, current state is {boardStateMachine.CurrentState}");
            return null;
        }

        var validLocations = GetAvailableRoadLocations(player);
        if (validLocations == null || validLocations.Count == 0)
        {
            Debug.LogWarning($"No valid road locations available for player {player.PlayerId}");
            return null;
        }

        manuallySelectedRoadLocation = null;

        boardStateMachine.GoToState(BoardMode.ChooseRoadLocation);

        while (manuallySelectedRoadLocation == null)
        {
            await Task.Yield();
        }

        boardStateMachine.GoToState(BoardMode.Idle);

        return manuallySelectedRoadLocation;
    }

    private HexVertex GetLastSettlementPlaced(IPlayer player)
    {
        if (lastSettlementPlaced.TryGetValue(player, out var hexVertex))
        {
            return hexVertex;
        }
        return null;
    }

    public bool BuildSettlement(IPlayer player, HexVertex hexVertex)
    {
        if (currentPlayerTurn == null ||  currentPlayerTurn.Player != player)
        {
            Debug.LogError($"Board manager BuildSettlement: turn is not active for {player}");
            return false;
        }

        if (currentPlayerTurn.TurnType == PlayerTurnType.RegularTurn && !currentPlayerTurn.HasRolledDice)
        {
            Debug.LogError($"Player {player.PlayerId} must roll dice before building settlements");
            return false;
        }

        if (hexVertex == null)
        {
            Debug.LogError("Board manager BuildSettlement: hex vertex is null");
            return false;
        }

        var currentPlayerHand = playerResourceHands.GetValueOrDefault(player);

        var isBuildingFree = gameManager.CurrentGameState == GameManager.GameState.FirstSettlementPlacement ||
                             gameManager.CurrentGameState == GameManager.GameState.SecondSettlementPlacement;

        if (!isBuildingFree && !currentPlayerHand.HasEnoughResources(BuildingCosts.SettlementCost))
        {
            Debug.LogError($"Player {player.PlayerId} does not have enough resources to build a settlement");
            return false;
        }

        var success = hexVertex.TryPlaceBuilding(Building.BuildingType.Settlement, player);
        if (success)
        {
            // Track the last settlement placed by this player
            lastSettlementPlaced[player] = hexVertex;

            if (!isBuildingFree)
            {
                // Deduct resources from the player's hand
                currentPlayerHand.Remove(BuildingCosts.SettlementCost);
            }

            // If this is the second settlement during initial game setup, give the player resources from each of the adjacent hex tiles
            if (gameManager.CurrentGameState == GameManager.GameState.SecondSettlementPlacement)
            {
                GivePlayerResourcesForNeighborHexTiles(player, hexVertex);
            }

            Debug.Log($"PLACED SETTLEMENT: {hexVertex}");
        }
        else
        {
            Debug.Log("Invalid settlement position. Try again.");
        }

        return success;
    }

    private void GivePlayerResourcesForNeighborHexTiles(IPlayer player, HexVertex hexVertex)
    {
        if (player == null || hexVertex == null) return;
        if (!playerResourceHands.TryGetValue(player, out var hand)) return;
        if (hexVertex.Building == null) return;

        int amount = hexVertex.Building.Type == Building.BuildingType.City ? 2 : 1;

        foreach (var hex in hexVertex.NeighborHexTiles)
        {
            var resourceType = hex.ResourceType;
            if (resourceType != ResourceType.None)
            {
                hand.Add(resourceType, amount);
            }
        }
    }

    public bool BuildRoad(IPlayer player, HexEdge hexEdge)
    {
        if (currentPlayerTurn == null || currentPlayerTurn.Player != player)
        {
            Debug.LogError($"Board manager BuildRoad: turn is not active for {player}");
            return false;
        }

        if (currentPlayerTurn.TurnType == PlayerTurnType.RegularTurn && !currentPlayerTurn.HasRolledDice)
        {
            Debug.LogError($"Player {player.PlayerId} must roll dice before building roads");
            return false;
        }

        if (hexEdge == null)
        {
            Debug.LogError("Board manager BuildRoad: hex edge is null");
            return false;
        }

        var currentPlayerHand = playerResourceHands.GetValueOrDefault(player);

        var isBuildingFree = gameManager.CurrentGameState == GameManager.GameState.FirstSettlementPlacement ||
                             gameManager.CurrentGameState == GameManager.GameState.SecondSettlementPlacement;

        if (!isBuildingFree && !currentPlayerHand.HasEnoughResources(BuildingCosts.RoadCost))
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
                // Deduct resources from the player's hand
                currentPlayerHand.Remove(BuildingCosts.RoadCost);
            }
            Debug.Log($"PLACED ROAD: {hexEdge}");
        }
        else
        {
            Debug.Log("Invalid road position. Try again.");
        }

        return success;
    }

    // Call this before StartNewGame to set up player resource hands
    public void InitializePlayerResourceHands(IEnumerable<IPlayer> players)
    {
        playerResourceHands.Clear();
        foreach (var player in players)
        {
            playerResourceHands[player] = new ResourceHand();
            playerResourceHands[player].Add(ResourceType.Wood, 10);
            playerResourceHands[player].Add(ResourceType.Clay, 10);
            playerResourceHands[player].Add(ResourceType.Sheep, 10);
            playerResourceHands[player].Add(ResourceType.Wheat, 10);
        }
    }

    // Returns a copy of the resource hand for the given player, or null if not found
    public Dictionary<ResourceType, int> GetResourceHandForPlayer(IPlayer player)
    {
        if (playerResourceHands.TryGetValue(player, out var hand))
        {
            return hand.GetAll(); // returns a copy
        }
        return null;
    }

    public int GetPlayerScore(IPlayer player)
    {
        var score = 0;

        // 1 point for each settlement
        foreach (var vertex in vertexMap.Values)
        {
            if (vertex.Building != null && vertex.Building.Owner == player)
            {
                if (vertex.Building.Type == Building.BuildingType.Settlement)
                {
                    score += 1;
                }
            }
        }

        // TODO: 2 points for each city
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
        // Check if we're in second settlement placement phase and need to connect to the last settlement
        HexVertex requiredSettlement = null;
        if (gameManager.CurrentGameState == GameManager.GameState.SecondSettlementPlacement)
        {
            requiredSettlement = GetLastSettlementPlaced(currentPlayerTurn.Player);
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

    public bool BeginPlayerTurn(IPlayer player, PlayerTurnType turnType)
    {
        if (player == null || !playerResourceHands.ContainsKey(player))
        {
            Debug.LogError("Cannot begin player turn: player is null or not initialized");
            return false;
        }

        if (currentPlayerTurn != null)
        {
            Debug.LogError($"Cannot begin player turn: a player turn is already in progress for player {currentPlayerTurn.Player.PlayerId}");
            return false;
        }

        // TODO: confirm that the board is in a valid state for the turn to begin

        currentPlayerTurn = new PlayerTurn { Player = player, TurnType = turnType };

        Debug.Log($"Player {player.PlayerId} turn started");
        return true;
    }

    public bool EndPlayerTurn(IPlayer player)
    {
        if (currentPlayerTurn == null || currentPlayerTurn.Player != player)
        {
            Debug.LogError($"Cannot end player turn: no player turn in progress for player {player.PlayerId}");
            return false;
        }

        if (!currentPlayerTurn.CanEndTurn)
        {
            Debug.LogError($"Cannot end player turn");
            return false;
        }

        Debug.Log($"Player {player.PlayerId} turn ended");
        currentPlayerTurn = null;
        return true;
    }

    public async Task<int?> RollDice(IPlayer player)
    {
        if (currentPlayerTurn == null || currentPlayerTurn.Player != player)
        {
            Debug.LogError($"Cannot roll dice: no player turn in progress for player {player.PlayerId}");
            return null;
        }

        if (currentPlayerTurn.HasRolledDice)
        {
            Debug.LogError($"Cannot roll dice: player {player.PlayerId} has already rolled this turn");
            return null;
        }

        var dieA = random.Next(1, 7);
        var dieB = random.Next(1, 7);
        var diceRoll = dieA + dieB;

        Debug.Log($"Player {player.PlayerId} rolled a {diceRoll} ({dieA} + {dieB})");

        // Award resources to players who have tiles with the same dice roll
        foreach (var hexTile in hexTileMap.Values)
        {
            if (hexTile.DiceNumber == diceRoll)
            {
                GiveAllPlayersResourcesForHexTile(hexTile);
            }
        }

        // TODO: if the roll is 7, force players with more than 7 cards to discard
        // TODO: if the roll is 7, have the current player move the robber

        await Task.Delay(10); // Simulate some delay as stub for missing todos above

        currentPlayerTurn.HasRolledDice = true;

        return diceRoll;
    }

    private void GiveAllPlayersResourcesForHexTile(HexTile hexTile)
    {
        if (hexTile == null) return;
        var resourceType = hexTile.ResourceType;
        if (resourceType == ResourceType.None) return;

        foreach (var vertex in hexTile.NeighborVertices)
        {
            if (vertex == null || vertex.Building == null || vertex.Owner == null) continue;
            if (!playerResourceHands.TryGetValue(vertex.Owner, out var hand)) continue;

            int amount = vertex.Building.Type == Building.BuildingType.City ? 2 : 1;
            hand.Add(resourceType, amount);

            Debug.Log($"Player {vertex.Owner.PlayerId} awarded {amount} {resourceType} from hex {hexTile.HexCoordinates}");
        }
    }

    #endregion

    #region Unity lifecycle

    private void Update()
    {
        if (boardStateMachine == null)
        {
            return;
        }

        boardStateMachine.Update();
    }

    #endregion

    #region State machine

    private void OnEnterIdleMode()
    {
        foreach (var hexVertex in vertexMap.Values)
        {
            hexVertex.EnableSelection(false);
        }
        foreach (var hexEdge in edgeMap.Values)
        {
            hexEdge.EnableSelection(false);
        }
    }

    private void OnEnterSettlementLocationSelectionMode()
    {
        var playerColor = currentPlayerTurn.Player.PlayerColor;

        var settlementLocationList = GetAvailableSettlementLocations(currentPlayerTurn.Player);
        var settlementLocationsSet = new HashSet<HexVertex>(settlementLocationList);

        foreach (var hexVertex in vertexMap.Values)
        {
            var selectionEnabled = settlementLocationsSet.Contains(hexVertex);
            hexVertex.EnableSelection(selectionEnabled, playerColor);
        }
    }

    public void ManualSettlementLocationSelected(HexVertex hexVertex)
    {
        manuallySelectedSettlementLocation = hexVertex;
    }

    private void OnExitSettlementLocationSelectionMode()
    {
        foreach (var hexVertex in vertexMap.Values)
        {
            hexVertex.EnableSelection(false);
        }
    }

    private void OnEnterRoadLocationSelectionMode()
    {
        var playerColor = currentPlayerTurn.Player.PlayerColor;

        var roadLocationList = GetAvailableRoadLocations(currentPlayerTurn.Player);
        var roadLocationSet = new HashSet<HexEdge>(roadLocationList);

        foreach (var hexEdge in edgeMap.Values)
        {
            var selectionEnabled = roadLocationSet.Contains(hexEdge);
            hexEdge.EnableSelection(selectionEnabled, playerColor);
        }
    }

    public void ManualRoadLocationSelected(HexEdge hexEdge)
    {
        manuallySelectedRoadLocation = hexEdge;
    }

    private void OnExitRoadLocationSelectionMode()
    {
        foreach (var hexEdge in edgeMap.Values)
        {
            hexEdge.EnableSelection(false);
        }
    }

    #endregion

    private void InitializeBoard(int shuffleableGridSize, int fullGridSize)
    {
        // Initialize data structure for full grid
        for (int q = -fullGridSize; q <= fullGridSize; q++)
        {
            for (int r = -fullGridSize; r <= fullGridSize; r++)
            {
                if (q+r >= -fullGridSize && q+r <= fullGridSize)
                {
                    // Hex
                    var coord = new HexCoord(q, r);
                    var isValidForBuilding = coord.Ring <= shuffleableGridSize;
                    var hexTile = new HexTile(coord, isValidForBuilding);
                    hexTileMap.Add(coord, hexTile);

                    // Verts
                    var northVertexCoord = new VertexCoord(coord, VertexOrientation.North);
                    var northVertex = new HexVertex(northVertexCoord);
                    vertexMap.Add(northVertexCoord, northVertex);

                    var southVertexCoord = new VertexCoord(coord, VertexOrientation.South);
                    var southVertex = new HexVertex(southVertexCoord);
                    vertexMap.Add(southVertexCoord, southVertex);

                    // Edges
                    var westEdgeCoord = new EdgeCoord(coord, EdgeOrientation.West);
                    var westEdge = new HexEdge(westEdgeCoord);
                    edgeMap.Add(westEdgeCoord, westEdge);

                    var northWestEdgeCoord = new EdgeCoord(coord, EdgeOrientation.NorthWest);
                    var northWestEdge = new HexEdge(northWestEdgeCoord);
                    edgeMap.Add(northWestEdgeCoord, northWestEdge);

                    var northEastEdgeCoord = new EdgeCoord(coord, EdgeOrientation.NorthEast);
                    var northEastEdge = new HexEdge(northEastEdgeCoord);
                    edgeMap.Add(northEastEdgeCoord, northEastEdge);
                }
            }
        }

        // Spawn tile objects for the full grid.
        // Tiles within the inner shuffleable region will get randomized tiles.
        // Tiles outside the shuffleable area witll get water tiles.
        var tileTypesList = GetShuffledTileTypes();
        var shuffledTileCount = 0;
        var shuffledDiceNumbers = GetShuffledTileDiceNumbers();
        var shuffledDiceNumbersCount = 0;
        foreach (var hex in hexTileMap.Values)
        {
            var offsetCoord = GridHelpers.AxialHexToOffsetCoord(hex.HexCoordinates);
            
            var tilePosition = new Vector3(offsetCoord.x * horizontalSpacing, 0, offsetCoord.y * verticalSpacing);
            if (offsetCoord.y % 2 == 0)
            {
                tilePosition.x -= horizontalSpacing / 2;
            }

            var tilePrefab = WaterTilePrefab;
            int? diceNumber = null;
            if (hex.Ring <= shuffleableGridSize)
            {
                var tileType = tileTypesList[shuffledTileCount];
                tilePrefab = GetTilePrefab(tileType);
                shuffledTileCount++;

                if (tileType != TileType.Desert && tileType != TileType.Water)
                {
                    diceNumber = shuffledDiceNumbers[shuffledDiceNumbersCount];
                    shuffledDiceNumbersCount++;
                }
            }

            var tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
            var tileObjectComponent = tileObject.GetComponent<HexTileObject>();
            hex.TileObject = tileObjectComponent;
            tileObjectComponent.Initialize(hex, diceNumber);
        }

        // Spawn vertex objects and initialize neighbors
        foreach (var vertex in vertexMap.Values)
        {
            if (hexTileMap.TryGetValue(vertex.VertexCoord.HexCoord, out var hex))
            {
                vertex.InitializeNeighbors(this);

                if (!vertex.CanHaveBuildings())
                {
                    continue;
                }

                var vertexObject = Instantiate(HexVertexPrefab, Vector3.zero, Quaternion.identity);

                vertex.VertexObject = vertexObject.GetComponent<HexVertexObject>();
                vertex.VertexObject.Initialize(this, vertex);

                switch (vertex.VertexCoord.Orientation)
                {
                    case VertexOrientation.North:
                        vertexObject.transform.parent = hex.TileObject.NorthVertexTransform;
                        break;
                    case VertexOrientation.South:
                        vertexObject.transform.parent = hex.TileObject.SouthVertexTransform;
                        break;
                    default:
                        Debug.LogError($"Unknown vertex orientation for {vertex}");
                        break;
                }

                vertexObject.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.LogError($"Could not find parent hex for {vertex}");
            }
        }

        // After all vertices are initialized, initialize hex tile neighbors
        foreach (var hexTile in hexTileMap.Values)
        {
            hexTile.InitializeNeighbors(this);
        }

        // Spawn edge objects
        foreach (var edge in edgeMap.Values)
        {
            if (hexTileMap.TryGetValue(edge.EdgeCoord.HexCoord, out var hex))
            {
                edge.InitializeNeighbors(this);

                if (!edge.CanHaveRoads())
                {
                    continue;
                }

                var edgeObject = Instantiate(HexEdgePrefab, Vector3.zero, Quaternion.identity);

                edge.EdgeObject = edgeObject.GetComponent<HexEdgeObject>();
                edge.EdgeObject.Initialize(this, edge);

                switch (edge.EdgeCoord.Orientation)
                {
                    case EdgeOrientation.West:
                        edgeObject.transform.parent = hex.TileObject.WestEdgeTransform;
                        break;
                    case EdgeOrientation.NorthWest:
                        edgeObject.transform.parent = hex.TileObject.NorthWestEdgeTransform;
                        break;
                    case EdgeOrientation.NorthEast:
                        edgeObject.transform.parent = hex.TileObject.NorthEastEdgeTransform;
                        break;
                    default:
                        Debug.LogError($"Unknown edge orientation for {edge}");
                        break;
                }

                edgeObject.transform.localPosition = Vector3.zero;
                edgeObject.transform.localRotation = Quaternion.identity;
            }
            else
            {
                Debug.LogError($"Could not find parent hex for {edge}");
            }
        }
    }

    private void ClearBoard()
    {
        currentPlayerTurn = null;

        foreach (var tile in hexTileMap.Values)
        {
            if (tile.TileObject != null)
            {
                Destroy(tile.TileObject.gameObject);
            }
        }
        hexTileMap.Clear();

        foreach (var vertex in vertexMap.Values)
        {
            if (vertex.VertexObject != null)
            {
                Destroy(vertex.VertexObject.gameObject);
            }
        }
        vertexMap.Clear();

        foreach (var edge in edgeMap.Values)
        {
            if (edge.EdgeObject != null)
            {
                Destroy(edge.EdgeObject.gameObject);
            }
        }
        edgeMap.Clear();
    }

    private List<TileType> GetShuffledTileTypes()
    {
        var tileCounts = gameConfig.GetShuffleableTileCounts();
        var tileTypesList = new List<TileType>();
        foreach (var type in tileCounts.Keys)
        {
            var count = tileCounts[type];
            tileTypesList.AddRange(Enumerable.Repeat(type, count));
        }

        Util.Shuffle(tileTypesList);

        return tileTypesList;
    }

    private GameObject GetTilePrefab(TileType type)
    {
        switch (type)
        {
            case TileType.Wood:     return WoodTilePrefab;
            case TileType.Clay:     return ClayTilePrefab;
            case TileType.Sheep:    return SheepTilePrefab;
            case TileType.Wheat:    return WheatTilePrefab;
            case TileType.Ore:      return OreTilePrefab;
            case TileType.Desert:   return DesertTilePrefab;
            case TileType.Water:    return WaterTilePrefab;
            default:                return null;
        }
    }

    private List<int> GetShuffledTileDiceNumbers()
    {
        var diceNumbers = gameConfig.TileDiceNumbers.ToList();
        Util.Shuffle(diceNumbers);
        return diceNumbers;
    }

    #region Debugging

    // Returns a string with the current resource hands of each player for debugging
    public string GetAllPlayerResourceHandsDebugString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var kvp in playerResourceHands)
        {
            var player = kvp.Key;
            var hand = kvp.Value;
            sb.AppendLine($"Player {player.PlayerId}:");
            foreach (var resourceKvp in hand.GetAll())
            {
                sb.AppendLine($"  {resourceKvp.Key}: {resourceKvp.Value}");
            }
        }
        return sb.ToString();
    }

    #endregion
}
