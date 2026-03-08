using UnityEngine;
using UnityEngine.UI;

internal readonly struct UIReferences
{
    internal readonly Canvas MainCanvas;
    internal readonly GameObject SetupScreen;
    internal readonly GameObject BoardConfirmationScreen;
    internal readonly GameObject GameOverScreen;
    internal readonly GameObject DiscardScreen;
    internal readonly GameObject PlayerSelectionScreen;
    internal readonly GameObject ActionPanel;
    internal readonly GameObject PlayerPanelsContainer;
    internal readonly GameObject PlayerPanelPrefab;
    internal readonly Button RollDiceButton;
    internal readonly Button BuildRoadButton;
    internal readonly Button BuildSettlementButton;
    internal readonly Button BuildCityButton;
    internal readonly Button BuyDevelopmentCardButton;
    internal readonly Button TradeButton;
    internal readonly Button EndTurnButton;

    internal UIReferences(
        Canvas mainCanvas,
        GameObject setupScreen,
        GameObject boardConfirmationScreen,
        GameObject gameOverScreen,
        GameObject discardScreen,
        GameObject playerSelectionScreen,
        GameObject actionPanel,
        GameObject playerPanelsContainer,
        GameObject playerPanelPrefab,
        Button rollDiceButton,
        Button buildRoadButton,
        Button buildSettlementButton,
        Button buildCityButton,
        Button buyDevelopmentCardButton,
        Button tradeButton,
        Button endTurnButton)
    {
        MainCanvas = mainCanvas;
        SetupScreen = setupScreen;
        BoardConfirmationScreen = boardConfirmationScreen;
        GameOverScreen = gameOverScreen;
        DiscardScreen = discardScreen;
        PlayerSelectionScreen = playerSelectionScreen;
        ActionPanel = actionPanel;
        PlayerPanelsContainer = playerPanelsContainer;
        PlayerPanelPrefab = playerPanelPrefab;
        RollDiceButton = rollDiceButton;
        BuildRoadButton = buildRoadButton;
        BuildSettlementButton = buildSettlementButton;
        BuildCityButton = buildCityButton;
        BuyDevelopmentCardButton = buyDevelopmentCardButton;
        TradeButton = tradeButton;
        EndTurnButton = endTurnButton;
    }
}
