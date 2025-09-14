using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSelectionUIController : MonoBehaviour
{
    [Header("UI References")]
    public Button[] playerButtons;
    public TextMeshProUGUI instructionText;
    
    private IPlayer currentPlayer;
    private List<IPlayer> availablePlayers;
    private TaskCompletionSource<IPlayer> selectionCompletionSource;
    private bool buttonsInitialized = false;
    
    private void Start()
    {
        // Find UI elements if not assigned
        if (instructionText == null)
        {
            instructionText = transform.Find("Player Selection Content/Instruction Text")?.GetComponent<TextMeshProUGUI>();
        }
        
        // Initialize player buttons
        InitializePlayerButtons();
    }
    
    private void OnEnable()
    {
        // Re-initialize buttons when the object becomes active
        InitializePlayerButtons();
    }
    
    private void OnDestroy()
    {
        // Clean up button listeners
        if (playerButtons != null)
        {
            for (int i = 0; i < playerButtons.Length; i++)
            {
                if (playerButtons[i] != null)
                {
                    playerButtons[i].onClick.RemoveAllListeners();
                }
            }
        }
        
        buttonsInitialized = false;
    }
    
    public void Initialize(IPlayer currentPlayer, List<IPlayer> availablePlayers)
    {
        this.currentPlayer = currentPlayer;
        this.availablePlayers = availablePlayers;
        
        UpdateUI();
    }
    
    public async Task<IPlayer> WaitForPlayerSelection()
    {
        selectionCompletionSource = new TaskCompletionSource<IPlayer>();
        return await selectionCompletionSource.Task;
    }
    
    private void InitializePlayerButtons()
    {
        // Only initialize once
        if (buttonsInitialized && playerButtons != null)
            return;
            
        // Find player buttons if not assigned
        if (playerButtons == null || playerButtons.Length == 0)
        {
            var buttonContainer = transform.Find("Player Selection Content/Player Buttons Container");
            if (buttonContainer != null)
            {
                var buttons = new List<Button>();
                for (int i = 0; i < buttonContainer.childCount; i++)
                {
                    var button = buttonContainer.GetChild(i).GetComponent<Button>();
                    if (button != null)
                    {
                        buttons.Add(button);
                    }
                }
                playerButtons = buttons.ToArray();
            }
        }
        
        // Add click listeners to buttons
        if (playerButtons != null)
        {
            for (int i = 0; i < playerButtons.Length; i++)
            {
                if (playerButtons[i] != null)
                {
                    int playerIndex = i; // Capture for closure
                    playerButtons[i].onClick.AddListener(() => OnPlayerButtonClicked(playerIndex));
                }
            }
        }
        
        buttonsInitialized = true;
    }
    
    private void OnPlayerButtonClicked(int playerIndex)
    {
        Debug.Log($"PlayerSelectionUIController: Player button {playerIndex} clicked");
        
        if (availablePlayers == null || playerIndex >= availablePlayers.Count)
        {
            Debug.LogError($"Invalid player index: {playerIndex}");
            return;
        }
        
        IPlayer selectedPlayer = availablePlayers[playerIndex];
        Debug.Log($"Player {currentPlayer.PlayerId} selected player {selectedPlayer.PlayerId}");
        
        // Complete the selection
        selectionCompletionSource?.SetResult(selectedPlayer);
    }
    
    private void UpdateUI()
    {
        // Update instruction text
        if (instructionText != null)
        {
            instructionText.text = $"Player {currentPlayer.PlayerId + 1}, select a player to steal from:";
        }
        
        // Update player buttons
        if (playerButtons != null && availablePlayers != null)
        {
            for (int i = 0; i < playerButtons.Length; i++)
            {
                if (playerButtons[i] != null)
                {
                    bool shouldShow = i < availablePlayers.Count;
                    playerButtons[i].gameObject.SetActive(shouldShow);
                    
                    if (shouldShow)
                    {
                        // Update button text
                        var buttonText = playerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = $"Player {availablePlayers[i].PlayerId + 1}";
                        }
                        
                        // Enable/disable button
                        playerButtons[i].interactable = true;
                    }
                }
            }
        }
    }
}
