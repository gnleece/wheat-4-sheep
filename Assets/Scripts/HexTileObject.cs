using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void SetDebugText(string text)
    {
        debugText.text = text;
    }
}
