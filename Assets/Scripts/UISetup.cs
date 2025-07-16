using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISetup : MonoBehaviour
{
    [Header("Prefab References")]
    public Font defaultFont;
    public Sprite buttonSprite;
    public Sprite panelSprite;
    
    private Canvas mainCanvas;
    private UIManager uiManager;
    
    private void Start()
    {
        Debug.Log("UISetup Start() called");
        CreateMainUI();
    }
    
    private void CreateMainUI()
    {
        Debug.Log("Creating main UI...");
        CreateCanvas();
        CreateActionPanel();
        CreatePlayerPanelsArea();
        SetupUIManager();
        Debug.Log("Main UI creation completed");
    }
    
    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Main UI Canvas");
        canvasObject.transform.SetParent(transform);
        
        mainCanvas = canvasObject.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObject.AddComponent<GraphicRaycaster>();
    }
    
    private void CreateActionPanel()
    {
        GameObject actionPanel = CreatePanel("Action Panel", mainCanvas.transform);
        RectTransform actionRect = actionPanel.GetComponent<RectTransform>();
        
        actionRect.anchorMin = new Vector2(0, 0);
        actionRect.anchorMax = new Vector2(0.3f, 0.4f);
        actionRect.anchoredPosition = Vector2.zero;
        actionRect.sizeDelta = Vector2.zero;
        actionRect.offsetMin = new Vector2(20, 20);
        actionRect.offsetMax = new Vector2(-20, -20);
        
        CreateActionButtons(actionPanel);
    }
    
    private void CreateActionButtons(GameObject parent)
    {
        string[] buttonNames = { "Build Road", "Build Settlement", "Build City", "Buy Dev Card", "Trade", "End Turn" };
        float buttonHeight = 60f;
        float spacing = 10f;
        
        for (int i = 0; i < buttonNames.Length; i++)
        {
            GameObject button = CreateButton(buttonNames[i], parent.transform);
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            
            buttonRect.anchorMin = new Vector2(0, 1);
            buttonRect.anchorMax = new Vector2(1, 1);
            buttonRect.sizeDelta = new Vector2(0, buttonHeight);
            buttonRect.anchoredPosition = new Vector2(0, -(i * (buttonHeight + spacing) + buttonHeight / 2 + 20));
        }
    }
    
    private void CreatePlayerPanelsArea()
    {
        Debug.Log(".............................................CreatePlayerPanelsArea");
        GameObject playerPanelsContainer = CreatePanel("Player Panels Container", mainCanvas.transform);
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
        panelScript.playerNameText.fontSize = 18;
        panelScript.playerNameText.fontStyle = FontStyles.Bold;
        RectTransform nameRect = playerNameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0);
        nameRect.anchorMax = new Vector2(0.6f, 1);
        nameRect.anchoredPosition = Vector2.zero;
        nameRect.sizeDelta = Vector2.zero;
        
        // Victory points
        GameObject vpText = CreateText("Victory Points", headerContainer.transform, "VP: 0");
        panelScript.victoryPointsText = vpText.GetComponent<TextMeshProUGUI>();
        panelScript.victoryPointsText.fontSize = 16;
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
    
    private void CreateResourceDisplay(GameObject parent, PlayerUIPanel panelScript)
    {
        GameObject resourceContainer = new GameObject("Resources");
        resourceContainer.transform.SetParent(parent.transform);
        RectTransform resourceRect = resourceContainer.AddComponent<RectTransform>();
        resourceRect.anchorMin = new Vector2(0, 0);
        resourceRect.anchorMax = new Vector2(1, 0.7f);
        resourceRect.anchoredPosition = Vector2.zero;
        resourceRect.sizeDelta = Vector2.zero;
        
        string[] resources = { "Wood", "Clay", "Sheep", "Wheat", "Ore" };
        
        for (int i = 0; i < resources.Length; i++)
        {
            float xPos = i / (float)resources.Length;
            float width = 1f / resources.Length;
            
            GameObject resourceItem = CreateText($"{resources[i]} Count", resourceContainer.transform, "0");
            TextMeshProUGUI textComponent = resourceItem.GetComponent<TextMeshProUGUI>();
            textComponent.fontSize = 14;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            RectTransform itemRect = resourceItem.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(xPos, 0);
            itemRect.anchorMax = new Vector2(xPos + width, 1);
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.sizeDelta = Vector2.zero;
            
            switch (resources[i])
            {
                case "Wood": panelScript.woodCountText = textComponent; break;
                case "Clay": panelScript.clayCountText = textComponent; break;
                case "Sheep": panelScript.sheepCountText = textComponent; break;
                case "Wheat": panelScript.wheatCountText = textComponent; break;
                case "Ore": panelScript.oreCountText = textComponent; break;
            }
        }
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
            textComponent.fontSize = 12;
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
        panelScript.totalResourcesText.fontSize = 11;
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
        panelScript.developmentCardsText.fontSize = 10;
        panelScript.developmentCardsText.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform devRect = devCardsText.GetComponent<RectTransform>();
        devRect.anchorMin = new Vector2(0, 0.7f);
        devRect.anchorMax = new Vector2(1, 1);
        devRect.anchoredPosition = Vector2.zero;
        devRect.sizeDelta = Vector2.zero;
        
        // Longest road indicator
        GameObject longestRoadText = CreateText("Longest Road", infoContainer.transform, "");
        panelScript.longestRoadText = longestRoadText.GetComponent<TextMeshProUGUI>();
        panelScript.longestRoadText.fontSize = 10;
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
        panelScript.largestArmyText.fontSize = 10;
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
    
    private void SetupUIManager()
    {
        GameObject uiManagerObject = new GameObject("UI Manager");
        uiManagerObject.transform.SetParent(transform);
        uiManager = uiManagerObject.AddComponent<UIManager>();
        
        uiManager.mainCanvas = mainCanvas;
        uiManager.actionPanel = mainCanvas.transform.Find("Action Panel").gameObject;
        uiManager.playerPanelsContainer = mainCanvas.transform.Find("Player Panels Container").gameObject;
        uiManager.playerPanelPrefab = uiManager.playerPanelsContainer.transform.Find("Player Panel Prefab").gameObject;
        
        AssignActionButtons();
        
        // Hide action panel initially since no players are set up yet
        uiManager.actionPanel.SetActive(false);
        
        Debug.Log("UIManager setup completed successfully");
    }
    
    private void AssignActionButtons()
    {
        Transform actionPanel = mainCanvas.transform.Find("Action Panel");
        
        uiManager.buildRoadButton = actionPanel.Find("Build Road")?.GetComponent<Button>();
        uiManager.buildSettlementButton = actionPanel.Find("Build Settlement")?.GetComponent<Button>();
        uiManager.buildCityButton = actionPanel.Find("Build City")?.GetComponent<Button>();
        uiManager.buyDevelopmentCardButton = actionPanel.Find("Buy Dev Card")?.GetComponent<Button>();
        uiManager.tradeButton = actionPanel.Find("Trade")?.GetComponent<Button>();
        uiManager.endTurnButton = actionPanel.Find("End Turn")?.GetComponent<Button>();
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
    
    private GameObject CreateButton(string name, Transform parent)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent);
        
        Image image = button.AddComponent<Image>();
        image.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        
        Button buttonComponent = button.AddComponent<Button>();
        
        GameObject textObject = CreateText($"{name} Text", button.transform, name);
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
    
    private GameObject CreateText(string name, Transform parent, string text)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent);
        
        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 16;
        textComponent.color = Color.white;
        
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        
        return textObject;
    }
}