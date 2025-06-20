using UnityEngine;

public class HexEdgeObject : MonoBehaviour
{
    [SerializeField]
    private RoadLocationSelectionObject selectionObject;

    [SerializeField]
    private RoadObject roadObject;

    private IBoardManager boardManager;
    private HexEdge hexEdge;

    public void Initialize(IBoardManager boardManager, HexEdge hexEdge)
    {
        this.boardManager = boardManager;
        this.hexEdge = hexEdge;

        if (selectionObject != null)
        {
            selectionObject.Initialize(hexEdge);
            selectionObject.OnRoadLocationSelected += HandleRoadLocationSelected;
        }

        Refresh();
    }

    public void Refresh()
    {
        if (roadObject != null)
        {
            roadObject.Refresh(hexEdge.Road);
        }
    }

    private void HandleRoadLocationSelected()
    {
        boardManager.ManualRoadLocationSelected(hexEdge);
    }

    public void EnableSelection(bool enable, Color? hoverColor = null)
    {
        if (selectionObject != null)
        {
            selectionObject.gameObject.SetActive(enable);
            if (enable && hoverColor.HasValue)
            {
                selectionObject.SetHoverColor(hoverColor.Value);
            }
        }
    }

    private void OnDestroy()
    {
        if (selectionObject != null)
        {
            selectionObject.OnRoadLocationSelected -= HandleRoadLocationSelected;
        }
    }
}
