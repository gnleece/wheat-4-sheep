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
    private BoardPrefabConfig boardPrefabConfig;

    [SerializeField]
    private float horizontalSpacing = 1.5f;

    [SerializeField]
    private float verticalSpacing = 1.5f;

    #endregion

    #region Properties

    public IReadOnlyDictionary<HexCoord, HexTile> HexMap => _hexTileMap;
    public IReadOnlyDictionary<VertexCoord, HexVertex> VertexMap => _vertexMap;
    public IReadOnlyDictionary<EdgeCoord, HexEdge> EdgeMap => _edgeMap;

    public Action BoardStateChanged { get; set; } = null;

    #endregion

    #region Private members

    private IRandomProvider _random;
    private IUIManager _uiManager;
    private ResourceManager _resourceManager;
    private BuildingManager _buildingManager;
    private DevelopmentCardManager _devCardManager;
    private TradeManager _tradeManager;
    private GridInitializer _gridInitializer;
    private readonly TurnManager _turnManager = new();

    private StateMachine<BoardMode> _boardStateMachine;

    private readonly Dictionary<HexCoord, HexTile> _hexTileMap = new();
    private readonly Dictionary<VertexCoord, HexVertex> _vertexMap = new();
    private readonly Dictionary<EdgeCoord, HexEdge> _edgeMap = new();
    
    private TaskCompletionSource<object> _pendingSelection;

    #endregion

    #region Public methods

    public void StartNewGame(IGameManager gameManager, IUIManager uiManager)
    {
        if (gameManager == null)
        {
            Debug.LogError("Game manager cannot be null");
            return;
        }

        _uiManager = uiManager;

        _buildingManager = new BuildingManager(_vertexMap, _edgeMap, _hexTileMap, _turnManager, _resourceManager, gameManager);

        _gridInitializer = new GridInitializer(gameConfig, _random, horizontalSpacing, verticalSpacing, boardPrefabConfig);

        ClearBoard();
        _gridInitializer.InitializeBoard(
            INNER_SHUFFLEABLE_GRID_SIZE, FULL_GRID_SIZE,
            _hexTileMap, _vertexMap, _edgeMap,
            this,
            out var robberObj, out var robberTile);
        _buildingManager.InitializeRobber(robberObj, robberTile);

        _boardStateMachine = new StateMachine<BoardMode>("BoardMode");

        _boardStateMachine.AddState(BoardMode.Idle, OnEnterIdleMode, null, null);
        _boardStateMachine.AddState(BoardMode.ChooseSettlementLocation, OnEnterSettlementLocationSelectionMode, null, OnExitSettlementLocationSelectionMode);
        _boardStateMachine.AddState(BoardMode.ChooseRoadLocation, OnEnterRoadLocationSelectionMode, null, OnExitRoadLocationSelectionMode);
        _boardStateMachine.AddState(BoardMode.ChooseSettlementToUpgrade, OnEnterSettlementUpgradeSelectionMode, null, OnExitSettlementUpgradeSelectionMode);
        _boardStateMachine.AddState(BoardMode.ChooseRobberLocation, OnEnterRobberLocationSelectionMode, null, OnExitRobberLocationSelectionMode);

        _boardStateMachine.GoToState(BoardMode.Idle);
    }
    
    public async Task<HexVertex> GetManualSelectionForSettlementLocation(IPlayer player)
    {
        if (_boardStateMachine.CurrentState != BoardMode.Idle)
        {
            Debug.LogError($"Cannot get manual selection for settlement location: board is not in idle mode, current state is {_boardStateMachine.CurrentState}");
            return null;
        }

        var validLocations = GetAvailableSettlementLocations(player);
        if (validLocations == null || validLocations.Count == 0)
        {
            Debug.LogWarning($"No valid settlement locations available for player {player.PlayerId}");
            return null;
        }

        return await AwaitBoardSelection<HexVertex>(BoardMode.ChooseSettlementLocation);
    }

    public async Task<HexEdge> GetManualSelectionForRoadLocation(IPlayer player)
    {
        if (_boardStateMachine.CurrentState != BoardMode.Idle)
        {
            Debug.LogError($"Cannot get manual selection for road location: board is not in idle mode, current state is {_boardStateMachine.CurrentState}");
            return null;
        }

        var validLocations = GetAvailableRoadLocations(player);
        if (validLocations == null || validLocations.Count == 0)
        {
            Debug.LogWarning($"No valid road locations available for player {player.PlayerId}");
            return null;
        }

        return await AwaitBoardSelection<HexEdge>(BoardMode.ChooseRoadLocation);
    }

    public async Task<HexVertex> GetManualSelectionForSettlementUpgrade(IPlayer player)
    {
        if (_boardStateMachine.CurrentState != BoardMode.Idle)
        {
            Debug.LogError($"Cannot get manual selection for settlement upgrade: board is not in idle mode, current state is {_boardStateMachine.CurrentState}");
            return null;
        }

        var validLocations = GetAvailableSettlementsToUpgrade(player);
        if (validLocations == null || validLocations.Count == 0)
        {
            Debug.LogWarning($"No valid settlements available for upgrade for player {player.PlayerId}");
            return null;
        }

        return await AwaitBoardSelection<HexVertex>(BoardMode.ChooseSettlementToUpgrade);
    }

    public async Task<HexTile> GetManualSelectionForRobberLocation(IPlayer player)
    {
        if (_boardStateMachine.CurrentState != BoardMode.Idle)
        {
            Debug.LogError($"Cannot get manual selection for robber location: board is not in idle mode, current state is {_boardStateMachine.CurrentState}");
            return null;
        }

        var validLocations = GetAvailableRobberLocations(player);
        if (validLocations == null || validLocations.Count == 0)
        {
            Debug.LogWarning($"No valid robber locations available for player {player.PlayerId}");
            return null;
        }

        return await AwaitBoardSelection<HexTile>(BoardMode.ChooseRobberLocation);
    }

    public void CompleteSelection(object selection)
    {
        _pendingSelection?.TrySetResult(selection);
    }

    private async Task<T> AwaitBoardSelection<T>(BoardMode mode)
    {
        _pendingSelection = new TaskCompletionSource<object>();
        _boardStateMachine.GoToState(mode);
        var result = (T)await _pendingSelection.Task;
        _boardStateMachine.GoToState(BoardMode.Idle);
        _pendingSelection = null;
        return result;
    }

    public async Task GetManualDiscardOnSevenRoll(IPlayer player, ResourceHand hand, int cardsToDiscard)
    {
        Debug.Log($"Showing manual discard UI for Player {player.PlayerId} to discard {cardsToDiscard} cards...");
        
        // Show discard UI for human player
        if (_uiManager != null)
        {
            await _uiManager.ShowDiscardUI(player, hand, cardsToDiscard);
        }
        else
        {
            Debug.LogError("UIManager not set! Cannot show discard UI for human player.");
        }
    }

    public async Task<IPlayer> GetManualSelectionForPlayerToStealFrom(IPlayer currentPlayer, List<IPlayer> availablePlayers)
    {
        Debug.Log($"Showing player selection UI for Player {currentPlayer.PlayerId} to choose who to steal from...");
        
        // Show player selection UI for human player
        if (_uiManager != null)
        {
            return await _uiManager.ShowPlayerSelectionUI(currentPlayer, availablePlayers);
        }
        else
        {
            Debug.LogError("UIManager not set! Cannot show player selection UI for human player.");
            return null;
        }
    }

    public bool BuildSettlement(IPlayer player, HexVertex hexVertex)
    {
        var success = _buildingManager.BuildSettlement(player, hexVertex);
        BoardStateChanged?.Invoke();
        return success;
    }

    public bool BuildRoad(IPlayer player, HexEdge hexEdge)
    {
        var success = _buildingManager.BuildRoad(player, hexEdge);
        BoardStateChanged?.Invoke();
        return success;
    }

    public bool UpgradeSettlementToCity(IPlayer player, HexVertex hexVertex)
    {
        var success = _buildingManager.UpgradeSettlementToCity(player, hexVertex);
        BoardStateChanged?.Invoke();
        return success;
    }

    public bool MoveRobber(IPlayer player, HexTile hexTile)
    {
        var success = _buildingManager.MoveRobber(player, hexTile);
        BoardStateChanged?.Invoke();
        return success;
    }

    // Call this before StartNewGame to set up player resource hands and dev card system
    public void InitializePlayerResourceHands(IEnumerable<IPlayer> players, IRandomProvider randomProvider)
    {
        _random = randomProvider;
        _resourceManager = new ResourceManager(_random);

        var playerList = new System.Collections.Generic.List<IPlayer>(players);
        var extraResources = new System.Collections.Generic.Dictionary<ResourceType, int>
        {
            { ResourceType.Wood,  gameConfig.StartingWoodCardCount  },
            { ResourceType.Clay,  gameConfig.StartingClayCardCount  },
            { ResourceType.Sheep, gameConfig.StartingSheepCardCount },
            { ResourceType.Wheat, gameConfig.StartingWheatCardCount },
            { ResourceType.Ore,   gameConfig.StartingOreCardCount   },
        };
        _resourceManager.Initialize(playerList, extraResources);
        _devCardManager = new DevelopmentCardManager(_resourceManager, _turnManager, _random);
        _devCardManager.Initialize(playerList);
        _tradeManager = new TradeManager(_resourceManager, _turnManager, new PortAwareBankTradeRateProvider(this));
        _tradeManager.Initialize(playerList);
    }

    // Returns a copy of the resource hand for the given player, or null if not found
    public Dictionary<ResourceType, int> GetResourceHandForPlayer(IPlayer player)
    {
        return _resourceManager.GetResourceHandForPlayer(player);
    }

    public int GetPlayerScore(IPlayer player)
    {
        var score = _buildingManager?.GetPlayerBuildingScore(player) ?? 0;
        score += _devCardManager?.GetVictoryPointBonus(player) ?? 0;
        score += _devCardManager?.GetLargestArmyBonus(player) ?? 0;
        return score;
    }

    public bool CanBuyDevelopmentCard(IPlayer player) => _devCardManager?.CanBuyDevelopmentCard(player) ?? false;

    public DevelopmentCardType BuyDevelopmentCard(IPlayer player)
    {
        var card = _devCardManager.BuyDevelopmentCard(player);
        BoardStateChanged?.Invoke();
        return card;
    }

    public Dictionary<DevelopmentCardType, int> GetDevCardHandForPlayer(IPlayer player)
        => _devCardManager?.GetPlayerHand(player) ?? new Dictionary<DevelopmentCardType, int>();

    public bool CanPlayAnyDevCard(IPlayer player) => _devCardManager?.CanPlayAnyDevCard(player) ?? false;

    public async Task PlayDevelopmentCard(IPlayer player, DevelopmentCardType cardType)
    {
        if (_devCardManager == null || !_devCardManager.ConsumeDevCard(player, cardType))
        {
            return;
        }

        switch (cardType)
        {
            case DevelopmentCardType.Knight:
                _devCardManager.RecordKnightPlayed(player);
                await HandleMoveRobber();
                break;

            case DevelopmentCardType.YearOfPlenty:
                var resource1 = await GetManualResourceTypeSelection(player, "Choose first resource:");
                _devCardManager.GiveResourcesToPlayer(player, resource1, 1);
                var resource2 = await GetManualResourceTypeSelection(player, "Choose second resource:");
                _devCardManager.GiveResourcesToPlayer(player, resource2, 1);
                break;

            case DevelopmentCardType.Monopoly:
                var monopolyResource = await GetManualResourceTypeSelection(player, "Choose resource to monopolize:");
                _devCardManager.ExecuteMonopoly(player, monopolyResource);
                break;

            case DevelopmentCardType.RoadBuilding:
                _turnManager.AddFreeRoads(2);
                Debug.Log($"Player {player.PlayerId} played Road Building: 2 free roads granted.");
                break;

            case DevelopmentCardType.VictoryPoint:
                // VP cards are passive; ConsumeDevCard should not allow this to be reached
                Debug.LogWarning("VP card should not be actively played.");
                break;
        }

        BoardStateChanged?.Invoke();
    }

    public async Task<DevelopmentCardType> GetManualDevCardSelection(IPlayer player)
    {
        if (_uiManager == null)
        {
            Debug.LogError("UIManager not set! Cannot show dev card selection UI.");
            return default;
        }

        var hand = GetDevCardHandForPlayer(player);
        return await _uiManager.ShowDevCardSelectionUI(player, hand);
    }

    public async Task<ResourceType> GetManualResourceTypeSelection(IPlayer player, string prompt)
    {
        if (_uiManager == null)
        {
            Debug.LogError("UIManager not set! Cannot show resource type selection UI.");
            return ResourceType.None;
        }

        return await _uiManager.ShowResourceTypeSelectionUI(player, prompt);
    }

    public bool CanInitiateTrade(IPlayer player) => _tradeManager?.CanInitiateTrade(player) ?? false;

    public int GetBankTradeRate(IPlayer player, ResourceType resourceType)
        => _tradeManager?.GetBankTradeRate(player, resourceType) ?? 4;

    public bool CanBankTrade(IPlayer player, ResourceType giving, ResourceType receiving)
        => _tradeManager?.CanBankTrade(player, giving, receiving) ?? false;

    public void ExecuteBankTrade(IPlayer player, ResourceType giving, ResourceType receiving)
    {
        _tradeManager?.ExecuteBankTrade(player, giving, receiving);
        BoardStateChanged?.Invoke();
    }

    public async Task GetManualTradeSelection(IPlayer player)
    {
        if (_uiManager == null)
        {
            Debug.LogError("UIManager not set! Cannot show trade UI.");
            return;
        }

        await _uiManager.ShowTradeUI(player);
        BoardStateChanged?.Invoke();
    }

    public async Task<bool> GetManualTradeOfferResponse(IPlayer player, TradeOffer offer)
    {
        if (_uiManager == null)
        {
            Debug.LogError("UIManager not set! Cannot show trade offer UI.");
            return false;
        }

        return await _uiManager.ShowTradeOfferUI(player, offer);
    }

    public async Task<bool> ProposePlayerTrade(IPlayer initiator, TradeOffer offer)
    {
        if (_tradeManager == null)
        {
            return false;
        }

        var otherPlayers = _tradeManager.GetOtherPlayers(initiator);
        foreach (var player in otherPlayers)
        {
            if (!_tradeManager.CanExecutePlayerTrade(offer, player))
            {
                continue;
            }

            bool accepted = await player.ConsiderTradeOffer(offer);
            if (accepted)
            {
                _tradeManager.ExecutePlayerTrade(offer, player);
                BoardStateChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public List<HexVertex> GetAvailableSettlementLocations(IPlayer player) => _buildingManager.GetAvailableSettlementLocations(player);

    public List<HexEdge> GetAvailableRoadLocations(IPlayer player) => _buildingManager.GetAvailableRoadLocations(player);

    public List<HexVertex> GetAvailableSettlementsToUpgrade(IPlayer player) => _buildingManager.GetAvailableSettlementsToUpgrade(player);

    public List<HexTile> GetAvailableRobberLocations(IPlayer player) => _buildingManager.GetAvailableRobberLocations(player);

    public List<IPlayer> GetPlayersWithBuildingsOnHexTile(HexTile hexTile) => _buildingManager.GetPlayersWithBuildingsOnHexTile(hexTile);

    public ResourceType? StealRandomResourceFromPlayer(IPlayer fromPlayer, IPlayer toPlayer) => _resourceManager.StealRandomResourceFromPlayer(fromPlayer, toPlayer);

    public bool CanBuildSettlement(IPlayer player) => _buildingManager.CanBuildSettlement(player);

    public bool CanBuildRoad(IPlayer player) => _buildingManager.CanBuildRoad(player);

    public bool CanUpgradeSettlement(IPlayer player) => _buildingManager.CanUpgradeSettlement(player);

    public bool CanRollDice(IPlayer player)
    {
        // Is it this player's turn?
        if (!_turnManager.IsPlayerTurn(player))
        {
            return false;
        }

        // Have they already rolled the dice?
        if (_turnManager.HasRolledDice)
        {
            return false;
        }

        return true;
    }

    public bool BeginPlayerTurn(IPlayer player, PlayerTurnType turnType)
    {
        if (player == null || !_resourceManager.ContainsPlayer(player))
        {
            Debug.LogError("Cannot begin player turn: player is null or not initialized");
            return false;
        }

        // TODO: confirm that the board is in a valid state for the turn to begin

        var success = _turnManager.BeginTurn(player, turnType);
        if (success) BoardStateChanged?.Invoke();
        return success;
    }

    public bool EndPlayerTurn(IPlayer player)
    {
        var success = _turnManager.EndTurn(player);
        if (success) BoardStateChanged?.Invoke();
        return success;
    }

    public bool IsPlayerTurn(IPlayer player)
    {
        return _turnManager.IsPlayerTurn(player);
    }

    public bool CanEndTurn(IPlayer player)
    {
        return _turnManager.CanEndTurn(player);
    }

    public async Task<int?> RollDice(IPlayer player)
    {
        if (!_turnManager.IsPlayerTurn(player))
        {
            Debug.LogError($"Cannot roll dice: no player turn in progress for player {player.PlayerId}");
            return null;
        }

        if (_turnManager.HasRolledDice)
        {
            Debug.LogError($"Cannot roll dice: player {player.PlayerId} has already rolled this turn");
            return null;
        }

        var dieA = _random.Next(1, 7);
        var dieB = _random.Next(1, 7);
        var diceRoll = dieA + dieB;

        Debug.Log($"Player {player.PlayerId} rolled a {diceRoll} ({dieA} + {dieB})");

        // Award resources to players who have tiles with the same dice roll
        foreach (var hexTile in _hexTileMap.Values)
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
            await HandleMoveRobber();
        }

        _turnManager.SetHasRolledDice();

        BoardStateChanged?.Invoke();
        return diceRoll;
    }

    private async Task HandleSevenRollDiscard()
    {
        await _resourceManager.HandleSevenRollDiscard();
    }

    private async Task HandleMoveRobber()
    {
        await _turnManager.CurrentPlayer.MoveRobber();

        // After moving the robber, let the current player choose another player on the robber's hex to steal from
        var playersOnRobberHex = _buildingManager.GetPlayersWithBuildingsOnHexTile(_buildingManager.CurrentRobberHexTile);
        
        // Remove the current player from the list (can't steal from yourself)
        playersOnRobberHex.RemoveAll(p => p == _turnManager.CurrentPlayer);
        
        if (playersOnRobberHex.Count > 0)
        {
            Debug.Log($"Player {_turnManager.CurrentPlayer.PlayerId} can steal from {playersOnRobberHex.Count} players on the robber's hex");
            
            // Let the current player choose who to steal from
            var playerToStealFrom = await _turnManager.CurrentPlayer.ChoosePlayerToStealFrom(playersOnRobberHex);
            
            if (playerToStealFrom != null)
            {
                // Steal a random resource from the chosen player
                var stolenResource = StealRandomResourceFromPlayer(playerToStealFrom, _turnManager.CurrentPlayer);
                if (stolenResource.HasValue)
                {
                    Debug.Log($"Player {_turnManager.CurrentPlayer.PlayerId} successfully stole 1 {stolenResource.Value} from Player {playerToStealFrom.PlayerId}");
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
        _resourceManager.GiveAllPlayersResourcesForHexTile(hexTile, _buildingManager.CurrentRobberHexTile);
    }

    #endregion

    #region Unity lifecycle

    private void Update()
    {
        if (_boardStateMachine == null)
        {
            return;
        }

        _boardStateMachine.Update();
    }

    #endregion

    #region State machine

    private void OnEnterIdleMode()
    {
        foreach (var hexVertex in _vertexMap.Values)
        {
            hexVertex.EnableSelection(false);
        }
        foreach (var hexEdge in _edgeMap.Values)
        {
            hexEdge.EnableSelection(false);
        }
        foreach (var hexTile in _hexTileMap.Values)
        {
            hexTile.EnableSelection(false);
        }
    }

    private void OnEnterSettlementLocationSelectionMode()
    {
        var playerColor = _turnManager.CurrentPlayer.PlayerColor;

        var settlementLocationList = GetAvailableSettlementLocations(_turnManager.CurrentPlayer);
        var settlementLocationsSet = new HashSet<HexVertex>(settlementLocationList);

        foreach (var hexVertex in _vertexMap.Values)
        {
            var selectionEnabled = settlementLocationsSet.Contains(hexVertex);
            hexVertex.EnableSelection(selectionEnabled, playerColor);
        }
    }

    private void OnExitSettlementLocationSelectionMode()
    {
        foreach (var hexVertex in _vertexMap.Values)
        {
            hexVertex.EnableSelection(false);
        }
    }

    private void OnEnterRoadLocationSelectionMode()
    {
        var playerColor = _turnManager.CurrentPlayer.PlayerColor;

        var roadLocationList = GetAvailableRoadLocations(_turnManager.CurrentPlayer);
        var roadLocationSet = new HashSet<HexEdge>(roadLocationList);

        foreach (var hexEdge in _edgeMap.Values)
        {
            var selectionEnabled = roadLocationSet.Contains(hexEdge);
            hexEdge.EnableSelection(selectionEnabled, playerColor);
        }
    }

    private void OnExitRoadLocationSelectionMode()
    {
        foreach (var hexEdge in _edgeMap.Values)
        {
            hexEdge.EnableSelection(false);
        }
    }

    private void OnEnterSettlementUpgradeSelectionMode()
    {
        var playerColor = _turnManager.CurrentPlayer.PlayerColor;

        var settlementUpgradeList = GetAvailableSettlementsToUpgrade(_turnManager.CurrentPlayer);
        var settlementUpgradeSet = new HashSet<HexVertex>(settlementUpgradeList);

        foreach (var hexVertex in _vertexMap.Values)
        {
            var selectionEnabled = settlementUpgradeSet.Contains(hexVertex);
            hexVertex.EnableSelection(selectionEnabled, playerColor);
        }
    }

    private void OnExitSettlementUpgradeSelectionMode()
    {
        foreach (var hexVertex in _vertexMap.Values)
        {
            hexVertex.EnableSelection(false);
        }
    }

    private void OnEnterRobberLocationSelectionMode()
    {
        var playerColor = _turnManager.CurrentPlayer.PlayerColor;

        var robberLocationList = GetAvailableRobberLocations(_turnManager.CurrentPlayer);
        var robberLocationSet = new HashSet<HexTile>(robberLocationList);

        foreach (var hexTile in _hexTileMap.Values)
        {
            var selectionEnabled = robberLocationSet.Contains(hexTile);
            hexTile.EnableSelection(selectionEnabled, playerColor);
        }
    }

    private void OnExitRobberLocationSelectionMode()
    {
        foreach (var hexTile in _hexTileMap.Values)
        {
            hexTile.EnableSelection(false);
        }
    }

    #endregion

    private void ClearBoard()
    {
        _turnManager.Clear();
        _gridInitializer?.ClearBoard(_hexTileMap, _vertexMap, _edgeMap);
    }

    #region Debugging

    // Returns a string with the current resource hands of each player for debugging
    public string GetAllPlayerResourceHandsDebugString()
    {
        return _resourceManager.GetAllPlayerResourceHandsDebugString();
    }

    #endregion
}
