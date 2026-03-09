using UnityEngine;
using UnityEngine.UI;
using TMPro;

internal sealed class UISetup
{
    private Transform _parent;
    private IGameManager _gameManager;
    private Canvas _mainCanvas;

    internal UIReferences BuildUI(Transform parent, IGameManager gameManager)
    {
        _parent = parent;
        _gameManager = gameManager;

        Debug.Log("Creating main UI...");
        CreateCanvas();
        CreateSetupScreen();
        CreateBoardConfirmationScreen();
        CreateGameOverScreen();
        CreateDiscardScreen();
        CreatePlayerSelectionScreen();
        CreateDevCardSelectionScreen();
        CreateResourceTypeSelectionScreen();
        CreateActionPanel();
        CreatePlayerPanelsArea();
        Debug.Log("Main UI creation completed");

        return CollectReferences();
    }

    private UIReferences CollectReferences()
    {
        Transform canvasTransform = _mainCanvas.transform;

        GameObject actionPanelObj = canvasTransform.Find("Action Panel").gameObject;
        GameObject playerPanelsContainerObj = canvasTransform.Find("Player Panels Container").gameObject;

        actionPanelObj.SetActive(false);
        playerPanelsContainerObj.SetActive(false);

        Transform actionPanelTransform = actionPanelObj.transform;

        return new UIReferences(
            mainCanvas: _mainCanvas,
            setupScreen: canvasTransform.Find("Setup Screen").gameObject,
            boardConfirmationScreen: canvasTransform.Find("Board Confirmation Screen").gameObject,
            gameOverScreen: canvasTransform.Find("Game Over Screen").gameObject,
            discardScreen: canvasTransform.Find("Discard Screen").gameObject,
            playerSelectionScreen: canvasTransform.Find("Player Selection Screen").gameObject,
            devCardSelectionScreen: canvasTransform.Find("Dev Card Selection Screen").gameObject,
            resourceTypeSelectionScreen: canvasTransform.Find("Resource Type Selection Screen").gameObject,
            actionPanel: actionPanelObj,
            playerPanelsContainer: playerPanelsContainerObj,
            playerPanelPrefab: playerPanelsContainerObj.transform.Find("Player Panel Prefab").gameObject,
            rollDiceButton: actionPanelTransform.Find("Roll Dice")?.GetComponent<Button>(),
            buildRoadButton: actionPanelTransform.Find("Build Road")?.GetComponent<Button>(),
            buildSettlementButton: actionPanelTransform.Find("Build Settlement")?.GetComponent<Button>(),
            buildCityButton: actionPanelTransform.Find("Build City")?.GetComponent<Button>(),
            buyDevelopmentCardButton: actionPanelTransform.Find("Buy Dev Card")?.GetComponent<Button>(),
            playDevelopmentCardButton: actionPanelTransform.Find("Play Dev Card")?.GetComponent<Button>(),
            tradeButton: actionPanelTransform.Find("Trade")?.GetComponent<Button>(),
            endTurnButton: actionPanelTransform.Find("End Turn")?.GetComponent<Button>()
        );
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Main UI Canvas");
        canvasObject.transform.SetParent(_parent);

        _mainCanvas = canvasObject.AddComponent<Canvas>();
        _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _mainCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
    }

    private void CreateSetupScreen()
    {
        Debug.Log("Creating setup screen...");
        GameObject setupScreen = CreatePanel("Setup Screen", _mainCanvas.transform);
        RectTransform setupRect = setupScreen.GetComponent<RectTransform>();

        // Full screen setup panel
        setupRect.anchorMin = Vector2.zero;
        setupRect.anchorMax = Vector2.one;
        setupRect.anchoredPosition = Vector2.zero;
        setupRect.sizeDelta = Vector2.zero;

        // Semi-transparent background
        Image setupImage = setupScreen.GetComponent<Image>();
        setupImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // Create main container for content
        GameObject contentContainer = new GameObject("Setup Content");
        contentContainer.transform.SetParent(setupScreen.transform);
        RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.25f, 0.25f);
        contentRect.anchorMax = new Vector2(0.75f, 0.75f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        // Title
        GameObject titleText = CreateText("Game Setup Title", contentContainer.transform, "Select Number of Players");
        TextMeshProUGUI titleComponent = titleText.GetComponent<TextMeshProUGUI>();
        titleComponent.fontSize = 36;
        titleComponent.fontStyle = FontStyles.Bold;
        titleComponent.alignment = TextAlignmentOptions.Center;
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.9f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = Vector2.zero;

        // Button container
        GameObject buttonContainer = new GameObject("Button Container");
        buttonContainer.transform.SetParent(contentContainer.transform);
        RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
        buttonContainerRect.anchorMin = new Vector2(0.2f, 0.3f);
        buttonContainerRect.anchorMax = new Vector2(0.8f, 0.6f);
        buttonContainerRect.anchoredPosition = Vector2.zero;
        buttonContainerRect.sizeDelta = Vector2.zero;

        // Add layout group for buttons
        HorizontalLayoutGroup layoutGroup = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 50f;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = true;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;

        // Create 3 Player button
        GameObject threePlayerButton = CreateSetupButton("3 Players", buttonContainer.transform);
        Button threePlayerButtonComponent = threePlayerButton.GetComponent<Button>();
        threePlayerButtonComponent.onClick.AddListener(() => {
            Debug.Log("3 Players selected");
            _gameManager.SelectPlayerCount(3);
            setupScreen.SetActive(false);
        });

        // Create 4 Player button
        GameObject fourPlayerButton = CreateSetupButton("4 Players", buttonContainer.transform);
        Button fourPlayerButtonComponent = fourPlayerButton.GetComponent<Button>();
        fourPlayerButtonComponent.onClick.AddListener(() => {
            Debug.Log("4 Players selected");
            _gameManager.SelectPlayerCount(4);
            setupScreen.SetActive(false);
        });

        Debug.Log("Setup screen created successfully");
    }

    private void CreateBoardConfirmationScreen()
    {
        Debug.Log("Creating board confirmation screen...");
        GameObject boardConfirmScreen = CreatePanel("Board Confirmation Screen", _mainCanvas.transform);
        RectTransform boardConfirmRect = boardConfirmScreen.GetComponent<RectTransform>();

        // Full screen confirmation panel
        boardConfirmRect.anchorMin = Vector2.zero;
        boardConfirmRect.anchorMax = Vector2.one;
        boardConfirmRect.anchoredPosition = Vector2.zero;
        boardConfirmRect.sizeDelta = Vector2.zero;

        // Semi-transparent background
        Image boardConfirmImage = boardConfirmScreen.GetComponent<Image>();
        boardConfirmImage.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

        // Create main container for content
        GameObject contentContainer = new GameObject("Board Confirmation Content");
        contentContainer.transform.SetParent(boardConfirmScreen.transform);
        RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.25f, 0.25f);
        contentRect.anchorMax = new Vector2(0.75f, 0.75f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        // Title
        GameObject titleText = CreateText("Board Confirmation Title", contentContainer.transform, "Board Layout");
        TextMeshProUGUI titleComponent = titleText.GetComponent<TextMeshProUGUI>();
        titleComponent.fontSize = 32;
        titleComponent.fontStyle = FontStyles.Bold;
        titleComponent.alignment = TextAlignmentOptions.Center;
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.85f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = Vector2.zero;

        // Description text
        GameObject descText = CreateText("Board Confirmation Description", contentContainer.transform, "Are you satisfied with this board layout?");
        TextMeshProUGUI descComponent = descText.GetComponent<TextMeshProUGUI>();
        descComponent.fontSize = 18;
        descComponent.alignment = TextAlignmentOptions.Center;
        descComponent.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        RectTransform descRect = descText.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0.55f);
        descRect.anchorMax = new Vector2(1, 0.7f);
        descRect.anchoredPosition = Vector2.zero;
        descRect.sizeDelta = Vector2.zero;

        // Button container
        GameObject buttonContainer = new GameObject("Board Button Container");
        buttonContainer.transform.SetParent(contentContainer.transform);
        RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
        buttonContainerRect.anchorMin = new Vector2(0.15f, 0.3f);
        buttonContainerRect.anchorMax = new Vector2(0.85f, 0.5f);
        buttonContainerRect.anchoredPosition = Vector2.zero;
        buttonContainerRect.sizeDelta = Vector2.zero;

        // Add layout group for buttons
        HorizontalLayoutGroup layoutGroup = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 40f;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = true;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;

        // Create Accept Board button
        GameObject acceptButton = CreateBoardConfirmButton("Accept Board", buttonContainer.transform, new Color(0.2f, 0.6f, 0.2f, 1f));
        Button acceptButtonComponent = acceptButton.GetComponent<Button>();
        acceptButtonComponent.onClick.AddListener(() => {
            Debug.Log("Board accepted");
            _gameManager.ConfirmBoard();
        });

        // Create Regenerate Board button
        GameObject regenerateButton = CreateBoardConfirmButton("Regenerate Board", buttonContainer.transform, new Color(0.6f, 0.3f, 0.2f, 1f));
        Button regenerateButtonComponent = regenerateButton.GetComponent<Button>();
        regenerateButtonComponent.onClick.AddListener(() => {
            Debug.Log("Board regeneration requested");
            _gameManager.RegenerateBoard();
        });

        // Hide the screen initially
        boardConfirmScreen.SetActive(false);

        Debug.Log("Board confirmation screen created successfully");
    }

    private void CreateGameOverScreen()
    {
        Debug.Log("Creating game over screen...");
        GameObject gameOverScreen = CreatePanel("Game Over Screen", _mainCanvas.transform);
        RectTransform gameOverRect = gameOverScreen.GetComponent<RectTransform>();

        // Full screen game over panel
        gameOverRect.anchorMin = Vector2.zero;
        gameOverRect.anchorMax = Vector2.one;
        gameOverRect.anchoredPosition = Vector2.zero;
        gameOverRect.sizeDelta = Vector2.zero;

        // Semi-transparent background
        Image gameOverImage = gameOverScreen.GetComponent<Image>();
        gameOverImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Create main container for content
        GameObject contentContainer = new GameObject("Game Over Content");
        contentContainer.transform.SetParent(gameOverScreen.transform);
        RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.2f, 0.2f);
        contentRect.anchorMax = new Vector2(0.8f, 0.8f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        // Title
        GameObject titleText = CreateText("Game Over Title", contentContainer.transform, "Game Over!");
        TextMeshProUGUI titleComponent = titleText.GetComponent<TextMeshProUGUI>();
        titleComponent.fontSize = 48;
        titleComponent.fontStyle = FontStyles.Bold;
        titleComponent.alignment = TextAlignmentOptions.Center;
        titleComponent.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold color
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.9f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = Vector2.zero;

        // Winner text
        GameObject winnerText = CreateText("Winner Text", contentContainer.transform, "Winner: Player 1");
        TextMeshProUGUI winnerComponent = winnerText.GetComponent<TextMeshProUGUI>();
        winnerComponent.fontSize = 36;
        winnerComponent.fontStyle = FontStyles.Bold;
        winnerComponent.alignment = TextAlignmentOptions.Center;
        winnerComponent.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green color
        RectTransform winnerRect = winnerText.GetComponent<RectTransform>();
        winnerRect.anchorMin = new Vector2(0, 0.5f);
        winnerRect.anchorMax = new Vector2(1, 0.65f);
        winnerRect.anchoredPosition = Vector2.zero;
        winnerRect.sizeDelta = Vector2.zero;

        // Score text
        GameObject scoreText = CreateText("Score Text", contentContainer.transform, "Victory Points: 5");
        TextMeshProUGUI scoreComponent = scoreText.GetComponent<TextMeshProUGUI>();
        scoreComponent.fontSize = 28;
        scoreComponent.alignment = TextAlignmentOptions.Center;
        scoreComponent.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 0.4f);
        scoreRect.anchorMax = new Vector2(1, 0.5f);
        scoreRect.anchoredPosition = Vector2.zero;
        scoreRect.sizeDelta = Vector2.zero;

        // Button container
        GameObject buttonContainer = new GameObject("Game Over Button Container");
        buttonContainer.transform.SetParent(contentContainer.transform);
        RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
        buttonContainerRect.anchorMin = new Vector2(0.2f, 0.1f);
        buttonContainerRect.anchorMax = new Vector2(0.8f, 0.3f);
        buttonContainerRect.anchoredPosition = Vector2.zero;
        buttonContainerRect.sizeDelta = Vector2.zero;

        // Add layout group for buttons
        HorizontalLayoutGroup layoutGroup = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 40f;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = true;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;

        // Create Play Again button
        GameObject playAgainButton = CreateGameOverButton("Play Again", buttonContainer.transform, new Color(0.2f, 0.6f, 0.2f, 1f));
        Button playAgainButtonComponent = playAgainButton.GetComponent<Button>();
        playAgainButtonComponent.onClick.AddListener(() => {
            Debug.Log("Play Again clicked");
            _gameManager.RestartGame();
        });

        // Create Quit button
        GameObject quitButton = CreateGameOverButton("Quit", buttonContainer.transform, new Color(0.6f, 0.2f, 0.2f, 1f));
        Button quitButtonComponent = quitButton.GetComponent<Button>();
        quitButtonComponent.onClick.AddListener(() => {
            Debug.Log("Quit clicked");
            Application.Quit();
        });

        // Hide the screen initially
        gameOverScreen.SetActive(false);

        Debug.Log("Game over screen created successfully");
    }

    private void CreateDiscardScreen()
    {
        Debug.Log("Creating discard screen...");
        GameObject discardScreen = CreatePanel("Discard Screen", _mainCanvas.transform);
        RectTransform discardRect = discardScreen.GetComponent<RectTransform>();

        // Full screen discard panel
        discardRect.anchorMin = Vector2.zero;
        discardRect.anchorMax = Vector2.one;
        discardRect.anchoredPosition = Vector2.zero;
        discardRect.sizeDelta = Vector2.zero;

        // Semi-transparent background
        Image discardImage = discardScreen.GetComponent<Image>();
        discardImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Create main container for content
        GameObject contentContainer = new GameObject("Discard Content");
        contentContainer.transform.SetParent(discardScreen.transform);
        RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.15f, 0.15f);
        contentRect.anchorMax = new Vector2(0.85f, 0.85f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        // Title
        GameObject titleText = CreateText("Discard Title", contentContainer.transform, "Discard Cards");
        TextMeshProUGUI titleComponent = titleText.GetComponent<TextMeshProUGUI>();
        titleComponent.fontSize = 36;
        titleComponent.fontStyle = FontStyles.Bold;
        titleComponent.alignment = TextAlignmentOptions.Center;
        titleComponent.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold color
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.85f);
        titleRect.anchorMax = new Vector2(1, 0.95f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = Vector2.zero;

        // Player name
        GameObject playerNameText = CreateText("Player Name", contentContainer.transform, "Player 1");
        TextMeshProUGUI playerNameComponent = playerNameText.GetComponent<TextMeshProUGUI>();
        playerNameComponent.fontSize = 28;
        playerNameComponent.fontStyle = FontStyles.Bold;
        playerNameComponent.alignment = TextAlignmentOptions.Center;
        playerNameComponent.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green color
        RectTransform playerNameRect = playerNameText.GetComponent<RectTransform>();
        playerNameRect.anchorMin = new Vector2(0, 0.75f);
        playerNameRect.anchorMax = new Vector2(1, 0.85f);
        playerNameRect.anchoredPosition = Vector2.zero;
        playerNameRect.sizeDelta = Vector2.zero;

        // Cards to discard text
        GameObject cardsToDiscardText = CreateText("Cards To Discard", contentContainer.transform, "You must discard X cards");
        TextMeshProUGUI cardsToDiscardComponent = cardsToDiscardText.GetComponent<TextMeshProUGUI>();
        cardsToDiscardComponent.fontSize = 24;
        cardsToDiscardComponent.alignment = TextAlignmentOptions.Center;
        cardsToDiscardComponent.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        RectTransform cardsToDiscardRect = cardsToDiscardText.GetComponent<RectTransform>();
        cardsToDiscardRect.anchorMin = new Vector2(0, 0.65f);
        cardsToDiscardRect.anchorMax = new Vector2(1, 0.75f);
        cardsToDiscardRect.anchoredPosition = Vector2.zero;
        cardsToDiscardRect.sizeDelta = Vector2.zero;

        // Progress text
        GameObject progressText = CreateText("Progress Text", contentContainer.transform, "Discarded: 0/X");
        TextMeshProUGUI progressComponent = progressText.GetComponent<TextMeshProUGUI>();
        progressComponent.fontSize = 20;
        progressComponent.alignment = TextAlignmentOptions.Center;
        progressComponent.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        RectTransform progressRect = progressText.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0, 0.55f);
        progressRect.anchorMax = new Vector2(1, 0.65f);
        progressRect.anchoredPosition = Vector2.zero;
        progressRect.sizeDelta = Vector2.zero;

        // Resource container
        CreateDiscardResourceContainer(contentContainer);

        // Confirm button
        GameObject confirmButton = CreateDiscardButton("Confirm Discard", contentContainer.transform, new Color(0.2f, 0.6f, 0.2f, 1f));
        // Note: Click listener will be added by DiscardUIController
        RectTransform confirmRect = confirmButton.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.3f, 0.05f);
        confirmRect.anchorMax = new Vector2(0.7f, 0.15f);
        confirmRect.anchoredPosition = Vector2.zero;
        confirmRect.sizeDelta = Vector2.zero;

        // Add DiscardUIController component
        discardScreen.AddComponent<DiscardUIController>();

        // Hide the screen initially
        discardScreen.SetActive(false);

        Debug.Log("Discard screen created successfully");
    }

    private void CreatePlayerSelectionScreen()
    {
        Debug.Log("Creating player selection screen...");

        // Create the main screen object
        GameObject playerSelectionScreen = new GameObject("Player Selection Screen");
        playerSelectionScreen.transform.SetParent(_mainCanvas.transform);

        // Add Image component for background
        Image screenImage = playerSelectionScreen.AddComponent<Image>();
        screenImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black background

        // Set up RectTransform to fill the screen
        RectTransform screenRect = playerSelectionScreen.GetComponent<RectTransform>();
        screenRect.anchorMin = Vector2.zero;
        screenRect.anchorMax = Vector2.one;
        screenRect.anchoredPosition = Vector2.zero;
        screenRect.sizeDelta = Vector2.zero;

        // Create content container
        GameObject contentContainer = new GameObject("Player Selection Content");
        contentContainer.transform.SetParent(playerSelectionScreen.transform);
        RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.1f, 0.1f);
        contentRect.anchorMax = new Vector2(0.9f, 0.9f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        // Add background to content
        Image contentImage = contentContainer.AddComponent<Image>();
        contentImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        // Create instruction text
        GameObject instructionText = CreateText("Instruction Text", contentContainer.transform, "Select a player to steal from:");
        TextMeshProUGUI instructionComponent = instructionText.GetComponent<TextMeshProUGUI>();
        instructionComponent.fontSize = 24;
        instructionComponent.alignment = TextAlignmentOptions.Center;
        instructionComponent.color = Color.white;
        RectTransform instructionRect = instructionText.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0, 0.8f);
        instructionRect.anchorMax = new Vector2(1, 0.95f);
        instructionRect.anchoredPosition = Vector2.zero;
        instructionRect.sizeDelta = Vector2.zero;

        // Create player buttons container
        GameObject playerButtonsContainer = new GameObject("Player Buttons Container");
        playerButtonsContainer.transform.SetParent(contentContainer.transform);
        RectTransform buttonsRect = playerButtonsContainer.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.1f, 0.2f);
        buttonsRect.anchorMax = new Vector2(0.9f, 0.7f);
        buttonsRect.anchoredPosition = Vector2.zero;
        buttonsRect.sizeDelta = Vector2.zero;

        // Create player buttons (up to 4 players)
        for (int i = 0; i < 4; i++)
        {
            GameObject playerButton = CreatePlayerSelectionButton($"Player {i + 1} Button", playerButtonsContainer.transform, new Color(0.2f, 0.4f, 0.8f, 1f));
            RectTransform buttonRect = playerButton.GetComponent<RectTransform>();

            // Position buttons in a 2x2 grid
            float xPos = (i % 2) * 0.5f;
            float yPos = (i < 2) ? 0.5f : 0f;
            float width = 0.45f;
            float height = 0.45f;

            buttonRect.anchorMin = new Vector2(xPos, yPos);
            buttonRect.anchorMax = new Vector2(xPos + width, yPos + height);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.sizeDelta = Vector2.zero;
            buttonRect.offsetMin = new Vector2(5, 5);
            buttonRect.offsetMax = new Vector2(-5, -5);
        }

        // Add PlayerSelectionUIController component
        playerSelectionScreen.AddComponent<PlayerSelectionUIController>();

        // Hide the screen initially
        playerSelectionScreen.SetActive(false);

        Debug.Log("Player selection screen created successfully");
    }

    private GameObject CreatePlayerSelectionButton(string text, Transform parent, Color buttonColor)
    {
        GameObject button = new GameObject(text);
        button.transform.SetParent(parent);

        Image image = button.AddComponent<Image>();
        image.color = buttonColor;

        Button buttonComponent = button.AddComponent<Button>();

        // Enhanced button color transitions
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = new Color(buttonColor.r + 0.1f, buttonColor.g + 0.1f, buttonColor.b + 0.1f, 1f);
        colors.pressedColor = new Color(buttonColor.r - 0.1f, buttonColor.g - 0.1f, buttonColor.b - 0.1f, 1f);
        colors.selectedColor = new Color(buttonColor.r + 0.05f, buttonColor.g + 0.05f, buttonColor.b + 0.05f, 1f);
        buttonComponent.colors = colors;

        GameObject textObject = CreateText($"{text} Text", button.transform, text);
        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = 18;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        RectTransform rect = button.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;

        return button;
    }

    private void CreateDiscardResourceContainer(GameObject parent)
    {
        GameObject resourceContainer = new GameObject("Resource Container");
        resourceContainer.transform.SetParent(parent.transform);
        RectTransform resourceRect = resourceContainer.AddComponent<RectTransform>();
        resourceRect.anchorMin = new Vector2(0.05f, 0.2f);
        resourceRect.anchorMax = new Vector2(0.95f, 0.5f);
        resourceRect.anchoredPosition = Vector2.zero;
        resourceRect.sizeDelta = Vector2.zero;

        string[] resources = { "Wood", "Clay", "Sheep", "Wheat", "Ore" };
        Color[] resourceColors = {
            new Color(0.6f, 0.4f, 0.2f), // Brown for wood
            new Color(0.8f, 0.4f, 0.2f), // Orange for clay
            new Color(0.9f, 0.9f, 0.9f), // Light gray for sheep
            new Color(1f, 0.8f, 0.2f),   // Yellow for wheat
            new Color(0.6f, 0.6f, 0.6f)  // Gray for ore
        };

        for (int i = 0; i < resources.Length; i++)
        {
            float xPos = i / (float)resources.Length;
            float width = 1f / resources.Length;

            // Create resource item container
            GameObject resourceItem = new GameObject($"{resources[i]} Item");
            resourceItem.transform.SetParent(resourceContainer.transform);
            RectTransform itemRect = resourceItem.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(xPos, 0);
            itemRect.anchorMax = new Vector2(xPos + width, 1);
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.sizeDelta = Vector2.zero;
            itemRect.offsetMin = new Vector2(5, 5);
            itemRect.offsetMax = new Vector2(-5, -5);

            // Create button container for increment/decrement buttons
            GameObject buttonContainer = new GameObject($"{resources[i]} Button Container");
            buttonContainer.transform.SetParent(resourceItem.transform);
            RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
            buttonContainerRect.anchorMin = new Vector2(0.1f, 0.6f);
            buttonContainerRect.anchorMax = new Vector2(0.9f, 0.95f);
            buttonContainerRect.anchoredPosition = Vector2.zero;
            buttonContainerRect.sizeDelta = Vector2.zero;

            // Create increment button (+)
            GameObject incrementButton = CreateDiscardResourceButton($"{resources[i]} Increment Button", buttonContainer.transform, new Color(0.2f, 0.6f, 0.2f, 1f));
            RectTransform incrementRect = incrementButton.GetComponent<RectTransform>();
            incrementRect.anchorMin = new Vector2(0.5f, 0);
            incrementRect.anchorMax = new Vector2(1, 1);
            incrementRect.anchoredPosition = Vector2.zero;
            incrementRect.sizeDelta = Vector2.zero;

            // Add + text to increment button
            GameObject incrementText = CreateText($"{resources[i]} Increment Text", incrementButton.transform, "+");
            TextMeshProUGUI incrementTextComponent = incrementText.GetComponent<TextMeshProUGUI>();
            incrementTextComponent.fontSize = 24;
            incrementTextComponent.alignment = TextAlignmentOptions.Center;
            incrementTextComponent.fontStyle = FontStyles.Bold;
            incrementTextComponent.color = Color.white;
            RectTransform incrementTextRect = incrementText.GetComponent<RectTransform>();
            incrementTextRect.anchorMin = Vector2.zero;
            incrementTextRect.anchorMax = Vector2.one;
            incrementTextRect.anchoredPosition = Vector2.zero;
            incrementTextRect.sizeDelta = Vector2.zero;

            // Create decrement button (-)
            GameObject decrementButton = CreateDiscardResourceButton($"{resources[i]} Decrement Button", buttonContainer.transform, new Color(0.6f, 0.2f, 0.2f, 1f));
            RectTransform decrementRect = decrementButton.GetComponent<RectTransform>();
            decrementRect.anchorMin = new Vector2(0, 0);
            decrementRect.anchorMax = new Vector2(0.5f, 1);
            decrementRect.anchoredPosition = Vector2.zero;
            decrementRect.sizeDelta = Vector2.zero;

            // Add - text to decrement button
            GameObject decrementText = CreateText($"{resources[i]} Decrement Text", decrementButton.transform, "-");
            TextMeshProUGUI decrementTextComponent = decrementText.GetComponent<TextMeshProUGUI>();
            decrementTextComponent.fontSize = 24;
            decrementTextComponent.alignment = TextAlignmentOptions.Center;
            decrementTextComponent.fontStyle = FontStyles.Bold;
            decrementTextComponent.color = Color.white;
            RectTransform decrementTextRect = decrementText.GetComponent<RectTransform>();
            decrementTextRect.anchorMin = Vector2.zero;
            decrementTextRect.anchorMax = Vector2.one;
            decrementTextRect.anchoredPosition = Vector2.zero;
            decrementTextRect.sizeDelta = Vector2.zero;

            // Create resource icon (positioned above the buttons)
            GameObject icon = new GameObject($"{resources[i]} Icon");
            icon.transform.SetParent(resourceItem.transform);
            Image iconImage = icon.AddComponent<Image>();
            iconImage.color = resourceColors[i];
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.7f);
            iconRect.anchorMax = new Vector2(0.9f, 0.95f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = Vector2.zero;

            // Create current count text
            GameObject countText = CreateText($"{resources[i]} Count", resourceItem.transform, "0");
            TextMeshProUGUI countComponent = countText.GetComponent<TextMeshProUGUI>();
            countComponent.fontSize = 20;
            countComponent.alignment = TextAlignmentOptions.Center;
            countComponent.fontStyle = FontStyles.Bold;
            RectTransform countRect = countText.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.1f, 0.3f);
            countRect.anchorMax = new Vector2(0.9f, 0.55f);
            countRect.anchoredPosition = Vector2.zero;
            countRect.sizeDelta = Vector2.zero;

            // Create discard count text
            GameObject discardCountText = CreateText($"{resources[i]} Discard Count", resourceItem.transform, "0");
            TextMeshProUGUI discardCountComponent = discardCountText.GetComponent<TextMeshProUGUI>();
            discardCountComponent.fontSize = 16;
            discardCountComponent.alignment = TextAlignmentOptions.Center;
            discardCountComponent.color = new Color(1f, 0.2f, 0.2f, 1f); // Red color
            RectTransform discardCountRect = discardCountText.GetComponent<RectTransform>();
            discardCountRect.anchorMin = new Vector2(0.1f, 0.1f);
            discardCountRect.anchorMax = new Vector2(0.9f, 0.25f);
            discardCountRect.anchoredPosition = Vector2.zero;
            discardCountRect.sizeDelta = Vector2.zero;
        }
    }

    private GameObject CreateDiscardButton(string text, Transform parent, Color buttonColor)
    {
        GameObject button = new GameObject($"Discard Button - {text}");
        button.transform.SetParent(parent);

        Image image = button.AddComponent<Image>();
        image.color = buttonColor;

        Button buttonComponent = button.AddComponent<Button>();

        // Enhanced button color transitions
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = new Color(buttonColor.r + 0.1f, buttonColor.g + 0.1f, buttonColor.b + 0.1f, 1f);
        colors.pressedColor = new Color(buttonColor.r - 0.1f, buttonColor.g - 0.1f, buttonColor.b - 0.1f, 1f);
        colors.selectedColor = new Color(buttonColor.r + 0.05f, buttonColor.g + 0.05f, buttonColor.b + 0.05f, 1f);
        buttonComponent.colors = colors;

        GameObject textObject = CreateText($"{text} Text", button.transform, text);
        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = 20;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        RectTransform rect = button.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;

        return button;
    }

    private GameObject CreateDiscardResourceButton(string name, Transform parent, Color buttonColor)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent);

        Image image = button.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        Button buttonComponent = button.AddComponent<Button>();

        // Enhanced button color transitions
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 0.9f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        colors.selectedColor = new Color(0.35f, 0.35f, 0.35f, 0.85f);
        buttonComponent.colors = colors;

        RectTransform rect = button.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;

        return button;
    }

    private void CreateDevCardSelectionScreen()
    {
        GameObject screen = new GameObject("Dev Card Selection Screen");
        screen.transform.SetParent(_mainCanvas.transform);

        Image screenImage = screen.AddComponent<Image>();
        screenImage.color = new Color(0f, 0f, 0f, 0.8f);

        RectTransform screenRect = screen.GetComponent<RectTransform>();
        screenRect.anchorMin = Vector2.zero;
        screenRect.anchorMax = Vector2.one;
        screenRect.anchoredPosition = Vector2.zero;
        screenRect.sizeDelta = Vector2.zero;

        GameObject contentContainer = new GameObject("Dev Card Selection Content");
        contentContainer.transform.SetParent(screen.transform);
        RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.2f, 0.2f);
        contentRect.anchorMax = new Vector2(0.8f, 0.8f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        Image contentImage = contentContainer.AddComponent<Image>();
        contentImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        GameObject instructionText = CreateText("Instruction Text", contentContainer.transform, "Choose a card to play:");
        TextMeshProUGUI instructionComp = instructionText.GetComponent<TextMeshProUGUI>();
        instructionComp.fontSize = 24;
        instructionComp.alignment = TextAlignmentOptions.Center;
        RectTransform instructionRect = instructionText.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0, 0.8f);
        instructionRect.anchorMax = new Vector2(1, 0.95f);
        instructionRect.anchoredPosition = Vector2.zero;
        instructionRect.sizeDelta = Vector2.zero;

        GameObject cardButtonsContainer = new GameObject("Card Buttons Container");
        cardButtonsContainer.transform.SetParent(contentContainer.transform);
        RectTransform buttonsRect = cardButtonsContainer.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.05f, 0.1f);
        buttonsRect.anchorMax = new Vector2(0.95f, 0.75f);
        buttonsRect.anchoredPosition = Vector2.zero;
        buttonsRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = cardButtonsContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        screen.AddComponent<DevCardSelectionUIController>();
        screen.SetActive(false);
    }

    private void CreateResourceTypeSelectionScreen()
    {
        GameObject screen = new GameObject("Resource Type Selection Screen");
        screen.transform.SetParent(_mainCanvas.transform);

        Image screenImage = screen.AddComponent<Image>();
        screenImage.color = new Color(0f, 0f, 0f, 0.8f);

        RectTransform screenRect = screen.GetComponent<RectTransform>();
        screenRect.anchorMin = Vector2.zero;
        screenRect.anchorMax = Vector2.one;
        screenRect.anchoredPosition = Vector2.zero;
        screenRect.sizeDelta = Vector2.zero;

        GameObject contentContainer = new GameObject("Resource Selection Content");
        contentContainer.transform.SetParent(screen.transform);
        RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.2f, 0.25f);
        contentRect.anchorMax = new Vector2(0.8f, 0.75f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        Image contentImage = contentContainer.AddComponent<Image>();
        contentImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        GameObject instructionText = CreateText("Instruction Text", contentContainer.transform, "Choose a resource:");
        TextMeshProUGUI instructionComp = instructionText.GetComponent<TextMeshProUGUI>();
        instructionComp.fontSize = 24;
        instructionComp.alignment = TextAlignmentOptions.Center;
        RectTransform instructionRect = instructionText.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0, 0.75f);
        instructionRect.anchorMax = new Vector2(1, 0.95f);
        instructionRect.anchoredPosition = Vector2.zero;
        instructionRect.sizeDelta = Vector2.zero;

        GameObject resourceButtonsContainer = new GameObject("Resource Buttons Container");
        resourceButtonsContainer.transform.SetParent(contentContainer.transform);
        RectTransform buttonsRect = resourceButtonsContainer.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.05f, 0.05f);
        buttonsRect.anchorMax = new Vector2(0.95f, 0.7f);
        buttonsRect.anchoredPosition = Vector2.zero;
        buttonsRect.sizeDelta = Vector2.zero;

        HorizontalLayoutGroup layout = resourceButtonsContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        string[] resourceNames = { "Wood", "Clay", "Sheep", "Wheat", "Ore" };
        Color[] resourceColors =
        {
            new Color(0.6f, 0.4f, 0.2f),
            new Color(0.8f, 0.4f, 0.2f),
            new Color(0.9f, 0.9f, 0.9f),
            new Color(1f, 0.8f, 0.2f),
            new Color(0.6f, 0.6f, 0.6f),
        };

        for (int i = 0; i < resourceNames.Length; i++)
        {
            GameObject btn = new GameObject($"{resourceNames[i]} Button");
            btn.transform.SetParent(resourceButtonsContainer.transform);

            Image btnImage = btn.AddComponent<Image>();
            btnImage.color = resourceColors[i];

            btn.AddComponent<Button>();

            GameObject textObj = CreateText($"{resourceNames[i]} Text", btn.transform, resourceNames[i], 18);
            TextMeshProUGUI textComp = textObj.GetComponent<TextMeshProUGUI>();
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.color = Color.black;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;
        }

        screen.AddComponent<ResourceTypeSelectionUIController>();
        screen.SetActive(false);
    }

    private void CreateActionPanel()
    {
        GameObject actionPanel = CreatePanel("Action Panel", _mainCanvas.transform);
        RectTransform actionRect = actionPanel.GetComponent<RectTransform>();

        // Anchor the panel to the left side with proper spacing
        actionRect.anchorMin = new Vector2(0, 0.5f);
        actionRect.anchorMax = new Vector2(0, 0.5f);
        actionRect.anchoredPosition = new Vector2(130, 0); // Move it further right to be fully visible

        // Add layout components to auto-size the panel
        VerticalLayoutGroup layoutGroup = actionPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 10f;
        layoutGroup.padding = new RectOffset(15, 15, 15, 15);
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;

        ContentSizeFitter sizeFitter = actionPanel.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreateActionButtons(actionPanel);
    }

    private void CreateActionButtons(GameObject parent)
    {
        string[] buttonNames = { "Roll Dice", "Build Road", "Build Settlement", "Build City", "Buy Dev Card", "Play Dev Card", "Trade", "End Turn" };
        float buttonHeight = 50f;
        int fontSize = 30;
        float buttonWidth = 200f;

        for (int i = 0; i < buttonNames.Length; i++)
        {
            GameObject button = CreateButton(buttonNames[i], parent.transform, fontSize);
            RectTransform buttonRect = button.GetComponent<RectTransform>();

            // Set a fixed size for each button
            LayoutElement layoutElement = button.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = buttonHeight;
            layoutElement.preferredWidth = buttonWidth;
            layoutElement.flexibleWidth = 0;
            layoutElement.flexibleHeight = 0;
        }
    }

    private void CreatePlayerPanelsArea()
    {
        Debug.Log(".............................................CreatePlayerPanelsArea");
        GameObject playerPanelsContainer = CreatePanel("Player Panels Container", _mainCanvas.transform);
        RectTransform containerRect = playerPanelsContainer.GetComponent<RectTransform>();

        containerRect.anchorMin = new Vector2(0.7f, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = Vector2.zero;
        containerRect.offsetMin = new Vector2(20, 20);
        containerRect.offsetMax = new Vector2(-20, -20);

        VerticalLayoutGroup layout = playerPanelsContainer.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 10f;
        layout.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter fitter = playerPanelsContainer.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreatePlayerPanelPrefab(playerPanelsContainer);
    }

    private void CreatePlayerPanelPrefab(GameObject container)
    {
        Debug.Log(".............................................CreatePlayerPanelPrefab");
        GameObject playerPanel = CreatePanel("Player Panel Prefab", container.transform);
        RectTransform panelRect = playerPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, 180); // Increased height for more content

        PlayerUIPanel panelScript = playerPanel.AddComponent<PlayerUIPanel>();

        CreatePlayerPanelContent(playerPanel, panelScript);

        // Add a subtle border
        CreatePanelBorder(playerPanel);

        // Add a button component for clicking
        Button panelButton = playerPanel.AddComponent<Button>();
        panelButton.transition = Selectable.Transition.ColorTint;
        ColorBlock colorBlock = panelButton.colors;
        colorBlock.highlightedColor = new Color(0.3f, 0.3f, 0.4f, 0.9f);
        colorBlock.pressedColor = new Color(0.4f, 0.4f, 0.5f, 0.9f);
        panelButton.colors = colorBlock;

        playerPanel.SetActive(false);
    }

    private void CreatePlayerPanelContent(GameObject parent, PlayerUIPanel panelScript)
    {
        Debug.Log(".............................................CreatePlayerPanelContent");
        // Create header section with player name and victory points
        GameObject headerContainer = new GameObject("Header");
        headerContainer.transform.SetParent(parent.transform);
        RectTransform headerRect = headerContainer.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 0.75f);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.anchoredPosition = Vector2.zero;
        headerRect.sizeDelta = Vector2.zero;
        headerRect.offsetMin = new Vector2(10, 0);
        headerRect.offsetMax = new Vector2(-10, 0);

        // Player name
        GameObject playerNameText = CreateText("Player Name", headerContainer.transform, "Player 1");
        panelScript.playerNameText = playerNameText.GetComponent<TextMeshProUGUI>();
        panelScript.playerNameText.fontSize = 30;
        panelScript.playerNameText.fontStyle = FontStyles.Bold;
        RectTransform nameRect = playerNameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0);
        nameRect.anchorMax = new Vector2(0.6f, 1);
        nameRect.anchoredPosition = Vector2.zero;
        nameRect.sizeDelta = Vector2.zero;

        // Victory points
        GameObject vpText = CreateText("Victory Points", headerContainer.transform, "VP: 0");
        panelScript.victoryPointsText = vpText.GetComponent<TextMeshProUGUI>();
        panelScript.victoryPointsText.fontSize = 30;
        panelScript.victoryPointsText.fontStyle = FontStyles.Bold;
        panelScript.victoryPointsText.alignment = TextAlignmentOptions.MidlineRight;
        RectTransform vpRect = vpText.GetComponent<RectTransform>();
        vpRect.anchorMin = new Vector2(0.6f, 0);
        vpRect.anchorMax = new Vector2(0.85f, 1);
        vpRect.anchoredPosition = Vector2.zero;
        vpRect.sizeDelta = Vector2.zero;

        // Player color indicator
        GameObject colorIndicator = new GameObject("Color Indicator");
        colorIndicator.transform.SetParent(headerContainer.transform);
        panelScript.playerColorIndicator = colorIndicator.AddComponent<Image>();
        RectTransform colorRect = colorIndicator.GetComponent<RectTransform>();
        colorRect.anchorMin = new Vector2(0.85f, 0.1f);
        colorRect.anchorMax = new Vector2(0.95f, 0.9f);
        colorRect.anchoredPosition = Vector2.zero;
        colorRect.sizeDelta = Vector2.zero;

        // Create resource display section
        CreateEnhancedResourceDisplay(parent, panelScript);

        // Create additional info section
        CreateAdditionalInfoSection(parent, panelScript);

        // Active player indicator (border glow)
        GameObject activeIndicator = new GameObject("Active Indicator");
        activeIndicator.transform.SetParent(parent.transform);
        Image activeImage = activeIndicator.AddComponent<Image>();
        activeImage.color = new Color(1f, 1f, 0f, 0.3f); // Yellow glow
        panelScript.activePlayerIndicator = activeIndicator;
        RectTransform activeRect = activeIndicator.GetComponent<RectTransform>();
        activeRect.anchorMin = new Vector2(0, 0);
        activeRect.anchorMax = new Vector2(1, 1);
        activeRect.anchoredPosition = Vector2.zero;
        activeRect.sizeDelta = new Vector2(4, 4); // Slight border expansion
        activeIndicator.SetActive(false);
    }

    private void CreateEnhancedResourceDisplay(GameObject parent, PlayerUIPanel panelScript)
    {
        GameObject resourceContainer = new GameObject("Resources");
        resourceContainer.transform.SetParent(parent.transform);
        RectTransform resourceRect = resourceContainer.AddComponent<RectTransform>();
        resourceRect.anchorMin = new Vector2(0, 0.35f);
        resourceRect.anchorMax = new Vector2(1, 0.75f);
        resourceRect.anchoredPosition = Vector2.zero;
        resourceRect.sizeDelta = Vector2.zero;
        resourceRect.offsetMin = new Vector2(10, 0);
        resourceRect.offsetMax = new Vector2(-10, 0);

        string[] resources = { "Wood", "Clay", "Sheep", "Wheat", "Ore" };
        Color[] resourceColors = {
            new Color(0.6f, 0.4f, 0.2f), // Brown for wood
            new Color(0.8f, 0.4f, 0.2f), // Orange for clay
            new Color(0.9f, 0.9f, 0.9f), // Light gray for sheep
            new Color(1f, 0.8f, 0.2f),   // Yellow for wheat
            new Color(0.6f, 0.6f, 0.6f)  // Gray for ore
        };

        for (int i = 0; i < resources.Length; i++)
        {
            float xPos = i / (float)resources.Length;
            float width = 1f / resources.Length;

            // Create resource item container
            GameObject resourceItem = new GameObject($"{resources[i]} Item");
            resourceItem.transform.SetParent(resourceContainer.transform);
            RectTransform itemRect = resourceItem.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(xPos, 0);
            itemRect.anchorMax = new Vector2(xPos + width, 1);
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.sizeDelta = Vector2.zero;

            // Create icon background
            GameObject iconBg = new GameObject($"{resources[i]} Icon Background");
            iconBg.transform.SetParent(resourceItem.transform);
            Image iconBgImage = iconBg.AddComponent<Image>();
            iconBgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            RectTransform iconBgRect = iconBg.GetComponent<RectTransform>();
            iconBgRect.anchorMin = new Vector2(0.1f, 0.5f);
            iconBgRect.anchorMax = new Vector2(0.9f, 0.95f);
            iconBgRect.anchoredPosition = Vector2.zero;
            iconBgRect.sizeDelta = Vector2.zero;

            // Create resource icon
            GameObject icon = new GameObject($"{resources[i]} Icon");
            icon.transform.SetParent(iconBg.transform);
            Image iconImage = icon.AddComponent<Image>();
            iconImage.color = resourceColors[i];
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.2f, 0.2f);
            iconRect.anchorMax = new Vector2(0.8f, 0.8f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = Vector2.zero;

            // Assign icon to panel script
            switch (resources[i])
            {
                case "Wood": panelScript.woodIcon = iconImage; break;
                case "Clay": panelScript.clayIcon = iconImage; break;
                case "Sheep": panelScript.sheepIcon = iconImage; break;
                case "Wheat": panelScript.wheatIcon = iconImage; break;
                case "Ore": panelScript.oreIcon = iconImage; break;
            }

            // Create resource count text
            GameObject countText = CreateText($"{resources[i]} Count", resourceItem.transform, "0");
            TextMeshProUGUI textComponent = countText.GetComponent<TextMeshProUGUI>();
            textComponent.fontSize = 25;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontStyle = FontStyles.Bold;
            RectTransform textRect = countText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.05f);
            textRect.anchorMax = new Vector2(0.9f, 0.45f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            // Assign text to panel script
            switch (resources[i])
            {
                case "Wood": panelScript.woodCountText = textComponent; break;
                case "Clay": panelScript.clayCountText = textComponent; break;
                case "Sheep": panelScript.sheepCountText = textComponent; break;
                case "Wheat": panelScript.wheatCountText = textComponent; break;
                case "Ore": panelScript.oreCountText = textComponent; break;
            }
        }

        // Create total resources text
        GameObject totalText = CreateText("Total Resources", parent.transform, "Total: 0");
        panelScript.totalResourcesText = totalText.GetComponent<TextMeshProUGUI>();
        panelScript.totalResourcesText.fontSize = 25;
        panelScript.totalResourcesText.alignment = TextAlignmentOptions.MidlineRight;
        panelScript.totalResourcesText.fontStyle = FontStyles.Italic;
        RectTransform totalRect = totalText.GetComponent<RectTransform>();
        totalRect.anchorMin = new Vector2(0.5f, 0.25f);
        totalRect.anchorMax = new Vector2(0.95f, 0.35f);
        totalRect.anchoredPosition = Vector2.zero;
        totalRect.sizeDelta = Vector2.zero;
    }

    private void CreateAdditionalInfoSection(GameObject parent, PlayerUIPanel panelScript)
    {
        GameObject infoContainer = new GameObject("Additional Info");
        infoContainer.transform.SetParent(parent.transform);
        RectTransform infoRect = infoContainer.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0.05f);
        infoRect.anchorMax = new Vector2(1, 0.35f);
        infoRect.anchoredPosition = Vector2.zero;
        infoRect.sizeDelta = Vector2.zero;
        infoRect.offsetMin = new Vector2(10, 0);
        infoRect.offsetMax = new Vector2(-10, 0);

        // Development cards
        GameObject devCardsText = CreateText("Development Cards", infoContainer.transform, "Dev Cards: 0");
        panelScript.developmentCardsText = devCardsText.GetComponent<TextMeshProUGUI>();
        panelScript.developmentCardsText.fontSize = 25;
        panelScript.developmentCardsText.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform devRect = devCardsText.GetComponent<RectTransform>();
        devRect.anchorMin = new Vector2(0, 0.7f);
        devRect.anchorMax = new Vector2(1, 1);
        devRect.anchoredPosition = Vector2.zero;
        devRect.sizeDelta = Vector2.zero;

        // Longest road indicator
        GameObject longestRoadText = CreateText("Longest Road", infoContainer.transform, "");
        panelScript.longestRoadText = longestRoadText.GetComponent<TextMeshProUGUI>();
        panelScript.longestRoadText.fontSize = 12;
        panelScript.longestRoadText.alignment = TextAlignmentOptions.MidlineLeft;
        panelScript.longestRoadText.color = Color.yellow;
        RectTransform roadRect = longestRoadText.GetComponent<RectTransform>();
        roadRect.anchorMin = new Vector2(0, 0.35f);
        roadRect.anchorMax = new Vector2(0.5f, 0.65f);
        roadRect.anchoredPosition = Vector2.zero;
        roadRect.sizeDelta = Vector2.zero;

        // Largest army indicator
        GameObject largestArmyText = CreateText("Largest Army", infoContainer.transform, "");
        panelScript.largestArmyText = largestArmyText.GetComponent<TextMeshProUGUI>();
        panelScript.largestArmyText.fontSize = 12;
        panelScript.largestArmyText.alignment = TextAlignmentOptions.MidlineRight;
        panelScript.largestArmyText.color = Color.red;
        RectTransform armyRect = largestArmyText.GetComponent<RectTransform>();
        armyRect.anchorMin = new Vector2(0.5f, 0.35f);
        armyRect.anchorMax = new Vector2(1, 0.65f);
        armyRect.anchoredPosition = Vector2.zero;
        armyRect.sizeDelta = Vector2.zero;
    }

    private void CreatePanelBorder(GameObject panel)
    {
        GameObject border = new GameObject("Panel Border");
        border.transform.SetParent(panel.transform);
        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(2, 2);
    }

    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;

        return panel;
    }

    private GameObject CreateButton(string name, Transform parent, int fontSize)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent);

        Image image = button.AddComponent<Image>();
        image.color = new Color(0.4f, 0.4f, 0.4f, 1f);

        Button buttonComponent = button.AddComponent<Button>();

        GameObject textObject = CreateText($"{name} Text", button.transform, name, fontSize);
        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        RectTransform rect = button.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;

        return button;
    }

    private GameObject CreateText(string name, Transform parent, string text, int fontSize = 20)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent);

        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;

        return textObject;
    }

    private GameObject CreateSetupButton(string text, Transform parent)
    {
        GameObject button = new GameObject($"Setup Button - {text}");
        button.transform.SetParent(parent);

        Image image = button.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.6f, 1f); // Blue button color

        Button buttonComponent = button.AddComponent<Button>();

        // Enhanced button color transitions
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = new Color(0.2f, 0.4f, 0.6f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.5f, 1f);
        colors.selectedColor = new Color(0.25f, 0.45f, 0.65f, 1f);
        buttonComponent.colors = colors;

        GameObject textObject = CreateText($"{text} Text", button.transform, text);
        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = 24;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        RectTransform rect = button.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;

        return button;
    }

    private GameObject CreateBoardConfirmButton(string text, Transform parent, Color buttonColor)
    {
        GameObject button = new GameObject($"Board Confirm Button - {text}");
        button.transform.SetParent(parent);

        Image image = button.AddComponent<Image>();
        image.color = buttonColor;

        Button buttonComponent = button.AddComponent<Button>();

        // Enhanced button color transitions
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = new Color(buttonColor.r + 0.1f, buttonColor.g + 0.1f, buttonColor.b + 0.1f, 1f);
        colors.pressedColor = new Color(buttonColor.r - 0.1f, buttonColor.g - 0.1f, buttonColor.b - 0.1f, 1f);
        colors.selectedColor = new Color(buttonColor.r + 0.05f, buttonColor.g + 0.05f, buttonColor.b + 0.05f, 1f);
        buttonComponent.colors = colors;

        GameObject textObject = CreateText($"{text} Text", button.transform, text);
        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = 20;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        RectTransform rect = button.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;

        return button;
    }

    private GameObject CreateGameOverButton(string text, Transform parent, Color buttonColor)
    {
        GameObject button = new GameObject($"Game Over Button - {text}");
        button.transform.SetParent(parent);

        Image image = button.AddComponent<Image>();
        image.color = buttonColor;

        Button buttonComponent = button.AddComponent<Button>();

        // Enhanced button color transitions
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = new Color(buttonColor.r + 0.1f, buttonColor.g + 0.1f, buttonColor.b + 0.1f, 1f);
        colors.pressedColor = new Color(buttonColor.r - 0.1f, buttonColor.g - 0.1f, buttonColor.b - 0.1f, 1f);
        colors.selectedColor = new Color(buttonColor.r + 0.05f, buttonColor.g + 0.05f, buttonColor.b + 0.05f, 1f);
        buttonComponent.colors = colors;

        GameObject textObject = CreateText($"{text} Text", button.transform, text);
        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = 24;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        RectTransform rect = button.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;

        return button;
    }
}
