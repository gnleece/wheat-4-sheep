using Grid;
using UnityEngine;

public class HexVertexObject : MonoBehaviour
{
    #region Serialized fields

    [SerializeField]
    private BuildingLocationSelectionObject selectionObject;

    [SerializeField]
    private SettlementObject settlementObject;

    #endregion

    #region Private fields

    private IBoardManager boardManager;
    private HexVertex hexVertex;

    #endregion

    #region Public methods

    public void Initialize(IBoardManager boardManager, HexVertex hexVertex)
    {
        this.boardManager = boardManager;
        this.hexVertex = hexVertex;
        if (selectionObject != null)
        {
            selectionObject.Initialize(hexVertex);
            selectionObject.OnBuildingLocationSelected += HandleBuildingLocationSelected;
        }

        EnableSelection(false);
    }

    public void Refresh()
    {
        if (settlementObject != null)
        {
            settlementObject.Refresh(hexVertex.Building);
        }
    }

    private void HandleBuildingLocationSelected()
    {
        boardManager.ManualSettlementLocationSelected(hexVertex);
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
            selectionObject.OnBuildingLocationSelected -= HandleBuildingLocationSelected;
        }
    }

    #endregion
}

