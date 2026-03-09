using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class TradeSelection { }

public sealed class BankTradeSelection : TradeSelection
{
    public ResourceType Giving;
    public ResourceType Receiving;
}

public sealed class PlayerTradeSelection : TradeSelection
{
    public Dictionary<ResourceType, int> Offering;
    public Dictionary<ResourceType, int> Requesting;
}

public class TradeUIController : MonoBehaviour
{
    private static readonly ResourceType[] ResourceTypes =
    {
        ResourceType.Wood,
        ResourceType.Clay,
        ResourceType.Sheep,
        ResourceType.Wheat,
        ResourceType.Ore,
    };

    private IPlayer _currentPlayer;
    private IBoardManager _boardManager;
    private TaskCompletionSource<TradeSelection> _completionSource;

    // Bank trade state
    private ResourceType _bankGiving = ResourceType.None;
    private ResourceType _bankReceiving = ResourceType.None;
    private List<Button> _bankGivingButtons = new List<Button>();
    private List<Button> _bankReceivingButtons = new List<Button>();
    private TextMeshProUGUI _bankRateLabel;
    private Button _bankConfirmButton;

    // Player trade state
    private Dictionary<ResourceType, int> _playerOffering = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, int> _playerRequesting = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, TextMeshProUGUI> _offeringCountLabels = new Dictionary<ResourceType, TextMeshProUGUI>();
    private Dictionary<ResourceType, TextMeshProUGUI> _requestingCountLabels = new Dictionary<ResourceType, TextMeshProUGUI>();
    private Button _proposeButton;

    public void Initialize(IPlayer player, IBoardManager boardManager)
    {
        _currentPlayer = player;
        _boardManager = boardManager;

        _bankGiving = ResourceType.None;
        _bankReceiving = ResourceType.None;

        foreach (var type in ResourceTypes)
        {
            _playerOffering[type] = 0;
            _playerRequesting[type] = 0;
        }

        UpdateBankTradeUI();
        UpdatePlayerTradeUI();
    }

    public async Task<TradeSelection> WaitForSelection()
    {
        _completionSource = new TaskCompletionSource<TradeSelection>();
        return await _completionSource.Task;
    }

    // Called by UISetup to wire up buttons after building the hierarchy
    public void SetupBankTradeButtons(
        List<Button> givingButtons,
        List<Button> receivingButtons,
        TextMeshProUGUI rateLabel,
        Button confirmButton)
    {
        _bankGivingButtons = givingButtons;
        _bankReceivingButtons = receivingButtons;
        _bankRateLabel = rateLabel;
        _bankConfirmButton = confirmButton;

        for (int i = 0; i < givingButtons.Count; i++)
        {
            var resourceType = ResourceTypes[i];
            givingButtons[i].onClick.AddListener(() => OnBankGivingSelected(resourceType));
        }

        for (int i = 0; i < receivingButtons.Count; i++)
        {
            var resourceType = ResourceTypes[i];
            receivingButtons[i].onClick.AddListener(() => OnBankReceivingSelected(resourceType));
        }

        confirmButton.onClick.AddListener(OnBankConfirmClicked);
    }

    public void SetupPlayerTradeButtons(
        Dictionary<ResourceType, Button> offeringPlusButtons,
        Dictionary<ResourceType, Button> offeringMinusButtons,
        Dictionary<ResourceType, TextMeshProUGUI> offeringCountLabels,
        Dictionary<ResourceType, Button> requestingPlusButtons,
        Dictionary<ResourceType, Button> requestingMinusButtons,
        Dictionary<ResourceType, TextMeshProUGUI> requestingCountLabels,
        Button proposeButton)
    {
        _offeringCountLabels = offeringCountLabels;
        _requestingCountLabels = requestingCountLabels;
        _proposeButton = proposeButton;

        foreach (var type in ResourceTypes)
        {
            var capturedType = type;
            offeringPlusButtons[type].onClick.AddListener(() => OnOfferingChanged(capturedType, +1));
            offeringMinusButtons[type].onClick.AddListener(() => OnOfferingChanged(capturedType, -1));
            requestingPlusButtons[type].onClick.AddListener(() => OnRequestingChanged(capturedType, +1));
            requestingMinusButtons[type].onClick.AddListener(() => OnRequestingChanged(capturedType, -1));
        }

        proposeButton.onClick.AddListener(OnProposeClicked);
    }

    public void SetupCancelButton(Button cancelButton)
    {
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private void OnBankGivingSelected(ResourceType resourceType)
    {
        _bankGiving = resourceType;
        UpdateBankTradeUI();
    }

    private void OnBankReceivingSelected(ResourceType resourceType)
    {
        _bankReceiving = resourceType;
        UpdateBankTradeUI();
    }

    private void OnBankConfirmClicked()
    {
        if (_boardManager == null || !_boardManager.CanBankTrade(_currentPlayer, _bankGiving, _bankReceiving))
        {
            return;
        }

        _completionSource?.TrySetResult(new BankTradeSelection
        {
            Giving = _bankGiving,
            Receiving = _bankReceiving,
        });
    }

    private void OnOfferingChanged(ResourceType resourceType, int delta)
    {
        var hand = _boardManager.GetResourceHandForPlayer(_currentPlayer);
        int currentCount = _playerOffering[resourceType];
        int inHand = hand.TryGetValue(resourceType, out int handCount) ? handCount : 0;

        _playerOffering[resourceType] = Mathf.Clamp(currentCount + delta, 0, inHand);
        UpdatePlayerTradeUI();
    }

    private void OnRequestingChanged(ResourceType resourceType, int delta)
    {
        int currentCount = _playerRequesting[resourceType];
        _playerRequesting[resourceType] = Mathf.Max(0, currentCount + delta);
        UpdatePlayerTradeUI();
    }

    private void OnProposeClicked()
    {
        _completionSource?.TrySetResult(new PlayerTradeSelection
        {
            Offering = new Dictionary<ResourceType, int>(_playerOffering),
            Requesting = new Dictionary<ResourceType, int>(_playerRequesting),
        });
    }

    private void OnCancelClicked()
    {
        _completionSource?.TrySetResult(null);
    }

    private void UpdateBankTradeUI()
    {
        if (_boardManager == null || _currentPlayer == null)
        {
            return;
        }

        var hand = _boardManager.GetResourceHandForPlayer(_currentPlayer);

        for (int i = 0; i < ResourceTypes.Length && i < _bankGivingButtons.Count; i++)
        {
            var type = ResourceTypes[i];
            int rate = _boardManager.GetBankTradeRate(_currentPlayer, type);
            int count = hand != null && hand.TryGetValue(type, out int c) ? c : 0;
            bool canAfford = count >= rate;

            var btn = _bankGivingButtons[i];
            btn.interactable = canAfford;

            var colors = btn.colors;
            colors.normalColor = type == _bankGiving
                ? new Color(0.2f, 0.6f, 0.2f)
                : new Color(0.3f, 0.3f, 0.3f);
            btn.colors = colors;

            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = $"{type}\n({count})";
            }
        }

        if (_bankRateLabel != null && _bankGiving != ResourceType.None)
        {
            int rate = _boardManager.GetBankTradeRate(_currentPlayer, _bankGiving);
            _bankRateLabel.text = $"Rate: {rate}:1";
        }
        else if (_bankRateLabel != null)
        {
            _bankRateLabel.text = "Select resource to give";
        }

        for (int i = 0; i < ResourceTypes.Length && i < _bankReceivingButtons.Count; i++)
        {
            var type = ResourceTypes[i];
            var btn = _bankReceivingButtons[i];
            btn.interactable = type != _bankGiving;

            var colors = btn.colors;
            colors.normalColor = type == _bankReceiving
                ? new Color(0.2f, 0.4f, 0.7f)
                : new Color(0.3f, 0.3f, 0.3f);
            btn.colors = colors;
        }

        if (_bankConfirmButton != null)
        {
            _bankConfirmButton.interactable = _boardManager.CanBankTrade(_currentPlayer, _bankGiving, _bankReceiving);
        }
    }

    private void UpdatePlayerTradeUI()
    {
        foreach (var type in ResourceTypes)
        {
            if (_offeringCountLabels.TryGetValue(type, out var offerLabel))
            {
                offerLabel.text = _playerOffering[type].ToString();
            }

            if (_requestingCountLabels.TryGetValue(type, out var requestLabel))
            {
                requestLabel.text = _playerRequesting[type].ToString();
            }
        }

        if (_proposeButton != null)
        {
            bool hasOffering = false;
            bool hasRequesting = false;

            foreach (var type in ResourceTypes)
            {
                if (_playerOffering[type] > 0) hasOffering = true;
                if (_playerRequesting[type] > 0) hasRequesting = true;
            }

            _proposeButton.interactable = hasOffering && hasRequesting;
        }
    }

    private void OnDestroy()
    {
        foreach (var btn in _bankGivingButtons)
        {
            if (btn != null) btn.onClick.RemoveAllListeners();
        }

        foreach (var btn in _bankReceivingButtons)
        {
            if (btn != null) btn.onClick.RemoveAllListeners();
        }

        if (_bankConfirmButton != null) _bankConfirmButton.onClick.RemoveAllListeners();
        if (_proposeButton != null) _proposeButton.onClick.RemoveAllListeners();
    }
}
