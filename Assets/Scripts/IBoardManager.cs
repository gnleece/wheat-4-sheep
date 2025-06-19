using Grid;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum BoardMode
{
    Idle,
    BuildSettlement,
    BuildRoad,
    BuildCity,
    PlaceRobber,
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

public interface IBoardManager
{
    public Task<bool> ClaimBoardForPlayerActionAsync(IPlayer player, BoardMode mode);

    public bool TrySelectSettlementLocation(HexVertex hexVertex);

    public bool TrySelectRoadLocation(HexEdge hexEdge);

    public int? GetCurrentPlayerId();

    public bool BeginPlayerTurn(IPlayer player);

    public bool EndPlayerTurn(IPlayer player);

    public Task<int?> RollDice(IPlayer player);
}
