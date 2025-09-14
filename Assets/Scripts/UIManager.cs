using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Canvas mainCanvas;
    public GameObject setupScreen;
    public GameObject boardConfirmationScreen;
    public GameObject gameOverScreen;
    public GameObject discardScreen;
    public GameObject playerSelectionScreen;
    public GameObject actionPanel;
    public GameObject playerPanelsContainer;
    public GameObject playerPanelPrefab;
    
    [Header("Action Buttons")]
    public Button rollDiceButton;
    public Button buildRoadButton;
    public Button buildSettlementButton;
    public Button buildCityButton;
    public Button buyDevelopmentCardButton;
    public Button tradeButton;
    public Button endTurnButton;
    
    private GameManager gameManager;
    private List<PlayerUIPanel> playerUIPanels = new List<PlayerUIPanel>();
    private IPlayer currentPlayer;
    private IBoardManager boardManager;
    
    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        boardManager = FindAnyObjectByType<BoardManager>();
        boardManager.BoardStateChanged += RefreshUI;
        SetupActionButtons();
    }

    private void OnDestroy()
    {
        if (boardManager != null)
            boardManager.BoardStateChanged -= RefreshUI;

        // Clean up action button listeners
        if (rollDiceButton != null)
            rollDiceButton.onClick.RemoveAllListeners();
        if (buildRoadButton != null)
            buildRoadButton.onClick.RemoveAllListeners();
        if (buildSettlementButton != null)
            buildSettlementButton.onClick.RemoveAllListeners();
        if (buildCityButton != null)
            buildCityButton.onClick.RemoveAllListeners();
        if (buyDevelopmentCardButton != null)
            buyDevelopmentCardButton.onClick.RemoveAllListeners();
        if (tradeButton != null)
            tradeButton.onClick.RemoveAllListeners();
        if (endTurnButton != null)
            endTurnButton.onClick.RemoveAllListeners();
    }

    private void RefreshUI()
    {
        UpdatePlayerPanels();
        UpdateActionButtonStates();
    }

    private void SetupActionButtons()
    {
        if (rollDiceButton != null)
            rollDiceButton.onClick.AddListener(OnRollDiceClicked);
            
        if (buildRoadButton != null)
            buildRoadButton.onClick.AddListener(OnBuildRoadClicked);
            
        if (buildSettlementButton != null)
            buildSettlementButton.onClick.AddListener(OnBuildSettlementClicked);
            
        if (buildCityButton != null)
            buildCityButton.onClick.AddListener(OnBuildCityClicked);
            
        if (buyDevelopmentCardButton != null)
            buyDevelopmentCardButton.onClick.AddListener(OnBuyDevelopmentCardClicked);
            
        if (tradeButton != null)
            tradeButton.onClick.AddListener(OnTradeClicked);
            
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }
    
    private async void OnRollDiceClicked()
    {
        if (currentPlayer == null || boardManager == null) return;
        
        var diceRoll = await boardManager.RollDice(currentPlayer);
        Debug.Log($"Player {currentPlayer.PlayerId} rolled: {diceRoll}");
    }
    
    private async void OnBuildRoadClicked()
    {
        if (currentPlayer == null || boardManager == null) return;
        
        var chosenRoadLocation = await boardManager.GetManualSelectionForRoadLocation(currentPlayer);
        bool success = boardManager.BuildRoad(currentPlayer, chosenRoadLocation);
        
        if (success)
        {
            Debug.Log($"Player {currentPlayer.PlayerId} built a road");
        }
        else
        {
            Debug.Log($"Player {currentPlayer.PlayerId} failed to build road");
        }
    }
    
    private async void OnBuildSettlementClicked()
    {
        if (currentPlayer == null || boardManager == null) return;
        
        var chosenSettlementLocation = await boardManager.GetManualSelectionForSettlementLocation(currentPlayer);
        bool success = boardManager.BuildSettlement(currentPlayer, chosenSettlementLocation);
        
        if (success)
        {
            Debug.Log($"Player {currentPlayer.PlayerId} built a settlement");
        }
        else
        {
            Debug.Log($"Player {currentPlayer.PlayerId} failed to build settlement");
        }
    }
    
    private async void OnBuildCityClicked()
    {
        if (currentPlayer == null || boardManager == null) return;
        
        var chosenSettlementToUpgrade = await boardManager.GetManualSelectionForSettlementUpgrade(currentPlayer);
        bool success = boardManager.UpgradeSettlementToCity(currentPlayer, chosenSettlementToUpgrade);
        
        if (success)
        {
            Debug.Log($"Player {currentPlayer.PlayerId} upgraded settlement to city");
        }
        else
        {
            Debug.Log($"Player {currentPlayer.PlayerId} failed to upgrade settlement to city");
        }
    }
    
    private void OnBuyDevelopmentCardClicked()
    {
        if (currentPlayer == null) return;
        
        Debug.Log("Buy Development Card - Not yet implemented");
        // TODO: Implement development card purchase when available in IBoardManager
    }
    
    private void OnTradeClicked()
    {
        if (currentPlayer == null) return;
        
        Debug.Log("Trade - Not yet implemented");
        // TODO: Implement trading when available in IBoardManager
    }
    
    private void OnEndTurnClicked()
    {
        if (currentPlayer == null || boardManager == null) return;
        
        bool success = boardManager.EndPlayerTurn(currentPlayer);
        if (success)
        {
            Debug.Log($"Player {currentPlayer.PlayerId} ended their turn");
            UpdatePlayerPanels();
        }
        else
        {
            Debug.Log($"Player {currentPlayer.PlayerId} cannot end turn yet");
        }
    }
    
    public void InitializePlayerPanels(IReadOnlyList<IPlayer> players)
    {
        Debug.Log($"UIManager: InitializePlayerPanels called with {players.Count} players");
        ClearPlayerPanels();
        
        foreach (var player in players)
        {
            CreatePlayerPanel(player);
        }
        
        // Keep UI panels hidden until game actually starts (after initial placement)
        if (actionPanel != null)
            actionPanel.SetActive(false);
            
        if (playerPanelsContainer != null)
            playerPanelsContainer.SetActive(false);
            
        Debug.Log($"UIManager: Created {playerUIPanels.Count} player panels (hidden until game starts)");
    }
    
    private void CreatePlayerPanel(IPlayer player)
    {
        if (playerPanelPrefab == null || playerPanelsContainer == null) 
        {
            Debug.LogError("UIManager: PlayerPanelPrefab or PlayerPanelsContainer is null!");
            return;
        }
        
        Debug.Log($"UIManager: Creating panel for Player {player.PlayerId}");
        
        GameObject panelObject = Instantiate(playerPanelPrefab, playerPanelsContainer.transform);
        panelObject.SetActive(true); // Make sure the panel is active
        PlayerUIPanel playerPanel = panelObject.GetComponent<PlayerUIPanel>();
        
        if (playerPanel != null)
        {
            playerPanel.Initialize(player);
            playerUIPanels.Add(playerPanel);
            Debug.Log($"UIManager: Successfully created panel for Player {player.PlayerId}");
        }
        else
        {
            Debug.LogError($"UIManager: PlayerUIPanel component not found on instantiated panel for Player {player.PlayerId}");
        }
    }
    
    private void ClearPlayerPanels()
    {
        foreach (var panel in playerUIPanels)
        {
            if (panel != null && panel.gameObject != null)
                Destroy(panel.gameObject);
        }
        playerUIPanels.Clear();
    }
    
    public void UpdatePlayerPanels()
    {
        foreach (var panel in playerUIPanels)
        {
            if (panel != null)
                panel.UpdateDisplay();
        }
        
        // Update button states when player data changes
        UpdateActionButtonStates();
    }
    
    public void SetActivePlayer(int playerId)
    {
        // Find and store the current player reference
        currentPlayer = null;
        foreach (var panel in playerUIPanels)
        {
            if (panel != null)
            {
                bool isActive = panel.Player.PlayerId == playerId;
                panel.SetAsActivePlayer(isActive);
                
                if (isActive)
                {
                    currentPlayer = panel.Player;
                }
            }
        }
        
        // Update button states when active player changes
        UpdateActionButtonStates();
    }
    
    public void ShowSetupScreen()
    {
        if (setupScreen != null)
            setupScreen.SetActive(true);
    }
    
    public void HideSetupScreen()
    {
        if (setupScreen != null)
            setupScreen.SetActive(false);
    }
    
    public void ShowBoardConfirmationScreen()
    {
        if (boardConfirmationScreen != null)
            boardConfirmationScreen.SetActive(true);
    }
    
    public void HideBoardConfirmationScreen()
    {
        if (boardConfirmationScreen != null)
            boardConfirmationScreen.SetActive(false);
    }
    
    public void ShowGameOverScreen(IPlayer winner, int score)
    {
        if (gameOverScreen != null)
        {
            // Update the winner and score text
            UpdateGameOverText(winner, score);
            gameOverScreen.SetActive(true);
        }
    }
    
    public void HideGameOverScreen()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
    }
    
    public async System.Threading.Tasks.Task ShowDiscardUI(IPlayer player, ResourceHand hand, int cardsToDiscard)
    {
        if (discardScreen == null)
        {
            Debug.LogError("Discard screen not found! Cannot show discard UI.");
            return;
        }
        
        // Update the discard UI with current player's resources
        UpdateDiscardUI(player, hand, cardsToDiscard);
        
        // Show the discard screen
        discardScreen.SetActive(true);
        
        // Wait for the player to complete discarding
        var discardController = discardScreen.GetComponent<DiscardUIController>();
        if (discardController != null)
        {
            await discardController.WaitForDiscardCompletion();
        }
        
        // Hide the discard screen
        discardScreen.SetActive(false);
    }

    public async System.Threading.Tasks.Task<IPlayer> ShowPlayerSelectionUI(IPlayer currentPlayer, List<IPlayer> availablePlayers)
    {
        if (availablePlayers == null || availablePlayers.Count == 0)
        {
            Debug.LogError("No players available to steal from!");
            return null;
        }

        if (playerSelectionScreen == null)
        {
            Debug.LogError("Player selection screen not found! Cannot show player selection UI.");
            return null;
        }
        
        // Update the player selection UI with current player and available players
        UpdatePlayerSelectionUI(currentPlayer, availablePlayers);
        
        // Show the player selection screen
        playerSelectionScreen.SetActive(true);
        
        // Wait for the player to make a selection
        var selectionController = playerSelectionScreen.GetComponent<PlayerSelectionUIController>();
        IPlayer selectedPlayer = null;
        if (selectionController != null)
        {
            selectedPlayer = await selectionController.WaitForPlayerSelection();
        }
        
        // Hide the player selection screen
        playerSelectionScreen.SetActive(false);
        
        return selectedPlayer;
    }
    
    private void UpdatePlayerSelectionUI(IPlayer currentPlayer, List<IPlayer> availablePlayers)
    {
        if (playerSelectionScreen == null) return;
        
        // Initialize the player selection controller
        var selectionController = playerSelectionScreen.GetComponent<PlayerSelectionUIController>();
        if (selectionController != null)
        {
            selectionController.Initialize(currentPlayer, availablePlayers);
        }
    }
    
    private void UpdateDiscardUI(IPlayer player, ResourceHand hand, int cardsToDiscard)
    {
        if (discardScreen == null) return;
        
        // Update player name
        Transform playerNameTransform = discardScreen.transform.Find("Discard Content/Player Name");
        if (playerNameTransform != null)
        {
            TextMeshProUGUI playerNameText = playerNameTransform.GetComponent<TextMeshProUGUI>();
            if (playerNameText != null)
            {
                playerNameText.text = $"Player {player.PlayerId + 1}";
            }
        }
        
        // Update cards to discard count
        Transform cardsToDiscardTransform = discardScreen.transform.Find("Discard Content/Cards To Discard");
        if (cardsToDiscardTransform != null)
        {
            TextMeshProUGUI cardsToDiscardText = cardsToDiscardTransform.GetComponent<TextMeshProUGUI>();
            if (cardsToDiscardText != null)
            {
                cardsToDiscardText.text = $"You must discard {cardsToDiscard} cards";
            }
        }
        
        // Update resource counts
        var allResources = hand.GetAll();
        string[] resourceNames = { "Wood", "Clay", "Sheep", "Wheat", "Ore" };
        ResourceType[] resourceTypes = { ResourceType.Wood, ResourceType.Clay, ResourceType.Sheep, ResourceType.Wheat, ResourceType.Ore };
        
        for (int i = 0; i < resourceNames.Length; i++)
        {
            Transform resourceTransform = discardScreen.transform.Find($"Discard Content/Resource Container/{resourceNames[i]} Item/{resourceNames[i]} Count");
            if (resourceTransform != null)
            {
                TextMeshProUGUI resourceText = resourceTransform.GetComponent<TextMeshProUGUI>();
                if (resourceText != null)
                {
                    int count = allResources.ContainsKey(resourceTypes[i]) ? allResources[resourceTypes[i]] : 0;
                    resourceText.text = count.ToString();
                }
            }
        }
        
        // Initialize the discard controller
        var discardController = discardScreen.GetComponent<DiscardUIController>();
        if (discardController != null)
        {
            discardController.Initialize(player, hand, cardsToDiscard);
        }
    }
    
    private void UpdateGameOverText(IPlayer winner, int score)
    {
        if (gameOverScreen == null) return;
        
        // Find and update winner text
        Transform winnerTextTransform = gameOverScreen.transform.Find("Game Over Content/Winner Text");
        if (winnerTextTransform != null)
        {
            TextMeshProUGUI winnerText = winnerTextTransform.GetComponent<TextMeshProUGUI>();
            if (winnerText != null)
            {
                winnerText.text = $"Winner: Player {winner.PlayerId + 1}";
            }
        }
        
        // Find and update score text
        Transform scoreTextTransform = gameOverScreen.transform.Find("Game Over Content/Score Text");
        if (scoreTextTransform != null)
        {
            TextMeshProUGUI scoreText = scoreTextTransform.GetComponent<TextMeshProUGUI>();
            if (scoreText != null)
            {
                scoreText.text = $"Victory Points: {score}";
            }
        }
    }
    
    public void ShowGameplayUI()
    {
        if (actionPanel != null)
            actionPanel.SetActive(true);
            
        if (playerPanelsContainer != null)
            playerPanelsContainer.SetActive(true);
            
        Debug.Log("UIManager: Gameplay UI panels are now visible");
        
        // Update button states when UI becomes visible
        UpdateActionButtonStates();
    }
    
    private void UpdateActionButtonStates()
    {
        if (currentPlayer == null || boardManager == null)
        {
            // Disable all buttons if no current player or board manager
            SetButtonInteractable(rollDiceButton, false);
            SetButtonInteractable(buildRoadButton, false);
            SetButtonInteractable(buildSettlementButton, false);
            SetButtonInteractable(buildCityButton, false);
            SetButtonInteractable(buyDevelopmentCardButton, false);
            SetButtonInteractable(tradeButton, false);
            SetButtonInteractable(endTurnButton, false);
            return;
        }
        
        SetButtonInteractable(rollDiceButton, boardManager.CanRollDice(currentPlayer));
        SetButtonInteractable(buildRoadButton, boardManager.CanBuildRoad(currentPlayer));
        SetButtonInteractable(buildSettlementButton, boardManager.CanBuildSettlement(currentPlayer));
        SetButtonInteractable(buildCityButton, boardManager.CanUpgradeSettlement(currentPlayer));
        SetButtonInteractable(buyDevelopmentCardButton, false); // Disabled until dev cards are implemented
        SetButtonInteractable(tradeButton, false);              // Disabled until trade is implemented
        
        SetButtonInteractable(endTurnButton, boardManager.CanEndTurn(currentPlayer));
    }
    
    private void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
            
            // Visual feedback - dim disabled buttons
            var colors = button.colors;
            if (interactable)
            {
                colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            }
            else
            {
                colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            }
            button.colors = colors;
        }
    }
}