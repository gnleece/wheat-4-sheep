using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum BoardMode
{
    Idle,
    ChooseSettlementLocation,
    ChooseRoadLocation,
    ChooseSettlementToUpgrade,
    ChooseRobberLocation,
}

public enum PlayerAction
{
    None,
    RollDice,
    BuildSettlement,
    BuildRoad,
    UpgradeSettlementToCity,
    MoveRobber,
    Trade,
    PlayDevelopmentCard,
}

public enum PlayerTurnType
{
    InitialPlacement,
    RegularTurn
}


public interface IBoardManager
{
    public bool BeginPlayerTurn(IPlayer player, PlayerTurnType turnType);

    public bool EndPlayerTurn(IPlayer player);
    public bool IsPlayerTurn(IPlayer player);
    public bool CanEndTurn(IPlayer player);

    public Task<HexVertex> GetManualSelectionForSettlementLocation(IPlayer player);

    public Task<HexEdge> GetManualSelectionForRoadLocation(IPlayer player);

    public Task<HexVertex> GetManualSelectionForSettlementUpgrade(IPlayer player);

    public void ManualSettlementLocationSelected(HexVertex hexVertex);

    public void ManualRoadLocationSelected(HexEdge hexEdge);

    public void ManualSettlementUpgradeLocationSelected(HexVertex hexVertex);

    public void ManualVertexSelected(HexVertex hexVertex);

    public bool BuildSettlement(IPlayer player, HexVertex hexVertex);

    public bool BuildRoad(IPlayer player, HexEdge hexEdge);

    public bool UpgradeSettlementToCity(IPlayer player, HexVertex hexVertex);

    public Task<int?> RollDice(IPlayer player);

    public Dictionary<ResourceType, int> GetResourceHandForPlayer(IPlayer player);

    public int GetPlayerScore(IPlayer player);

    public List<HexVertex> GetAvailableSettlementLocations(IPlayer player);

    public List<HexEdge> GetAvailableRoadLocations(IPlayer player);

    public bool CanBuildSettlement(IPlayer player);
    public bool CanBuildRoad(IPlayer player);
    public bool CanUpgradeSettlement(IPlayer player);
    public bool CanRollDice(IPlayer player);

    public IReadOnlyDictionary<HexCoord, HexTile> HexMap { get; }
    public IReadOnlyDictionary<VertexCoord, HexVertex> VertexMap { get; }
    public IReadOnlyDictionary<EdgeCoord, HexEdge> EdgeMap { get; }

    public Action BoardStateChanged { get; set; }
}
