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

public class BoardManager : MonoBehaviour, IBoardManager, IGrid
{
    #region Consts

    private const int INNER_SHUFFLEABLE_GRID_SIZE = 2;
    private const int FULL_GRID_SIZE = 3;

    #endregion

    #region Classes and structs

    private class PlayerActionRequest
    {
        public enum RequestState
        {
            NotStarted,
            InProgress,
            Complete,
            ReadyForCleanup,
            CleanupFinished
        }

        public IPlayer Player;
        public BoardMode Mode;

        public RequestState State = RequestState.NotStarted;
        public bool Success = false;

        public PlayerActionRequest(IPlayer player, BoardMode mode)
        {
            Player = player;
            Mode = mode;
        }

        public override string ToString() => $"Player {Player.PlayerId}, Mode {Mode}, State {State}";
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

    public IReadOnlyDictionary<HexCoord, HexTile> HexMap => hexMap;
    public IReadOnlyDictionary<VertexCoord, HexVertex> VertexMap => vertexMap;
    public IReadOnlyDictionary<EdgeCoord, HexEdge> EdgeMap => edgeMap;

    #endregion

    #region Private members

    private Dictionary<HexCoord, HexTile> hexMap = new Dictionary<HexCoord, HexTile>();
    private Dictionary<VertexCoord, HexVertex> vertexMap = new Dictionary<VertexCoord, HexVertex>();
    private Dictionary<EdgeCoord, HexEdge> edgeMap = new Dictionary<EdgeCoord, HexEdge>();

    private StateMachine<BoardMode> boardStateMachine;

    private PlayerActionRequest currentPlayerActionRequest = null;

    #endregion

    #region Public methods

    public void StartNewGame()
    {
        ClearBoard();
        InitializeBoard(INNER_SHUFFLEABLE_GRID_SIZE, FULL_GRID_SIZE);

        boardStateMachine = new StateMachine<BoardMode>("BoardMode");

        boardStateMachine.AddState(BoardMode.Idle, OnEnterIdleMode, null, null);
        boardStateMachine.AddState(BoardMode.BuildSettlement, OnEnterSettlementPlacementMode, null, OnExitSettlementPlacementMode);
        boardStateMachine.AddState(BoardMode.BuildRoad, OnEnterRoadPlacementMode, null, OnExitRoadPlacementMode);

        boardStateMachine.GoToState(BoardMode.Idle);
    }

    public async Task<bool> ClaimBoardForPlayerActionAsync(IPlayer player, BoardMode mode)
    {
        var actionRequest = new PlayerActionRequest(player, mode);

        if (currentPlayerActionRequest != null)
        {
            Debug.LogError($"Board manager ClaimBoardForPlayerActionAsync failed for request: {actionRequest}. Current request: {currentPlayerActionRequest}");
            return false;
        }

        if (boardStateMachine.CurrentState != BoardMode.Idle)
        {
            Debug.LogError($"Board manager has no active request but board is not in idle mode");
            return false;
        }

        // Don't transition modes right away. Instead wait to process the request on the
        // next Update, so that operations are guaranteed to happen on the main thread.
        currentPlayerActionRequest = actionRequest;

        // Wait for the request to complete
        while (currentPlayerActionRequest.State != PlayerActionRequest.RequestState.Complete)
        {
            await Task.Yield();
        }

        // Mark the request for cleanup & wait for cleanup to finish before returning
        currentPlayerActionRequest.State = PlayerActionRequest.RequestState.ReadyForCleanup;
        while (currentPlayerActionRequest.State != PlayerActionRequest.RequestState.CleanupFinished)
        {
            await Task.Yield();
        }

        var success = currentPlayerActionRequest.Success;
        currentPlayerActionRequest = null;

        return success;
    }

    public bool TrySelectSettlementLocation(HexVertex hexVertex)
    {
        if (currentPlayerActionRequest == null)
        {
            Debug.LogError("Board manager: settlement location selected but there is no active player action request");
            return false;
        }
        if (boardStateMachine.CurrentState != BoardMode.BuildSettlement)
        {
            Debug.LogError($"Board manager: settlement location selected but board is in {boardStateMachine.CurrentState} mode");
            return false;
        }

        if (hexVertex == null)
        {
            Debug.LogError("Board manager: settlement location selected but hex vertex is null");
            return false;
        }

        var success = hexVertex.TryPlaceBuilding(Building.BuildingType.Settlement, currentPlayerActionRequest.Player);
        if (success)
        {
            Debug.Log($"PLACED SETTLEMENT: {hexVertex}");
            currentPlayerActionRequest.Success = success;
            currentPlayerActionRequest.State = PlayerActionRequest.RequestState.Complete;
        }
        else
        {
            Debug.Log("Invalid settlement position. Try again.");
        }

        return success;
    }

    public bool TrySelectRoadLocation(HexEdge hexEdge)
    {
        if (currentPlayerActionRequest == null)
        {
            Debug.LogError("Board manager: road location selected but there is no active player action request");
            return false;
        }
        if (boardStateMachine.CurrentState != BoardMode.BuildRoad)
        {
            Debug.LogError($"Board manager: road location selected but board is in {boardStateMachine.CurrentState} mode");
            return false;
        }

        if (hexEdge == null)
        {
            Debug.LogError("Board manager: road location selected but hex edge is null");
            return false;
        }

        Debug.Log($"Trying to place road at {hexEdge}");

        var success = hexEdge.TryPlaceRoad(currentPlayerActionRequest.Player);
        if (success)
        {
            currentPlayerActionRequest.Success = success;
            currentPlayerActionRequest.State = PlayerActionRequest.RequestState.Complete;
        }
        else
        {
            Debug.Log("Invalid road position. Try again.");
        }

        return success;
    }

    public int? GetCurrentPlayerId()
    {
        if (currentPlayerActionRequest == null || 
            currentPlayerActionRequest.State != PlayerActionRequest.RequestState.InProgress)
        {
            return null;
        }
        return currentPlayerActionRequest.Player.PlayerId;
    }

    #endregion

    #region Unity lifecycle

    private void Update()
    {
        if (boardStateMachine == null)
        {
            return;
        }

        if (currentPlayerActionRequest != null &&
            currentPlayerActionRequest.State == PlayerActionRequest.RequestState.NotStarted)
        {
            currentPlayerActionRequest.State = PlayerActionRequest.RequestState.InProgress;
            boardStateMachine.GoToState(currentPlayerActionRequest.Mode);
        }

        if (currentPlayerActionRequest != null &&
            currentPlayerActionRequest.State == PlayerActionRequest.RequestState.ReadyForCleanup)
        {
            boardStateMachine.GoToState(BoardMode.Idle);
            currentPlayerActionRequest.State = PlayerActionRequest.RequestState.CleanupFinished;
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

    private void OnEnterSettlementPlacementMode()
    {
        var playerColor = currentPlayerActionRequest.Player.PlayerColor;
        var mustConnectToRoad = false; // TODO: Get this from game state
        foreach (var hexVertex in vertexMap.Values)
        {
            var selectionEnabled = hexVertex.AvailableForBuilding(currentPlayerActionRequest.Player, mustConnectToRoad);
            hexVertex.EnableSelection(selectionEnabled, playerColor);
        }
    }

    private void OnExitSettlementPlacementMode()
    {
        foreach (var hexVertex in vertexMap.Values)
        {
            hexVertex.EnableSelection(false);
        }
    }

    private void OnEnterRoadPlacementMode()
    {
        var playerColor = currentPlayerActionRequest.Player.PlayerColor;
        foreach (var hexEdge in edgeMap.Values)
        {
            // TODO: During initial placement, second road must connect to the second settlement
            var selectionEnabled = hexEdge.AvailableForBuilding(currentPlayerActionRequest.Player);
            hexEdge.EnableSelection(selectionEnabled, playerColor);
        }
    }

    private void OnExitRoadPlacementMode()
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
                    hexMap.Add(coord, hexTile);

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
        foreach (var hex in hexMap.Values)
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
            if (hexMap.TryGetValue(vertex.VertexCoord.HexCoord, out var hex))
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

        // Spawn edge objects
        foreach (var edge in edgeMap.Values)
        {
            if (hexMap.TryGetValue(edge.EdgeCoord.HexCoord, out var hex))
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
        currentPlayerActionRequest = null;

        foreach (var tile in hexMap.Values)
        {
            Destroy(tile.TileObject.gameObject);
        }
        hexMap.Clear();

        foreach (var vertex in vertexMap.Values)
        {
            Destroy(vertex.VertexObject.gameObject);
        }
        vertexMap.Clear();

        foreach (var edge in edgeMap.Values)
        {
            Destroy(edge.SelectionObject);
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
}
