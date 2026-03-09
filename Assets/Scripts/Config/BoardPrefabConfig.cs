using UnityEngine;

/// <summary>
/// ScriptableObject that holds all prefab references needed to build the board.
/// </summary>
[CreateAssetMenu(fileName = "BoardPrefabConfig", menuName = "Wheat4Sheep/Board Prefab Config")]
public class BoardPrefabConfig : ScriptableObject
{
    public GameObject WoodTilePrefab;
    public GameObject ClayTilePrefab;
    public GameObject SheepTilePrefab;
    public GameObject WheatTilePrefab;
    public GameObject OreTilePrefab;
    public GameObject DesertTilePrefab;
    public GameObject WaterTilePrefab;
    public GameObject HexVertexPrefab;
    public GameObject HexEdgePrefab;
    public GameObject RobberPrefab;
    public GameObject PortIndicatorPrefab;
    public GameObject PortVertexIndicatorPrefab;
}
