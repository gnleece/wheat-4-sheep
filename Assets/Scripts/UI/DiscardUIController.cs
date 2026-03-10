using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiscardUIController : MonoBehaviour
{
    [Header("UI References")]
    public Button confirmDiscardButton;
    public Button[] incrementButtons;
    public Button[] decrementButtons;
    
    private IPlayer _currentPlayer;
    private ResourceHand _playerHand;
    private int _cardsToDiscard;
    private int _cardsDiscarded;
    private readonly Dictionary<ResourceType, int> _discardCounts = new();
    private TaskCompletionSource<bool> _discardCompletionSource;
    private bool _buttonsInitialized;
    
    private void Start()
    {
        // Find UI elements if not assigned
        if (confirmDiscardButton == null)
        {
            confirmDiscardButton = transform.Find("Discard Content/Discard Button - Confirm Discard")?.GetComponent<Button>();
            if (confirmDiscardButton != null)
            {
                Debug.Log("DiscardUIController: Found confirm button");
            }
            else
            {
                Debug.LogWarning("DiscardUIController: Confirm button not found!");
            }
        }
        
        if (confirmDiscardButton != null)
        {
            confirmDiscardButton.onClick.AddListener(OnConfirmDiscardClicked);
            Debug.Log("DiscardUIController: Added click listener to confirm button");
        }
        
        // Initialize resource buttons
        InitializeResourceButtons();
    }
    
    private void OnEnable()
    {
        // Re-initialize buttons when the object becomes active
        InitializeResourceButtons();
    }
    
    private void OnDestroy()
    {
        // Clean up button listeners
        if (incrementButtons != null)
        {
            for (int i = 0; i < incrementButtons.Length; i++)
            {
                if (incrementButtons[i] != null)
                {
                    incrementButtons[i].onClick.RemoveAllListeners();
                }
            }
        }
        
        if (decrementButtons != null)
        {
            for (int i = 0; i < decrementButtons.Length; i++)
            {
                if (decrementButtons[i] != null)
                {
                    decrementButtons[i].onClick.RemoveAllListeners();
                }
            }
        }
        
        if (confirmDiscardButton != null)
        {
            confirmDiscardButton.onClick.RemoveAllListeners();
        }
        
        _buttonsInitialized = false;
    }
    
    public void Initialize(IPlayer player, ResourceHand hand, int cardsToDiscard)
    {
        _currentPlayer = player;
        _playerHand = hand;
        _cardsToDiscard = cardsToDiscard;
        _cardsDiscarded = 0;
        _discardCounts.Clear();
        
        // Initialize discard counts
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (type != ResourceType.None)
            {
                _discardCounts[type] = 0;
            }
        }
        
        UpdateUI();
    }
    
    public async Task WaitForDiscardCompletion()
    {
        _discardCompletionSource = new TaskCompletionSource<bool>();
        await _discardCompletionSource.Task;
    }
    
    private void InitializeResourceButtons()
    {
        // Only initialize once
        if (_buttonsInitialized && incrementButtons != null && decrementButtons != null)
        {
            return;
        }
        
        // Remove existing listeners first to prevent duplicates
        if (incrementButtons != null)
        {
            for (int i = 0; i < incrementButtons.Length; i++)
            {
                if (incrementButtons[i] != null)
                {
                    incrementButtons[i].onClick.RemoveAllListeners();
                }
            }
        }
        
        if (decrementButtons != null)
        {
            for (int i = 0; i < decrementButtons.Length; i++)
            {
                if (decrementButtons[i] != null)
                {
                    decrementButtons[i].onClick.RemoveAllListeners();
                }
            }
        }
        
        string[] resourceNames = { "Wood", "Clay", "Sheep", "Wheat", "Ore" };
        ResourceType[] resourceTypes = { ResourceType.Wood, ResourceType.Clay, ResourceType.Sheep, ResourceType.Wheat, ResourceType.Ore };
        
        incrementButtons = new Button[resourceNames.Length];
        decrementButtons = new Button[resourceNames.Length];
        
        Debug.Log($"DiscardUIController: Initializing resource buttons for {resourceNames.Length} resources");
        
        for (int i = 0; i < resourceNames.Length; i++)
        {
            // Find increment button
            string incrementButtonPath = $"Discard Content/Resource Container/{resourceNames[i]} Item/{resourceNames[i]} Button Container/{resourceNames[i]} Increment Button";
            Transform incrementButtonTransform = transform.Find(incrementButtonPath);
            
            if (incrementButtonTransform != null)
            {
                incrementButtons[i] = incrementButtonTransform.GetComponent<Button>();
                if (incrementButtons[i] != null)
                {
                    int index = i; // Capture for closure
                    incrementButtons[i].onClick.AddListener(() => OnIncrementButtonClicked(resourceTypes[index]));
                    Debug.Log($"DiscardUIController: Found and initialized {resourceNames[i]} increment button");
                }
                else
                {
                    Debug.LogWarning($"DiscardUIController: Increment button component not found on {incrementButtonPath}");
                }
            }
            else
            {
                Debug.LogWarning($"DiscardUIController: Increment button transform not found at {incrementButtonPath}");
            }
            
            // Find decrement button
            string decrementButtonPath = $"Discard Content/Resource Container/{resourceNames[i]} Item/{resourceNames[i]} Button Container/{resourceNames[i]} Decrement Button";
            Transform decrementButtonTransform = transform.Find(decrementButtonPath);
            
            if (decrementButtonTransform != null)
            {
                decrementButtons[i] = decrementButtonTransform.GetComponent<Button>();
                if (decrementButtons[i] != null)
                {
                    int index = i; // Capture for closure
                    decrementButtons[i].onClick.AddListener(() => OnDecrementButtonClicked(resourceTypes[index]));
                    Debug.Log($"DiscardUIController: Found and initialized {resourceNames[i]} decrement button");
                }
                else
                {
                    Debug.LogWarning($"DiscardUIController: Decrement button component not found on {decrementButtonPath}");
                }
            }
            else
            {
                Debug.LogWarning($"DiscardUIController: Decrement button transform not found at {decrementButtonPath}");
            }
        }
        
        _buttonsInitialized = true;
        Debug.Log($"DiscardUIController: Initialized {incrementButtons.Count(b => b != null)} increment and {decrementButtons.Count(b => b != null)} decrement buttons out of {resourceNames.Length} resources");
    }
    
    private void OnIncrementButtonClicked(ResourceType resourceType)
    {
        Debug.Log($"DiscardUIController: Increment button clicked for {resourceType}");
        
        if (_cardsDiscarded >= _cardsToDiscard)
        {
            Debug.Log("Already discarded enough cards!");
            return;
        }
        
        // Check if player has this resource
        int currentCount = _playerHand.GetCount(resourceType);
        int alreadyDiscarded = _discardCounts[resourceType];
        
        if (alreadyDiscarded >= currentCount)
        {
            Debug.Log($"No more {resourceType} to discard!");
            return;
        }
        
        // Add to discard count
        _discardCounts[resourceType]++;
        _cardsDiscarded++;
        
        Debug.Log($"Added 1 {resourceType} to discard ({_cardsDiscarded}/{_cardsToDiscard})");
        
        UpdateUI();
    }
    
    private void OnDecrementButtonClicked(ResourceType resourceType)
    {
        Debug.Log($"DiscardUIController: Decrement button clicked for {resourceType}");
        
        int alreadyDiscarded = _discardCounts[resourceType];
        
        if (alreadyDiscarded <= 0)
        {
            Debug.Log($"No {resourceType} to remove from discard!");
            return;
        }
        
        // Remove from discard count
        _discardCounts[resourceType]--;
        _cardsDiscarded--;
        
        Debug.Log($"Removed 1 {resourceType} from discard ({_cardsDiscarded}/{_cardsToDiscard})");
        
        UpdateUI();
    }
    
    private void OnConfirmDiscardClicked()
    {
        Debug.Log("DiscardUIController: Confirm discard button clicked!");
        
        if (_cardsDiscarded != _cardsToDiscard)
        {
            Debug.Log($"Must discard exactly {_cardsToDiscard} cards! Currently discarded: {_cardsDiscarded}");
            return;
        }
        
        // Apply the discard to the player's hand
        foreach (var kvp in _discardCounts)
        {
            if (kvp.Value > 0)
            {
                _playerHand.Remove(kvp.Key, kvp.Value);
                Debug.Log($"Removed {kvp.Value} {kvp.Key} from player's hand");
            }
        }
        
        Debug.Log($"Player {_currentPlayer.PlayerId} completed discarding {_cardsDiscarded} cards");
        
        // Complete the discard
        _discardCompletionSource?.SetResult(true);
    }
    
    private void UpdateUI()
    {
        // Ensure resource buttons are initialized (only if not already done)
        if (incrementButtons == null || decrementButtons == null)
        {
            InitializeResourceButtons();
        }
        
        // Update resource counts display
        string[] resourceNames = { "Wood", "Clay", "Sheep", "Wheat", "Ore" };
        ResourceType[] resourceTypes = { ResourceType.Wood, ResourceType.Clay, ResourceType.Sheep, ResourceType.Wheat, ResourceType.Ore };
        
        for (int i = 0; i < resourceNames.Length; i++)
        {
            // Update current count
            Transform countTransform = transform.Find($"Discard Content/Resource Container/{resourceNames[i]} Item/{resourceNames[i]} Count");
            if (countTransform != null)
            {
                TextMeshProUGUI countText = countTransform.GetComponent<TextMeshProUGUI>();
                if (countText != null)
                {
                    int currentCount = _playerHand.GetCount(resourceTypes[i]);
                    int alreadyDiscarded = _discardCounts[resourceTypes[i]];
                    countText.text = (currentCount - alreadyDiscarded).ToString();
                }
            }
            
            // Update discard count
            Transform discardTransform = transform.Find($"Discard Content/Resource Container/{resourceNames[i]} Item/{resourceNames[i]} Discard Count");
            if (discardTransform != null)
            {
                TextMeshProUGUI discardText = discardTransform.GetComponent<TextMeshProUGUI>();
                if (discardText != null)
                {
                    discardText.text = _discardCounts[resourceTypes[i]].ToString();
                }
            }
        }
        
        // Update progress text
        Transform progressTransform = transform.Find("Discard Content/Progress Text");
        if (progressTransform != null)
        {
            TextMeshProUGUI progressText = progressTransform.GetComponent<TextMeshProUGUI>();
            if (progressText != null)
            {
                progressText.text = $"Discarded: {_cardsDiscarded}/{_cardsToDiscard}";
            }
        }
        
        // Update confirm button state
        if (confirmDiscardButton != null)
        {
            confirmDiscardButton.interactable = (_cardsDiscarded == _cardsToDiscard);
        }
        
        // Update increment button states
        if (incrementButtons != null)
        {
            for (int i = 0; i < incrementButtons.Length; i++)
            {
                if (incrementButtons[i] != null)
                {
                    int currentCount = _playerHand.GetCount(resourceTypes[i]);
                    int alreadyDiscarded = _discardCounts[resourceTypes[i]];
                    incrementButtons[i].interactable = (_cardsDiscarded < _cardsToDiscard && alreadyDiscarded < currentCount);
                }
            }
        }
        
        // Update decrement button states
        if (decrementButtons != null)
        {
            for (int i = 0; i < decrementButtons.Length; i++)
            {
                if (decrementButtons[i] != null)
                {
                    int alreadyDiscarded = _discardCounts[resourceTypes[i]];
                    decrementButtons[i].interactable = (alreadyDiscarded > 0);
                }
            }
        }
    }
}
