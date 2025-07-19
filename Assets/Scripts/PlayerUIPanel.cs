using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerUIPanel : MonoBehaviour
{
    [Header("Player Info")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI victoryPointsText;
    public Image playerColorIndicator;
    public GameObject activePlayerIndicator;
    
    [Header("Resource Display")]
    public TextMeshProUGUI woodCountText;
    public TextMeshProUGUI clayCountText;
    public TextMeshProUGUI sheepCountText;
    public TextMeshProUGUI wheatCountText;
    public TextMeshProUGUI oreCountText;
    public TextMeshProUGUI totalResourcesText;
    
    [Header("Resource Icons")]
    public Image woodIcon;
    public Image clayIcon;
    public Image sheepIcon;
    public Image wheatIcon;
    public Image oreIcon;
    
    [Header("Additional Info")]
    public TextMeshProUGUI developmentCardsText;
    public TextMeshProUGUI longestRoadText;
    public TextMeshProUGUI largestArmyText;
    
    private IPlayer player;
    private BoardManager boardManager;
    
    public IPlayer Player => player;
    
    public void Initialize(IPlayer playerData)
    {
        Debug.Log($"PlayerUIPanel: Initializing panel for Player {playerData?.PlayerId}");
        player = playerData;
        boardManager = FindObjectOfType<BoardManager>();
        
        if (boardManager == null)
        {
            Debug.LogWarning("PlayerUIPanel: BoardManager not found!");
        }
        
        SetupPlayerInfo();
        UpdateDisplay();
        Debug.Log($"PlayerUIPanel: Initialization completed for Player {player?.PlayerId}");
    }
    
    private void SetupPlayerInfo()
    {
        if (player == null) return;
        
        if (playerNameText != null)
            playerNameText.text = $"Player {player.PlayerId + 1}";
            
        if (playerColorIndicator != null)
            playerColorIndicator.color = player.PlayerColor;
    }
    
    public void UpdateDisplay()
    {
        if (player == null || boardManager == null) return;
        
        UpdateResourceDisplay();
        UpdateVictoryPoints();
    }
    
    private void UpdateResourceDisplay()
    {
        var resources = boardManager.GetResourceHandForPlayer(player);
        if (resources == null) return;
        int totalResources = 0;
        
        foreach (var resource in resources)
        {
            totalResources += resource.Value;
            
            switch (resource.Key)
            {
                case ResourceType.Wood:
                    if (woodCountText != null) woodCountText.text = resource.Value.ToString();
                    break;
                case ResourceType.Clay:
                    if (clayCountText != null) clayCountText.text = resource.Value.ToString();
                    break;
                case ResourceType.Sheep:
                    if (sheepCountText != null) sheepCountText.text = resource.Value.ToString();
                    break;
                case ResourceType.Wheat:
                    if (wheatCountText != null) wheatCountText.text = resource.Value.ToString();
                    break;
                case ResourceType.Ore:
                    if (oreCountText != null) oreCountText.text = resource.Value.ToString();
                    break;
            }
        }
        
        if (totalResourcesText != null)
            totalResourcesText.text = $"Total: {totalResources}";
    }
    
    private void UpdateVictoryPoints()
    {
        int victoryPoints = CalculateVictoryPoints();
        if (victoryPointsText != null)
            victoryPointsText.text = $"VP: {victoryPoints}";
    }
    
    private int CalculateVictoryPoints()
    {
        if (player == null || boardManager == null) return 0;
        
        return boardManager.GetPlayerScore(player);
    }
    
    public void SetAsActivePlayer(bool isActive)
    {
        if (activePlayerIndicator != null)
            activePlayerIndicator.SetActive(isActive);
    }
}