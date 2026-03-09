using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceTypeSelectionUIController : MonoBehaviour
{
    private static readonly ResourceType[] SelectableResources =
    {
        ResourceType.Wood,
        ResourceType.Clay,
        ResourceType.Sheep,
        ResourceType.Wheat,
        ResourceType.Ore,
    };

    private TaskCompletionSource<ResourceType> selectionCompletionSource;

    public void Initialize(IPlayer player, string prompt)
    {
        var instructionText = transform.Find("Resource Selection Content/Instruction Text")?.GetComponent<TextMeshProUGUI>();
        if (instructionText != null)
        {
            instructionText.text = $"Player {player.PlayerId + 1}: {prompt}";
        }

        SetupResourceButtons();
    }

    public async Task<ResourceType> WaitForResourceTypeSelection()
    {
        selectionCompletionSource = new TaskCompletionSource<ResourceType>();
        return await selectionCompletionSource.Task;
    }

    private void SetupResourceButtons()
    {
        var buttonContainer = transform.Find("Resource Selection Content/Resource Buttons Container");
        if (buttonContainer == null) return;

        // Wire up existing buttons (created by UISetup) to resource types
        for (int i = 0; i < SelectableResources.Length; i++)
        {
            int index = i;
            var childTransform = buttonContainer.childCount > i ? buttonContainer.GetChild(i) : null;
            if (childTransform == null) continue;

            var button = childTransform.GetComponent<Button>();
            if (button == null) continue;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnResourceButtonClicked(SelectableResources[index]));

            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = SelectableResources[i].ToString();
            }
        }
    }

    private void OnResourceButtonClicked(ResourceType resourceType)
    {
        Debug.Log($"ResourceTypeSelectionUIController: {resourceType} selected");
        selectionCompletionSource?.TrySetResult(resourceType);
    }

    private void OnDisable()
    {
        // If the modal is hidden before a selection is made, cancel the task
        selectionCompletionSource?.TrySetCanceled();
    }
}
