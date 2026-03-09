using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUIManager
{
    void Initialize(IBoardManager boardManager);
    void InitializePlayerPanels(IReadOnlyList<IPlayer> players);
    void SetActivePlayer(int playerId);
    void UpdatePlayerPanels();
    void ShowSetupScreen();
    void HideSetupScreen();
    void ShowBoardConfirmationScreen();
    void HideBoardConfirmationScreen();
    void ShowGameplayUI();
    void ShowGameOverScreen(IPlayer winner, int score);
    void HideGameOverScreen();
    Task ShowDiscardUI(IPlayer player, ResourceHand hand, int cardsToDiscard);
    Task<IPlayer> ShowPlayerSelectionUI(IPlayer currentPlayer, List<IPlayer> availablePlayers);
    Task<DevelopmentCardType> ShowDevCardSelectionUI(IPlayer player, Dictionary<DevelopmentCardType, int> hand);
    Task<ResourceType> ShowResourceTypeSelectionUI(IPlayer player, string prompt);
}
