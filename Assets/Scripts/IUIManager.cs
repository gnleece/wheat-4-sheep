using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUIManager
{
    Task ShowDiscardUI(IPlayer player, ResourceHand hand, int cardsToDiscard);
    Task<IPlayer> ShowPlayerSelectionUI(IPlayer currentPlayer, List<IPlayer> availablePlayers);
}
