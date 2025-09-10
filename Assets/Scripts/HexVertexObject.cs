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
        // Check the current board mode to determine which selection method to call
        if (boardManager is BoardManager bm)
        {
            var currentMode = bm.GetCurrentBoardMode();
            switch (currentMode)
            {
                case BoardMode.ChooseSettlementLocation:
                    boardManager.ManualSettlementLocationSelected(hexVertex);
                    break;
                case BoardMode.ChooseSettlementToUpgrade:
                    boardManager.ManualSettlementUpgradeLocationSelected(hexVertex);
                    break;
                default:
                    // Fallback to settlement selection for backward compatibility
                    boardManager.ManualSettlementLocationSelected(hexVertex);
                    break;
            }
        }
        else
        {
            // Fallback for interface compatibility
            boardManager.ManualSettlementLocationSelected(hexVertex);
        }
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

