using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DevCardSelectionUIController : MonoBehaviour
{
    private IPlayer currentPlayer;
    private Dictionary<DevelopmentCardType, int> availableCards;
    private TaskCompletionSource<DevelopmentCardType> selectionCompletionSource;

    private List<Button> cardButtons = new List<Button>();

    public void Initialize(IPlayer player, Dictionary<DevelopmentCardType, int> hand)
    {
        currentPlayer = player;

        // Only show cards that can be actively played (exclude VP cards, which are passive)
        availableCards = new Dictionary<DevelopmentCardType, int>();
        foreach (var kvp in hand)
        {
            if (kvp.Key != DevelopmentCardType.VictoryPoint && kvp.Value > 0)
            {
                availableCards[kvp.Key] = kvp.Value;
            }
        }

        UpdateUI();
    }

    public async Task<DevelopmentCardType> WaitForCardSelection()
    {
        selectionCompletionSource = new TaskCompletionSource<DevelopmentCardType>();
        return await selectionCompletionSource.Task;
    }

    private void UpdateUI()
    {
        // Update instruction text
        var instructionText = transform.Find("Dev Card Selection Content/Instruction Text")?.GetComponent<TextMeshProUGUI>();
        if (instructionText != null)
        {
            instructionText.text = $"Player {currentPlayer.PlayerId + 1}, choose a card to play:";
        }

        // Clear existing card buttons
        foreach (var btn in cardButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        cardButtons.Clear();

        // Create a button for each available card type
        var buttonContainer = transform.Find("Dev Card Selection Content/Card Buttons Container");
        if (buttonContainer == null) return;

        foreach (var kvp in availableCards)
        {
            var cardType = kvp.Key;
            var count = kvp.Value;

            var buttonObj = new GameObject($"{cardType} Button");
            buttonObj.transform.SetParent(buttonContainer, false);

            var rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160f, 50f);

            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.4f, 0.7f, 1f);

            var button = buttonObj.AddComponent<Button>();
            var capturedCardType = cardType;
            button.onClick.AddListener(() => OnCardButtonClicked(capturedCardType));

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = $"{cardType} (x{count})";
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            cardButtons.Add(button);
        }
    }

    private void OnCardButtonClicked(DevelopmentCardType cardType)
    {
        Debug.Log($"DevCardSelectionUIController: {cardType} selected");
        selectionCompletionSource?.TrySetResult(cardType);
    }

    private void OnDestroy()
    {
        foreach (var btn in cardButtons)
        {
            if (btn != null) btn.onClick.RemoveAllListeners();
        }
    }
}
