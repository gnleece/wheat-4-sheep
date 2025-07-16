using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Canvas mainCanvas;
    public GameObject actionPanel;
    public GameObject playerPanelsContainer;
    public GameObject playerPanelPrefab;
    
    [Header("Action Buttons")]
    public Button buildRoadButton;
    public Button buildSettlementButton;
    public Button buildCityButton;
    public Button buyDevelopmentCardButton;
    public Button tradeButton;
    public Button endTurnButton;
    
    private GameManager gameManager;
    private List<PlayerUIPanel> playerUIPanels = new List<PlayerUIPanel>();
    
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        SetupActionButtons();
    }
    
    private void SetupActionButtons()
    {
        if (buildRoadButton != null)
            buildRoadButton.onClick.AddListener(() => Debug.Log("Build Road clicked"));
            
        if (buildSettlementButton != null)
            buildSettlementButton.onClick.AddListener(() => Debug.Log("Build Settlement clicked"));
            
        if (buildCityButton != null)
            buildCityButton.onClick.AddListener(() => Debug.Log("Build City clicked"));
            
        if (buyDevelopmentCardButton != null)
            buyDevelopmentCardButton.onClick.AddListener(() => Debug.Log("Buy Development Card clicked"));
            
        if (tradeButton != null)
            tradeButton.onClick.AddListener(() => Debug.Log("Trade clicked"));
            
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(() => Debug.Log("End Turn clicked"));
    }
    
    public void InitializePlayerPanels(IReadOnlyList<IPlayer> players)
    {
        Debug.Log($"UIManager: InitializePlayerPanels called with {players.Count} players");
        ClearPlayerPanels();
        
        foreach (var player in players)
        {
            CreatePlayerPanel(player);
        }
        
        // Show action panel now that players are set up
        if (actionPanel != null)
            actionPanel.SetActive(true);
            
        Debug.Log($"UIManager: Created {playerUIPanels.Count} player panels");
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
    }
    
    public void SetActivePlayer(int playerId)
    {
        foreach (var panel in playerUIPanels)
        {
            if (panel != null)
                panel.SetAsActivePlayer(panel.Player.PlayerId == playerId);
        }
    }
}