using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grid;
using UnityEngine.UI;

public class HexTileObject : MonoBehaviour
{
    #region Serialized fields

    [SerializeField]
    private TileType tileType;

    [SerializeField]
    private Transform northVertexTransform;
    [SerializeField]
    private Transform southVertexTransform;

    [SerializeField]
    private Transform westEdgeTransform;
    [SerializeField]
    private Transform northWestEdgeTransform;
    [SerializeField]
    private Transform northEastEdgeTransform;

    [SerializeField]
    private HexTileSelectionObject selectionObject;

    [SerializeField]
    private TextMesh debugText;

    #endregion

    #region Properties

    public TileType TileType => tileType;

    public int? DiceNumber { get; set; } = null;

    public Transform NorthVertexTransform => northVertexTransform;
    public Transform SouthVertexTransform => southVertexTransform;

    public Transform WestEdgeTransform => westEdgeTransform;
    public Transform NorthWestEdgeTransform => northWestEdgeTransform;
    public Transform NorthEastEdgeTransform => northEastEdgeTransform;

    #endregion

    #region Private fields

    private IBoardManager boardManager;
    private HexTile hexTile;

    #endregion

    public void Initialize(IBoardManager boardManager, HexTile hexTile, int? diceNumber)
    {
        this.boardManager = boardManager;
        this.hexTile = hexTile;
        this.DiceNumber = diceNumber;

        if (selectionObject != null)
        {
            selectionObject.Initialize(hexTile);
            selectionObject.OnHexTileSelected += HexTileSelected;
        }

        RefreshDebugText();
    }

    private void OnEnable()
    {
        if (DebugSettings.Instance != null)
        {
            DebugSettings.Instance.OnSettingsChanged += RefreshDebugText;
        }
    }

    private void OnDisable()
    {
        if (DebugSettings.Instance != null)
        {
            DebugSettings.Instance.OnSettingsChanged -= RefreshDebugText;
        }
    }

    private void HexTileSelected()
    {
        boardManager.CompleteSelection(hexTile);
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

    public void RefreshDebugText()
    {
        if (debugText != null && hexTile != null)
        {
            var text = "";
            if (DebugSettings.Instance.ShowHexCoordinates)
            {
                text += $"{hexTile.HexCoordinates}\n";
            }
            if (DiceNumber.HasValue)
            {
                text += $"{DiceNumber}";
            }
            debugText.text = text;
        }
    }
}
