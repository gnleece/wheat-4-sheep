using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour, IUIManager
{
    [Header("Manager References")]
    [SerializeField]
    private GameManager _gameManager;

    [SerializeField]
    private BoardManager _boardManager;

    [Header("Art Assets")]
    [SerializeField]
    private Font _defaultFont;

    [SerializeField]
    private Sprite _buttonSprite;

    [SerializeField]
    private Sprite _panelSprite;

    private Canvas _mainCanvas;
    private GameObject _setupScreen;
    private GameObject _boardConfirmationScreen;
    private GameObject _gameOverScreen;
    private GameObject _discardScreen;
    private GameObject _playerSelectionScreen;
    private GameObject _devCardSelectionScreen;
    private GameObject _resourceTypeSelectionScreen;
    private GameObject _actionPanel;
    private GameObject _playerPanelsContainer;
    private GameObject _playerPanelPrefab;
    private Button _rollDiceButton;
    private Button _buildRoadButton;
    private Button _buildSettlementButton;
    private Button _buildCityButton;
    private Button _buyDevelopmentCardButton;
    private Button _playDevelopmentCardButton;
    private Button _tradeButton;
    private Button _endTurnButton;

    private List<PlayerUIPanel> _playerUIPanels = new List<PlayerUIPanel>();
    private IPlayer _currentPlayer;
    private IBoardManager _boardManagerInterface;

    private void Awake()
    {
        var setup = new UISetup();
        UIReferences refs = setup.BuildUI(transform, _gameManager);

        _mainCanvas = refs.MainCanvas;
        _setupScreen = refs.SetupScreen;
        _boardConfirmationScreen = refs.BoardConfirmationScreen;
        _gameOverScreen = refs.GameOverScreen;
        _discardScreen = refs.DiscardScreen;
        _playerSelectionScreen = refs.PlayerSelectionScreen;
        _devCardSelectionScreen = refs.DevCardSelectionScreen;
        _resourceTypeSelectionScreen = refs.ResourceTypeSelectionScreen;
        _actionPanel = refs.ActionPanel;
        _playerPanelsContainer = refs.PlayerPanelsContainer;
        _playerPanelPrefab = refs.PlayerPanelPrefab;
        _rollDiceButton = refs.RollDiceButton;
        _buildRoadButton = refs.BuildRoadButton;
        _buildSettlementButton = refs.BuildSettlementButton;
        _buildCityButton = refs.BuildCityButton;
        _buyDevelopmentCardButton = refs.BuyDevelopmentCardButton;
        _playDevelopmentCardButton = refs.PlayDevelopmentCardButton;
        _tradeButton = refs.TradeButton;
        _endTurnButton = refs.EndTurnButton;

        Initialize(_boardManager);
        _gameManager.RegisterUIManager(this);
    }

    public void Initialize(IBoardManager boardManager)
    {
        _boardManagerInterface = boardManager;
        _boardManagerInterface.BoardStateChanged += RefreshUI;
        SetupActionButtons();
    }

    private void OnDestroy()
    {
        if (_boardManagerInterface != null)
        {
            _boardManagerInterface.BoardStateChanged -= RefreshUI;
        }

        // Clean up action button listeners
        if (_rollDiceButton != null)
        {
            _rollDiceButton.onClick.RemoveAllListeners();
        }

        if (_buildRoadButton != null)
        {
            _buildRoadButton.onClick.RemoveAllListeners();
        }

        if (_buildSettlementButton != null)
        {
            _buildSettlementButton.onClick.RemoveAllListeners();
        }

        if (_buildCityButton != null)
        {
            _buildCityButton.onClick.RemoveAllListeners();
        }

        if (_buyDevelopmentCardButton != null)
        {
            _buyDevelopmentCardButton.onClick.RemoveAllListeners();
        }

        if (_playDevelopmentCardButton != null)
        {
            _playDevelopmentCardButton.onClick.RemoveAllListeners();
        }

        if (_tradeButton != null)
        {
            _tradeButton.onClick.RemoveAllListeners();
        }

        if (_endTurnButton != null)
        {
            _endTurnButton.onClick.RemoveAllListeners();
        }
    }

    private void RefreshUI()
    {
        UpdatePlayerPanels();
        UpdateActionButtonStates();
    }

    private void SetupActionButtons()
    {
        if (_rollDiceButton != null)
        {
            _rollDiceButton.onClick.AddListener(OnRollDiceClicked);
        }

        if (_buildRoadButton != null)
        {
            _buildRoadButton.onClick.AddListener(OnBuildRoadClicked);
        }

        if (_buildSettlementButton != null)
        {
            _buildSettlementButton.onClick.AddListener(OnBuildSettlementClicked);
        }

        if (_buildCityButton != null)
        {
            _buildCityButton.onClick.AddListener(OnBuildCityClicked);
        }

        if (_buyDevelopmentCardButton != null)
        {
            _buyDevelopmentCardButton.onClick.AddListener(OnBuyDevelopmentCardClicked);
        }

        if (_playDevelopmentCardButton != null)
        {
            _playDevelopmentCardButton.onClick.AddListener(OnPlayDevelopmentCardClicked);
        }

        if (_tradeButton != null)
        {
            _tradeButton.onClick.AddListener(OnTradeClicked);
        }

        if (_endTurnButton != null)
        {
            _endTurnButton.onClick.AddListener(OnEndTurnClicked);
        }
    }

    private async void OnRollDiceClicked()
    {
        if (_currentPlayer == null || _boardManagerInterface == null) return;

        var diceRoll = await _boardManagerInterface.RollDice(_currentPlayer);
        Debug.Log($"Player {_currentPlayer.PlayerId} rolled: {diceRoll}");
    }

    private async void OnBuildRoadClicked()
    {
        if (_currentPlayer == null || _boardManagerInterface == null) return;

        var chosenRoadLocation = await _boardManagerInterface.GetManualSelectionForRoadLocation(_currentPlayer);
        bool success = _boardManagerInterface.BuildRoad(_currentPlayer, chosenRoadLocation);

        if (success)
        {
            Debug.Log($"Player {_currentPlayer.PlayerId} built a road");
        }
        else
        {
            Debug.Log($"Player {_currentPlayer.PlayerId} failed to build road");
        }
    }

    private async void OnBuildSettlementClicked()
    {
        if (_currentPlayer == null || _boardManagerInterface == null) return;

        var chosenSettlementLocation = await _boardManagerInterface.GetManualSelectionForSettlementLocation(_currentPlayer);
        bool success = _boardManagerInterface.BuildSettlement(_currentPlayer, chosenSettlementLocation);

        if (success)
        {
            Debug.Log($"Player {_currentPlayer.PlayerId} built a settlement");
        }
        else
        {
            Debug.Log($"Player {_currentPlayer.PlayerId} failed to build settlement");
        }
    }

    private async void OnBuildCityClicked()
    {
        if (_currentPlayer == null || _boardManagerInterface == null) return;

        var chosenSettlementToUpgrade = await _boardManagerInterface.GetManualSelectionForSettlementUpgrade(_currentPlayer);
        bool success = _boardManagerInterface.UpgradeSettlementToCity(_currentPlayer, chosenSettlementToUpgrade);

        if (success)
        {
            Debug.Log($"Player {_currentPlayer.PlayerId} upgraded settlement to city");
        }
        else
        {
            Debug.Log($"Player {_currentPlayer.PlayerId} failed to upgrade settlement to city");
        }
    }

    private void OnBuyDevelopmentCardClicked()
    {
        if (_currentPlayer == null || _boardManagerInterface == null) return;

        var card = _boardManagerInterface.BuyDevelopmentCard(_currentPlayer);
        Debug.Log($"Player {_currentPlayer.PlayerId} bought a {card} card.");
    }

    private async void OnPlayDevelopmentCardClicked()
    {
        if (_currentPlayer == null || _boardManagerInterface == null) return;

        var hand = _boardManagerInterface.GetDevCardHandForPlayer(_currentPlayer);
        var selectedCard = await ShowDevCardSelectionUI(_currentPlayer, hand);
        await _boardManagerInterface.PlayDevelopmentCard(_currentPlayer, selectedCard);
    }

    private void OnTradeClicked()
    {
        if (_currentPlayer == null) return;

        Debug.Log("Trade - Not yet implemented");
        // TODO: Implement trading when available in IBoardManager
    }

    private void OnEndTurnClicked()
    {
        if (_currentPlayer == null || _boardManagerInterface == null) return;

        bool success = _boardManagerInterface.EndPlayerTurn(_currentPlayer);
        if (success)
        {
            Debug.Log($"Player {_currentPlayer.PlayerId} ended their turn");
            UpdatePlayerPanels();
        }
        else
        {
            Debug.Log($"Player {_currentPlayer.PlayerId} cannot end turn yet");
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
        if (_actionPanel != null)
        {
            _actionPanel.SetActive(false);
        }

        if (_playerPanelsContainer != null)
        {
            _playerPanelsContainer.SetActive(false);
        }

        Debug.Log($"UIManager: Created {_playerUIPanels.Count} player panels (hidden until game starts)");
    }

    private void CreatePlayerPanel(IPlayer player)
    {
        if (_playerPanelPrefab == null || _playerPanelsContainer == null)
        {
            Debug.LogError("UIManager: PlayerPanelPrefab or PlayerPanelsContainer is null!");
            return;
        }

        Debug.Log($"UIManager: Creating panel for Player {player.PlayerId}");

        GameObject panelObject = Instantiate(_playerPanelPrefab, _playerPanelsContainer.transform);
        panelObject.SetActive(true); // Make sure the panel is active
        PlayerUIPanel playerPanel = panelObject.GetComponent<PlayerUIPanel>();

        if (playerPanel != null)
        {
            playerPanel.Initialize(player, _boardManagerInterface);
            _playerUIPanels.Add(playerPanel);
            Debug.Log($"UIManager: Successfully created panel for Player {player.PlayerId}");
        }
        else
        {
            Debug.LogError($"UIManager: PlayerUIPanel component not found on instantiated panel for Player {player.PlayerId}");
        }
    }

    private void ClearPlayerPanels()
    {
        foreach (var panel in _playerUIPanels)
        {
            if (panel != null && panel.gameObject != null)
            {
                Destroy(panel.gameObject);
            }
        }

        _playerUIPanels.Clear();
    }

    public void UpdatePlayerPanels()
    {
        foreach (var panel in _playerUIPanels)
        {
            if (panel != null)
            {
                panel.UpdateDisplay();
            }
        }

        // Update button states when player data changes
        UpdateActionButtonStates();
    }

    public void SetActivePlayer(int playerId)
    {
        // Find and store the current player reference
        _currentPlayer = null;
        foreach (var panel in _playerUIPanels)
        {
            if (panel != null)
            {
                bool isActive = panel.Player.PlayerId == playerId;
                panel.SetAsActivePlayer(isActive);

                if (isActive)
                {
                    _currentPlayer = panel.Player;
                }
            }
        }

        // Update button states when active player changes
        UpdateActionButtonStates();
    }

    public void ShowSetupScreen()
    {
        if (_setupScreen != null)
        {
            _setupScreen.SetActive(true);
        }
    }

    public void HideSetupScreen()
    {
        if (_setupScreen != null)
        {
            _setupScreen.SetActive(false);
        }
    }

    public void ShowBoardConfirmationScreen()
    {
        if (_boardConfirmationScreen != null)
        {
            _boardConfirmationScreen.SetActive(true);
        }
    }

    public void HideBoardConfirmationScreen()
    {
        if (_boardConfirmationScreen != null)
        {
            _boardConfirmationScreen.SetActive(false);
        }
    }

    public void ShowGameOverScreen(IPlayer winner, int score)
    {
        if (_gameOverScreen != null)
        {
            // Update the winner and score text
            UpdateGameOverText(winner, score);
            _gameOverScreen.SetActive(true);
        }
    }

    public void HideGameOverScreen()
    {
        if (_gameOverScreen != null)
        {
            _gameOverScreen.SetActive(false);
        }
    }

    public async System.Threading.Tasks.Task ShowDiscardUI(IPlayer player, ResourceHand hand, int cardsToDiscard)
    {
        if (_discardScreen == null)
        {
            Debug.LogError("Discard screen not found! Cannot show discard UI.");
            return;
        }

        // Update the discard UI with current player's resources
        UpdateDiscardUI(player, hand, cardsToDiscard);

        // Show the discard screen
        _discardScreen.SetActive(true);

        // Wait for the player to complete discarding
        var discardController = _discardScreen.GetComponent<DiscardUIController>();
        if (discardController != null)
        {
            await discardController.WaitForDiscardCompletion();
        }

        // Hide the discard screen
        _discardScreen.SetActive(false);
    }

    public async System.Threading.Tasks.Task<IPlayer> ShowPlayerSelectionUI(IPlayer currentPlayer, List<IPlayer> availablePlayers)
    {
        if (availablePlayers == null || availablePlayers.Count == 0)
        {
            Debug.LogError("No players available to steal from!");
            return null;
        }

        if (_playerSelectionScreen == null)
        {
            Debug.LogError("Player selection screen not found! Cannot show player selection UI.");
            return null;
        }

        // Update the player selection UI with current player and available players
        UpdatePlayerSelectionUI(currentPlayer, availablePlayers);

        // Show the player selection screen
        _playerSelectionScreen.SetActive(true);

        // Wait for the player to make a selection
        var selectionController = _playerSelectionScreen.GetComponent<PlayerSelectionUIController>();
        IPlayer selectedPlayer = null;
        if (selectionController != null)
        {
            selectedPlayer = await selectionController.WaitForPlayerSelection();
        }

        // Hide the player selection screen
        _playerSelectionScreen.SetActive(false);

        return selectedPlayer;
    }

    private void UpdatePlayerSelectionUI(IPlayer currentPlayer, List<IPlayer> availablePlayers)
    {
        if (_playerSelectionScreen == null) return;

        // Initialize the player selection controller
        var selectionController = _playerSelectionScreen.GetComponent<PlayerSelectionUIController>();
        if (selectionController != null)
        {
            selectionController.Initialize(currentPlayer, availablePlayers);
        }
    }

    private void UpdateDiscardUI(IPlayer player, ResourceHand hand, int cardsToDiscard)
    {
        if (_discardScreen == null) return;

        // Update player name
        Transform playerNameTransform = _discardScreen.transform.Find("Discard Content/Player Name");
        if (playerNameTransform != null)
        {
            TextMeshProUGUI playerNameText = playerNameTransform.GetComponent<TextMeshProUGUI>();
            if (playerNameText != null)
            {
                playerNameText.text = $"Player {player.PlayerId + 1}";
            }
        }

        // Update cards to discard count
        Transform cardsToDiscardTransform = _discardScreen.transform.Find("Discard Content/Cards To Discard");
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
            Transform resourceTransform = _discardScreen.transform.Find($"Discard Content/Resource Container/{resourceNames[i]} Item/{resourceNames[i]} Count");
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
        var discardController = _discardScreen.GetComponent<DiscardUIController>();
        if (discardController != null)
        {
            discardController.Initialize(player, hand, cardsToDiscard);
        }
    }

    private void UpdateGameOverText(IPlayer winner, int score)
    {
        if (_gameOverScreen == null) return;

        // Find and update winner text
        Transform winnerTextTransform = _gameOverScreen.transform.Find("Game Over Content/Winner Text");
        if (winnerTextTransform != null)
        {
            TextMeshProUGUI winnerText = winnerTextTransform.GetComponent<TextMeshProUGUI>();
            if (winnerText != null)
            {
                winnerText.text = $"Winner: Player {winner.PlayerId + 1}";
            }
        }

        // Find and update score text
        Transform scoreTextTransform = _gameOverScreen.transform.Find("Game Over Content/Score Text");
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
        if (_actionPanel != null)
        {
            _actionPanel.SetActive(true);
        }

        if (_playerPanelsContainer != null)
        {
            _playerPanelsContainer.SetActive(true);
        }

        Debug.Log("UIManager: Gameplay UI panels are now visible");

        // Update button states when UI becomes visible
        UpdateActionButtonStates();
    }

    private void UpdateActionButtonStates()
    {
        if (_currentPlayer == null || _boardManagerInterface == null)
        {
            // Disable all buttons if no current player or board manager
            SetButtonInteractable(_rollDiceButton, false);
            SetButtonInteractable(_buildRoadButton, false);
            SetButtonInteractable(_buildSettlementButton, false);
            SetButtonInteractable(_buildCityButton, false);
            SetButtonInteractable(_buyDevelopmentCardButton, false);
            SetButtonInteractable(_playDevelopmentCardButton, false);
            SetButtonInteractable(_tradeButton, false);
            SetButtonInteractable(_endTurnButton, false);
            return;
        }

        SetButtonInteractable(_rollDiceButton, _boardManagerInterface.CanRollDice(_currentPlayer));
        SetButtonInteractable(_buildRoadButton, _boardManagerInterface.CanBuildRoad(_currentPlayer));
        SetButtonInteractable(_buildSettlementButton, _boardManagerInterface.CanBuildSettlement(_currentPlayer));
        SetButtonInteractable(_buildCityButton, _boardManagerInterface.CanUpgradeSettlement(_currentPlayer));
        SetButtonInteractable(_buyDevelopmentCardButton, _boardManagerInterface.CanBuyDevelopmentCard(_currentPlayer));
        SetButtonInteractable(_playDevelopmentCardButton, _boardManagerInterface.CanPlayAnyDevCard(_currentPlayer));
        SetButtonInteractable(_tradeButton, false); // Disabled until trade is implemented

        SetButtonInteractable(_endTurnButton, _boardManagerInterface.CanEndTurn(_currentPlayer));
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

    public async System.Threading.Tasks.Task<DevelopmentCardType> ShowDevCardSelectionUI(IPlayer player, Dictionary<DevelopmentCardType, int> hand)
    {
        if (_devCardSelectionScreen == null)
        {
            Debug.LogError("Dev card selection screen not found!");
            return default;
        }

        var controller = _devCardSelectionScreen.GetComponent<DevCardSelectionUIController>();
        if (controller != null)
        {
            controller.Initialize(player, hand);
        }

        _devCardSelectionScreen.SetActive(true);
        DevelopmentCardType selected = default;
        if (controller != null)
        {
            selected = await controller.WaitForCardSelection();
        }

        _devCardSelectionScreen.SetActive(false);
        return selected;
    }

    public async System.Threading.Tasks.Task<ResourceType> ShowResourceTypeSelectionUI(IPlayer player, string prompt)
    {
        if (_resourceTypeSelectionScreen == null)
        {
            Debug.LogError("Resource type selection screen not found!");
            return ResourceType.None;
        }

        var controller = _resourceTypeSelectionScreen.GetComponent<ResourceTypeSelectionUIController>();
        if (controller != null)
        {
            controller.Initialize(player, prompt);
        }

        _resourceTypeSelectionScreen.SetActive(true);
        ResourceType selected = ResourceType.None;
        if (controller != null)
        {
            selected = await controller.WaitForResourceTypeSelection();
        }

        _resourceTypeSelectionScreen.SetActive(false);
        return selected;
    }
}
