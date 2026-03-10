using Grid;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Async UI-driven selections. These methods suspend until a human (or AI) has made
/// a choice, and resolve locally even in a networked game — they don't produce
/// authoritative game-state changes by themselves.
/// </summary>
public interface IBoardSelection
{
    public Task<VertexCoord> GetManualSelectionForSettlementLocation(IPlayer player);
    public Task<EdgeCoord> GetManualSelectionForRoadLocation(IPlayer player);
    public Task<VertexCoord> GetManualSelectionForSettlementUpgrade(IPlayer player);
    public Task<HexCoord> GetManualSelectionForRobberLocation(IPlayer player);
    public Task GetManualDiscardOnSevenRoll(IPlayer player, ResourceHand hand, int cardsToDiscard);
    public Task<IPlayer> GetManualSelectionForPlayerToStealFrom(IPlayer currentPlayer, List<IPlayer> availablePlayers);
    public Task<DevelopmentCardType> GetManualDevCardSelection(IPlayer player);
    public Task<ResourceType> GetManualResourceTypeSelection(IPlayer player, string prompt);
    public Task GetManualTradeSelection(IPlayer player);
    public Task<bool> GetManualTradeOfferResponse(IPlayer player, TradeOffer offer);

    public void CompleteSelection(object selection);
}
