public interface IGameManager
{
    GameManager.GameState CurrentGameState { get; }
    bool SettlementsMustConnectToRoad { get; }
    void SelectPlayerCount(int count);
    void ConfirmBoard();
    void RegenerateBoard();
    void RestartGame();
    void RegisterUIManager(IUIManager uiManager);
}
