using Grid;
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

    public Task<HexVertex> GetManualSelectionForSettlementLocation(IPlayer player);

    public Task<HexEdge> GetManualSelectionForRoadLocation(IPlayer player);

    public void ManualSettlementLocationSelected(HexVertex hexVertex);

    public void ManualRoadLocationSelected(HexEdge hexEdge);

    public bool BuildSettlement(IPlayer player, HexVertex hexVertex);

    public bool BuildRoad(IPlayer player, HexEdge hexEdge);

    public Task<int?> RollDice(IPlayer player);

    public Dictionary<ResourceType, int> GetResourceHandForPlayer(IPlayer player);

    public int GetPlayerScore(IPlayer player);

    public IReadOnlyDictionary<HexCoord, HexTile> HexMap { get; }
    public IReadOnlyDictionary<VertexCoord, HexVertex> VertexMap { get; }
    public IReadOnlyDictionary<EdgeCoord, HexEdge> EdgeMap { get; }
}
