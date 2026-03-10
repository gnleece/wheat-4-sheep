using Grid;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IPlayer
{
    public int PlayerId { get; }

    public Color PlayerColor { get; }

    public bool IsHuman { get; }

    public void Initialize(int playerId, IBoardManager boardManager);

    public Task PlaceFirstSettlementAndRoadAsync();

    public Task PlaceSecondSettlementAndRoadAsync();

    public Task PlayTurnAsync();
    
    public Task DiscardOnSevenRoll(ResourceHand hand, int cardsToDiscard);
    public Task MoveRobber();
    public Task<IPlayer> ChoosePlayerToStealFrom(List<IPlayer> availablePlayers);
    public Task<bool> ConsiderTradeOffer(TradeOffer offer);
}
