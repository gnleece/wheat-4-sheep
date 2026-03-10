using UnityEngine;

public class HexEdgeObject : MonoBehaviour
{
    [SerializeField]
    private RoadLocationSelectionObject selectionObject;

    [SerializeField]
    private RoadObject roadObject;

    private IBoardManager _boardManager;
    private HexEdge _hexEdge;

    public void Initialize(IBoardManager boardManager, HexEdge hexEdge)
    {
        _boardManager = boardManager;
        _hexEdge = hexEdge;

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
            roadObject.Refresh(_hexEdge.Road);
        }
    }

    private void HandleRoadLocationSelected()
    {
        _boardManager.CompleteSelection(_hexEdge);
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
