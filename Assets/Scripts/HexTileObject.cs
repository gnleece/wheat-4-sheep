using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileObject : MonoBehaviour
{
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

    public Transform NorthVertexTransform { get { return northVertexTransform; } }
    public Transform SouthVertexTransform { get { return southVertexTransform; } }


    public Transform WestEdgeTransform { get { return westEdgeTransform; } }
    public Transform NorthWestEdgeTransform { get { return northWestEdgeTransform; } }
    public Transform NorthEastEdgeTransform { get { return northEastEdgeTransform; } }

    public void SetDebugText(string text)
    {
        debugText.text = text;
    }
}
