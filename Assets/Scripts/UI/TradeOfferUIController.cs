using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeOfferUIController : MonoBehaviour
{
    private TaskCompletionSource<bool> _completionSource;
    private Button _acceptButton;
    private Button _declineButton;

    public void Initialize(IPlayer receivingPlayer, TradeOffer offer)
    {
        UpdateUI(receivingPlayer, offer);
    }

    public async Task<bool> WaitForResponse()
    {
        _completionSource = new TaskCompletionSource<bool>();
        return await _completionSource.Task;
    }

    public void SetupButtons(Button acceptButton, Button declineButton)
    {
        _acceptButton = acceptButton;
        _declineButton = declineButton;

        acceptButton.onClick.AddListener(OnAcceptClicked);
        declineButton.onClick.AddListener(OnDeclineClicked);
    }

    private void UpdateUI(IPlayer receivingPlayer, TradeOffer offer)
    {
        var instructionText = transform.Find("Trade Offer Content/Instruction Text")?.GetComponent<TextMeshProUGUI>();
        if (instructionText != null)
        {
            instructionText.text = $"Player {receivingPlayer.PlayerId + 1}, Player {offer.Initiator.PlayerId + 1} wants to trade:";
        }

        var offeringText = transform.Find("Trade Offer Content/Offering Text")?.GetComponent<TextMeshProUGUI>();
        if (offeringText != null)
        {
            offeringText.text = "They offer: " + FormatResources(offer.Offering);
        }

        var requestingText = transform.Find("Trade Offer Content/Requesting Text")?.GetComponent<TextMeshProUGUI>();
        if (requestingText != null)
        {
            requestingText.text = "They want: " + FormatResources(offer.Requesting);
        }
    }

    private void OnAcceptClicked()
    {
        Debug.Log("TradeOfferUIController: Trade accepted");
        _completionSource?.TrySetResult(true);
    }

    private void OnDeclineClicked()
    {
        Debug.Log("TradeOfferUIController: Trade declined");
        _completionSource?.TrySetResult(false);
    }

    private static string FormatResources(Dictionary<ResourceType, int> resources)
    {
        var parts = new List<string>();
        foreach (var kvp in resources)
        {
            if (kvp.Value > 0)
            {
                parts.Add($"{kvp.Value} {kvp.Key}");
            }
        }

        return parts.Count > 0 ? string.Join(", ", parts) : "nothing";
    }

    private void OnDestroy()
    {
        if (_acceptButton != null) _acceptButton.onClick.RemoveAllListeners();
        if (_declineButton != null) _declineButton.onClick.RemoveAllListeners();
    }
}
