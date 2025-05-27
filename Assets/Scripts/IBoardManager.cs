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

public interface IBoardManager
{
    public Task<bool> ClaimBoardForPlayerActionAsync(IPlayer player, BoardMode mode);

    public bool TrySelectSettlementLocation(HexVertex hexVertex);

    public bool TrySelectRoadLocation(HexEdge hexEdge);

    public int? GetCurrentPlayerId();
}
