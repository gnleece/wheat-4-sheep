using Grid;
using UnityEngine;

public class HexVertexObject : MonoBehaviour
{
    #region Serialized fields

    [SerializeField]
    private BuildingLocationSelectionObject selectionObject;

    #endregion

    #region Public methods

    public void Initialize(IBoardManager boardManager, HexVertex hexVertex)
    {
        if (selectionObject != null)
        {
            selectionObject.Initialize(boardManager, hexVertex);
        }

        EnableSelection(false);
    }

    public void EnableSelection(bool enable)
    {
        if (selectionObject != null)
        {
            selectionObject.gameObject.SetActive(enable);
        }
    }

    #endregion
}
