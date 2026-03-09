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

    public Task<HexTile> GetManualSelectionForRobberLocation(IPlayer player);

    public Task GetManualDiscardOnSevenRoll(IPlayer player, ResourceHand hand, int cardsToDiscard);

    public Task<IPlayer> GetManualSelectionForPlayerToStealFrom(IPlayer currentPlayer, List<IPlayer> availablePlayers);

    public void CompleteSelection(object selection);

    public bool BuildSettlement(IPlayer player, HexVertex hexVertex);

    public bool BuildRoad(IPlayer player, HexEdge hexEdge);

    public bool UpgradeSettlementToCity(IPlayer player, HexVertex hexVertex);
    public bool MoveRobber(IPlayer player, HexTile hexTile);

    public Task<int?> RollDice(IPlayer player);

    public Dictionary<ResourceType, int> GetResourceHandForPlayer(IPlayer player);

    public int GetPlayerScore(IPlayer player);

    public List<HexVertex> GetAvailableSettlementLocations(IPlayer player);

    public List<HexEdge> GetAvailableRoadLocations(IPlayer player);

    public List<HexTile> GetAvailableRobberLocations(IPlayer player);

    public List<IPlayer> GetPlayersWithBuildingsOnHexTile(HexTile hexTile);

    public ResourceType? StealRandomResourceFromPlayer(IPlayer fromPlayer, IPlayer toPlayer);

    public bool CanBuildSettlement(IPlayer player);
    public bool CanBuildRoad(IPlayer player);
    public bool CanUpgradeSettlement(IPlayer player);
    public bool CanRollDice(IPlayer player);

    public bool CanBuyDevelopmentCard(IPlayer player);
    public DevelopmentCardType BuyDevelopmentCard(IPlayer player);
    public Dictionary<DevelopmentCardType, int> GetDevCardHandForPlayer(IPlayer player);
    public bool CanPlayAnyDevCard(IPlayer player);
    public Task PlayDevelopmentCard(IPlayer player, DevelopmentCardType cardType);
    public Task<DevelopmentCardType> GetManualDevCardSelection(IPlayer player);
    public Task<ResourceType> GetManualResourceTypeSelection(IPlayer player, string prompt);

    public IReadOnlyDictionary<HexCoord, HexTile> HexMap { get; }
    public IReadOnlyDictionary<VertexCoord, HexVertex> VertexMap { get; }
    public IReadOnlyDictionary<EdgeCoord, HexEdge> EdgeMap { get; }

    public Action BoardStateChanged { get; set; }
}
