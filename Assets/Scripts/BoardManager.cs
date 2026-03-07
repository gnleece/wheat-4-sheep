using Grid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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

    [SerializeField]
    private GameObject RobberPrefab;

    #endregion

    #region Properties

    public IReadOnlyDictionary<HexCoord, HexTile> HexMap => hexTileMap;
    public IReadOnlyDictionary<VertexCoord, HexVertex> VertexMap => vertexMap;
    public IReadOnlyDictionary<EdgeCoord, HexEdge> EdgeMap => edgeMap;

    public Action BoardStateChanged { get; set; } = null;

    #endregion

    #region Private members

    private readonly System.Random random = new System.Random();    // TODO: use a single global random instance that can be seeded

    private IGameManager gameManager;

    private StateMachine<BoardMode> boardStateMachine;

    private Dictionary<HexCoord, HexTile> hexTileMap = new Dictionary<HexCoord, HexTile>();
    private Dictionary<VertexCoord, HexVertex> vertexMap = new Dictionary<VertexCoord, HexVertex>();
    private Dictionary<EdgeCoord, HexEdge> edgeMap = new Dictionary<EdgeCoord, HexEdge>();

    private ResourceManager resourceManager = new ResourceManager();

    private TurnManager turnManager = new TurnManager();

    private BuildingManager buildingManager;

    private GridInitializer gridInitializer;

    private HexVertex manuallySelectedSettlementLocation = null;
    private HexEdge manuallySelectedRoadLocation = null;
    private HexVertex manuallySelectedSettlementToUpgrade = null;
    private HexTile manuallySelectedRobberLocation = null;

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

        buildingManager = new BuildingManager(vertexMap, edgeMap, hexTileMap, turnManager, resourceManager, gameManager);

        gridInitializer = new GridInitializer(
            gameConfig, random, horizontalSpacing, verticalSpacing,
            WoodTilePrefab, ClayTilePrefab, SheepTilePrefab, WheatTilePrefab, OreTilePrefab,
            DesertTilePrefab, WaterTilePrefab, HexVertexPrefab, HexEdgePrefab, RobberPrefab);

        ClearBoard();
        gridInitializer.InitializeBoard(
            INNER_SHUFFLEABLE_GRID_SIZE, FULL_GRID_SIZE,
            hexTileMap, vertexMap, edgeMap,
            this,
            out var robberObj, out var robberTile);
        buildingManager.InitializeRobber(robberObj, robberTile);

        boardStateMachine = new StateMachine<BoardMode>("BoardMode");

        boardStateMachine.AddState(BoardMode.Idle, OnEnterIdleMode, null, null);
        boardStateMachine.AddState(BoardMode.ChooseSettlementLocation, OnEnterSettlementLocationSelectionMode, null, OnExitSettlementLocationSelectionMode);
        boardStateMachine.AddState(BoardMode.ChooseRoadLocation, OnEnterRoadLocationSelectionMode, null, OnExitRoadLocationSelectionMode);
        boardStateMachine.AddState(BoardMode.ChooseSettlementToUpgrade, OnEnterSettlementUpgradeSelectionMode, null, OnExitSettlementUpgradeSelectionMode);
        boardStateMachine.AddState(BoardMode.ChooseRobberLocation, OnEnterRobberLocationSelectionMode, null, OnExitRobberLocationSelectionMode);

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

    public async Task<HexVertex> GetManualSelectionForSettlementUpgrade(IPlayer player)
    {
        if (boardStateMachine.CurrentState != BoardMode.Idle)
        {
            Debug.LogError($"Cannot get manual selection for settlement upgrade: board is not in idle mode, current state is {boardStateMachine.CurrentState}");
            return null;
        }

        var validLocations = GetAvailableSettlementsToUpgrade(player);
        if (validLocations == null || validLocations.Count == 0)
        {
            Debug.LogWarning($"No valid settlements available for upgrade for player {player.PlayerId}");
            return null;
        }

        manuallySelectedSettlementToUpgrade = null;

        boardStateMachine.GoToState(BoardMode.ChooseSettlementToUpgrade);

        while (manuallySelectedSettlementToUpgrade == null)
        {
            await Task.Yield();
        }

        boardStateMachine.GoToState(BoardMode.Idle);

        return manuallySelectedSettlementToUpgrade;
    }

    public async Task<HexTile> GetManualSelectionForRobberLocation(IPlayer player)
    {
        if (boardStateMachine.CurrentState != BoardMode.Idle)
        {
            Debug.LogError($"Cannot get manual selection for robber location: board is not in idle mode, current state is {boardStateMachine.CurrentState}");
            return null;
        }

        var validLocations = GetAvailableRobberLocations(player);
        if (validLocations == null || validLocations.Count == 0)
        {
            Debug.LogWarning($"No valid robber locations available for player {player.PlayerId}");
            return null;
        }

        manuallySelectedRobberLocation = null;

        boardStateMachine.GoToState(BoardMode.ChooseRobberLocation);

        while (manuallySelectedRobberLocation == null)
        {
            await Task.Yield();
        }

        boardStateMachine.GoToState(BoardMode.Idle);

        return manuallySelectedRobberLocation;
    }

    public async Task GetManualDiscardOnSevenRoll(IPlayer player, ResourceHand hand, int cardsToDiscard)
    {
        Debug.Log($"Showing manual discard UI for Player {player.PlayerId} to discard {cardsToDiscard} cards...");
        
        // Show discard UI for human player
        var uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            await uiManager.ShowDiscardUI(player, hand, cardsToDiscard);
        }
        else
        {
            Debug.LogError("UIManager not found! Cannot show discard UI for human player.");
        }
    }

    public async Task<IPlayer> GetManualSelectionForPlayerToStealFrom(IPlayer currentPlayer, List<IPlayer> availablePlayers)
    {
        Debug.Log($"Showing player selection UI for Player {currentPlayer.PlayerId} to choose who to steal from...");
        
        // Show player selection UI for human player
        var uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            return await uiManager.ShowPlayerSelectionUI(currentPlayer, availablePlayers);
        }
        else
        {
            Debug.LogError("UIManager not found! Cannot show player selection UI for human player.");
            return null;
        }
    }

    public bool BuildSettlement(IPlayer player, HexVertex hexVertex)
    {
        var success = buildingManager.BuildSettlement(player, hexVertex);
        BoardStateChanged?.Invoke();
        return success;
    }

    public bool BuildRoad(IPlayer player, HexEdge hexEdge)
    {
        var success = buildingManager.BuildRoad(player, hexEdge);
        BoardStateChanged?.Invoke();
        return success;
    }

    public bool UpgradeSettlementToCity(IPlayer player, HexVertex hexVertex)
    {
        var success = buildingManager.UpgradeSettlementToCity(player, hexVertex);
        BoardStateChanged?.Invoke();
        return success;
    }

    public bool MoveRobber(IPlayer player, HexTile hexTile)
    {
        var success = buildingManager.MoveRobber(player, hexTile);
        BoardStateChanged?.Invoke();
        return success;
    }

    // Call this before StartNewGame to set up player resource hands
    public void InitializePlayerResourceHands(IEnumerable<IPlayer> players)
    {
        resourceManager.Initialize(players);
    }

    // Returns a copy of the resource hand for the given player, or null if not found
    public Dictionary<ResourceType, int> GetResourceHandForPlayer(IPlayer player)
    {
        return resourceManager.GetResourceHandForPlayer(player);
    }

    public int GetPlayerScore(IPlayer player) => buildingManager?.GetPlayerScore(player) ?? 0;

    public List<HexVertex> GetAvailableSettlementLocations(IPlayer player) => buildingManager.GetAvailableSettlementLocations(player);

    public List<HexEdge> GetAvailableRoadLocations(IPlayer player) => buildingManager.GetAvailableRoadLocations(player);

    public List<HexVertex> GetAvailableSettlementsToUpgrade(IPlayer player) => buildingManager.GetAvailableSettlementsToUpgrade(player);

    public List<HexTile> GetAvailableRobberLocations(IPlayer player) => buildingManager.GetAvailableRobberLocations(player);

    public List<IPlayer> GetPlayersWithBuildingsOnHexTile(HexTile hexTile) => buildingManager.GetPlayersWithBuildingsOnHexTile(hexTile);

    public ResourceType? StealRandomResourceFromPlayer(IPlayer fromPlayer, IPlayer toPlayer) => resourceManager.StealRandomResourceFromPlayer(fromPlayer, toPlayer);

    public bool CanBuildSettlement(IPlayer player) => buildingManager.CanBuildSettlement(player);

    public bool CanBuildRoad(IPlayer player) => buildingManager.CanBuildRoad(player);

    public bool CanUpgradeSettlement(IPlayer player) => buildingManager.CanUpgradeSettlement(player);

    public bool CanRollDice(IPlayer player)
    {
        // Is it this player's turn?
        if (!turnManager.IsPlayerTurn(player))
        {
            return false;
        }

        // Have they already rolled the dice?
        if (turnManager.HasRolledDice)
        {
            return false;
        }

        return true;
    }

    public bool BeginPlayerTurn(IPlayer player, PlayerTurnType turnType)
    {
        if (player == null || !resourceManager.ContainsPlayer(player))
        {
            Debug.LogError("Cannot begin player turn: player is null or not initialized");
            return false;
        }

        // TODO: confirm that the board is in a valid state for the turn to begin

        var success = turnManager.BeginTurn(player, turnType);
        if (success) BoardStateChanged?.Invoke();
        return success;
    }

    public bool EndPlayerTurn(IPlayer player)
    {
        var success = turnManager.EndTurn(player);
        if (success) BoardStateChanged?.Invoke();
        return success;
    }

    public bool IsPlayerTurn(IPlayer player)
    {
        return turnManager.IsPlayerTurn(player);
    }

    public bool CanEndTurn(IPlayer player)
    {
        return turnManager.CanEndTurn(player);
    }

    public async Task<int?> RollDice(IPlayer player)
    {
        if (!turnManager.IsPlayerTurn(player))
        {
            Debug.LogError($"Cannot roll dice: no player turn in progress for player {player.PlayerId}");
            return null;
        }

        if (turnManager.HasRolledDice)
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

        // Handle 7 roll - force players with more than 7 cards to discard
        if (diceRoll == 7)
        {
            await HandleSevenRollDiscard();         // TODO: check rules to confirm which order these two steps should occur in
            await HandleSevenRollMoveRobber();
        }

        turnManager.SetHasRolledDice();

        BoardStateChanged?.Invoke();
        return diceRoll;
    }

    private async Task HandleSevenRollDiscard()
    {
        await resourceManager.HandleSevenRollDiscard();
    }

    private async Task HandleSevenRollMoveRobber()
    {
        await turnManager.CurrentPlayer.MoveRobber();

        // After moving the robber, let the current player choose another player on the robber's hex to steal from
        var playersOnRobberHex = buildingManager.GetPlayersWithBuildingsOnHexTile(buildingManager.CurrentRobberHexTile);
        
        // Remove the current player from the list (can't steal from yourself)
        playersOnRobberHex.RemoveAll(p => p == turnManager.CurrentPlayer);
        
        if (playersOnRobberHex.Count > 0)
        {
            Debug.Log($"Player {turnManager.CurrentPlayer.PlayerId} can steal from {playersOnRobberHex.Count} players on the robber's hex");
            
            // Let the current player choose who to steal from
            var playerToStealFrom = await turnManager.CurrentPlayer.ChoosePlayerToStealFrom(playersOnRobberHex);
            
            if (playerToStealFrom != null)
            {
                // Steal a random resource from the chosen player
                var stolenResource = StealRandomResourceFromPlayer(playerToStealFrom, turnManager.CurrentPlayer);
                if (stolenResource.HasValue)
                {
                    Debug.Log($"Player {turnManager.CurrentPlayer.PlayerId} successfully stole 1 {stolenResource.Value} from Player {playerToStealFrom.PlayerId}");
                }
                else
                {
                    Debug.Log($"Player {playerToStealFrom.PlayerId} had no resources to steal");
                }
            }
            else
            {
                Debug.Log("No player was chosen to steal from");
            }
        }
        else
        {
            Debug.Log("No players have buildings on the robber's hex tile - nothing to steal");
        }
    }

    private void GiveAllPlayersResourcesForHexTile(HexTile hexTile)
    {
        resourceManager.GiveAllPlayersResourcesForHexTile(hexTile, buildingManager.CurrentRobberHexTile);
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
        foreach (var hexTile in hexTileMap.Values)
        {
            hexTile.EnableSelection(false);
        }
    }

    private void OnEnterSettlementLocationSelectionMode()
    {
        var playerColor = turnManager.CurrentPlayer.PlayerColor;

        var settlementLocationList = GetAvailableSettlementLocations(turnManager.CurrentPlayer);
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

    public void ManualVertexSelected(HexVertex hexVertex)
    {
        // Route the selection based on current board mode
        switch (boardStateMachine.CurrentState)
        {
            case BoardMode.ChooseSettlementLocation:
                ManualSettlementLocationSelected(hexVertex);    // TODO: these methods don't need to be public anymore
                break;
            case BoardMode.ChooseSettlementToUpgrade:
                ManualSettlementUpgradeLocationSelected(hexVertex);
                break;
            default:
                // Fallback to settlement selection for backward compatibility
                ManualSettlementLocationSelected(hexVertex);
                break;
        }
    }

    public void ManualHexTileSelected(HexTile hexTile)
    {
        manuallySelectedRobberLocation = hexTile;
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
        var playerColor = turnManager.CurrentPlayer.PlayerColor;

        var roadLocationList = GetAvailableRoadLocations(turnManager.CurrentPlayer);
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

    private void OnEnterSettlementUpgradeSelectionMode()
    {
        var playerColor = turnManager.CurrentPlayer.PlayerColor;

        var settlementUpgradeList = GetAvailableSettlementsToUpgrade(turnManager.CurrentPlayer);
        var settlementUpgradeSet = new HashSet<HexVertex>(settlementUpgradeList);

        foreach (var hexVertex in vertexMap.Values)
        {
            var selectionEnabled = settlementUpgradeSet.Contains(hexVertex);
            hexVertex.EnableSelection(selectionEnabled, playerColor);
        }
    }

    public void ManualSettlementUpgradeLocationSelected(HexVertex hexVertex)
    {
        manuallySelectedSettlementToUpgrade = hexVertex;
    }

    private void OnExitSettlementUpgradeSelectionMode()
    {
        foreach (var hexVertex in vertexMap.Values)
        {
            hexVertex.EnableSelection(false);
        }
    }

    private void OnEnterRobberLocationSelectionMode()
    {
        var playerColor = turnManager.CurrentPlayer.PlayerColor;

        var robberLocationList = GetAvailableRobberLocations(turnManager.CurrentPlayer);
        var robberLocationSet = new HashSet<HexTile>(robberLocationList);

        foreach (var hexTile in hexTileMap.Values)
        {
            var selectionEnabled = robberLocationSet.Contains(hexTile);
            hexTile.EnableSelection(selectionEnabled, playerColor);
        }
    }

    private void OnExitRobberLocationSelectionMode()
    {
        foreach (var hexTile in hexTileMap.Values)
        {
            hexTile.EnableSelection(false);
        }
    }

    #endregion

    private void ClearBoard()
    {
        turnManager.Clear();
        gridInitializer?.ClearBoard(hexTileMap, vertexMap, edgeMap);
    }

    #region Debugging

    // Returns a string with the current resource hands of each player for debugging
    public string GetAllPlayerResourceHandsDebugString()
    {
        return resourceManager.GetAllPlayerResourceHandsDebugString();
    }

    #endregion
}
