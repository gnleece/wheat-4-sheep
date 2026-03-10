using Grid;
using System;
using System.Collections.Generic;

/// <summary>
/// Read-only queries against board state. Calls here observe state without changing it
/// and, for a networked implementation, can be answered locally on any client.
/// </summary>
public interface IBoardQuery
{
    public bool IsPlayerTurn(IPlayer player);
    public bool CanEndTurn(IPlayer player);

    public Dictionary<ResourceType, int> GetResourceHandForPlayer(IPlayer player);
    public int GetPlayerScore(IPlayer player);

    public List<VertexCoord> GetAvailableSettlementLocations(IPlayer player);
    public List<EdgeCoord> GetAvailableRoadLocations(IPlayer player);
    public List<HexCoord> GetAvailableRobberLocations(IPlayer player);
    public List<IPlayer> GetPlayersWithBuildingsOnHexTile(HexCoord hexCoord);

    public bool CanBuildSettlement(IPlayer player);
    public bool CanBuildRoad(IPlayer player);
    public bool CanUpgradeSettlement(IPlayer player);
    public bool CanRollDice(IPlayer player);

    public bool CanBuyDevelopmentCard(IPlayer player);
    public Dictionary<DevelopmentCardType, int> GetDevCardHandForPlayer(IPlayer player);
    public bool CanPlayAnyDevCard(IPlayer player);

    public bool CanInitiateTrade(IPlayer player);
    public int GetBankTradeRate(IPlayer player, ResourceType resourceType);
    public bool CanBankTrade(IPlayer player, ResourceType giving, ResourceType receiving);

    public IReadOnlyDictionary<HexCoord, HexTile> HexMap { get; }
    public IReadOnlyDictionary<VertexCoord, HexVertex> VertexMap { get; }
    public IReadOnlyDictionary<EdgeCoord, HexEdge> EdgeMap { get; }

    public Action BoardStateChanged { get; set; }
}
