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

public interface IBoard
{
    public Task<bool> ClaimBoardForPlayerActionAsync(IPlayer player, BoardMode mode);

    public bool SettlementLocationSelected(HexVertex hexVertex);

    public bool RoadLocationSelected(HexEdge hexEdge);

    public int? GetCurrentPlayerId();
}
