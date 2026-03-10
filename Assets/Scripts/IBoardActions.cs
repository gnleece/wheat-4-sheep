using Grid;
using System.Threading.Tasks;

/// <summary>
/// Mutating operations on the board. Calls here change game state and, for a networked
/// implementation, must be sent to the authoritative host as commands.
/// </summary>
public interface IBoardActions
{
    public bool BeginPlayerTurn(IPlayer player, PlayerTurnType turnType);
    public bool EndPlayerTurn(IPlayer player);

    public Task<int?> RollDice(IPlayer player);

    public bool BuildSettlement(IPlayer player, VertexCoord vertexCoord);
    public bool BuildRoad(IPlayer player, EdgeCoord edgeCoord);
    public bool UpgradeSettlementToCity(IPlayer player, VertexCoord vertexCoord);
    public bool MoveRobber(IPlayer player, HexCoord hexCoord);

    public ResourceType? StealRandomResourceFromPlayer(IPlayer fromPlayer, IPlayer toPlayer);

    public DevelopmentCardType BuyDevelopmentCard(IPlayer player);
    public Task PlayDevelopmentCard(IPlayer player, DevelopmentCardType cardType);

    public void ExecuteBankTrade(IPlayer player, ResourceType giving, ResourceType receiving);
    public Task<bool> ProposePlayerTrade(IPlayer initiator, TradeOffer offer);
}
