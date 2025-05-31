using Grid;
using UnityEngine;

public class HexVertexObject : MonoBehaviour
{
    #region Serialized fields

    [SerializeField]
    private BuildingLocationSelectionObject selectionObject;

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

    private void HandleBuildingLocationSelected()
    {
        if (boardManager.TrySelectSettlementLocation(hexVertex))
        {
            EnableSelection(false);
        }
    }

    public void EnableSelection(bool enable)
    {
        if (selectionObject != null)
        {
            selectionObject.gameObject.SetActive(enable);
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

