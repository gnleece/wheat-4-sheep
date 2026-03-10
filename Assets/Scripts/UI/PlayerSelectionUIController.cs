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
    
    private IPlayer _currentPlayer;
    private List<IPlayer> _availablePlayers;
    private TaskCompletionSource<IPlayer> _selectionCompletionSource;
    private bool _buttonsInitialized;
    
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
        
        _buttonsInitialized = false;
    }
    
    public void Initialize(IPlayer currentPlayer, List<IPlayer> availablePlayers)
    {
        this._currentPlayer = currentPlayer;
        this._availablePlayers = availablePlayers;
        
        UpdateUI();
    }
    
    public async Task<IPlayer> WaitForPlayerSelection()
    {
        _selectionCompletionSource = new TaskCompletionSource<IPlayer>();
        return await _selectionCompletionSource.Task;
    }
    
    private void InitializePlayerButtons()
    {
        // Only initialize once
        if (_buttonsInitialized && playerButtons != null)
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
        
        _buttonsInitialized = true;
    }
    
    private void OnPlayerButtonClicked(int playerIndex)
    {
        Debug.Log($"PlayerSelectionUIController: Player button {playerIndex} clicked");
        
        if (_availablePlayers == null || playerIndex >= _availablePlayers.Count)
        {
            Debug.LogError($"Invalid player index: {playerIndex}");
            return;
        }
        
        IPlayer selectedPlayer = _availablePlayers[playerIndex];
        Debug.Log($"Player {_currentPlayer.PlayerId} selected player {selectedPlayer.PlayerId}");
        
        // Complete the selection
        _selectionCompletionSource?.SetResult(selectedPlayer);
    }
    
    private void UpdateUI()
    {
        // Update instruction text
        if (instructionText != null)
        {
            instructionText.text = $"Player {_currentPlayer.PlayerId + 1}, select a player to steal from:";
        }
        
        // Update player buttons
        if (playerButtons != null && _availablePlayers != null)
        {
            for (int i = 0; i < playerButtons.Length; i++)
            {
                if (playerButtons[i] != null)
                {
                    bool shouldShow = i < _availablePlayers.Count;
                    playerButtons[i].gameObject.SetActive(shouldShow);
                    
                    if (shouldShow)
                    {
                        // Update button text
                        var buttonText = playerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = $"Player {_availablePlayers[i].PlayerId + 1}";
                        }
                        
                        // Enable/disable button
                        playerButtons[i].interactable = true;
                    }
                }
            }
        }
    }
}
