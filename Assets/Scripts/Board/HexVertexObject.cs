using Grid;
using UnityEngine;

public class HexVertexObject : MonoBehaviour
{
    #region Serialized fields

    [SerializeField]
    private BuildingLocationSelectionObject selectionObject;

    [SerializeField]
    private SettlementObject settlementObject;

    [SerializeField]
    private CityObject cityObject;

    #endregion

    #region Private fields

    private IBoardManager _boardManager;
    private HexVertex _hexVertex;

    #endregion

    #region Public methods

    public void Initialize(IBoardManager boardManager, HexVertex hexVertex)
    {
        _boardManager = boardManager;
        _hexVertex = hexVertex;
        
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
            settlementObject.Refresh(_hexVertex.Building);
        }

        if (cityObject != null)
        {
            cityObject.Refresh(_hexVertex.Building);
        }
    }

    private void HandleBuildingLocationSelected()
    {
        _boardManager.CompleteSelection(_hexVertex);
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

