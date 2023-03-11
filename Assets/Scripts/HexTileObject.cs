using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileObject : MonoBehaviour
{
    [SerializeField]
    private Transform northVertexTransform;
    [SerializeField]
    private Transform southVertexTransform;

    public Transform NorthVertexTransform { get { return northVertexTransform; } }
    public Transform SouthVertexTransform { get { return southVertexTransform; } }
}
