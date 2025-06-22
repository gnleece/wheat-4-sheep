using System;

public interface IGameManager
{
    GameManager.GameState CurrentGameState { get; }
    
    bool SettlementsMustConnectToRoad { get; }
}
