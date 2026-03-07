using Grid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles creation of the hex grid data structures and their corresponding Unity GameObjects,
/// including neighbor wiring for tiles, vertices, and edges.
/// </summary>
public class GridInitializer
{
    #region Private members

    private readonly GameConfig gameConfig;
    private readonly System.Random random;
    private readonly float horizontalSpacing;
    private readonly float verticalSpacing;

    private readonly GameObject woodTilePrefab;
    private readonly GameObject clayTilePrefab;
    private readonly GameObject sheepTilePrefab;
    private readonly GameObject wheatTilePrefab;
    private readonly GameObject oreTilePrefab;
    private readonly GameObject desertTilePrefab;
    private readonly GameObject waterTilePrefab;
    private readonly GameObject hexVertexPrefab;
    private readonly GameObject hexEdgePrefab;
    private readonly GameObject robberPrefab;

    #endregion

    public GridInitializer(
        GameConfig gameConfig,
        System.Random random,
        float horizontalSpacing,
        float verticalSpacing,
        GameObject woodTilePrefab,
        GameObject clayTilePrefab,
        GameObject sheepTilePrefab,
        GameObject wheatTilePrefab,
        GameObject oreTilePrefab,
        GameObject desertTilePrefab,
        GameObject waterTilePrefab,
        GameObject hexVertexPrefab,
        GameObject hexEdgePrefab,
        GameObject robberPrefab)
    {
        this.gameConfig = gameConfig;
        this.random = random;
        this.horizontalSpacing = horizontalSpacing;
        this.verticalSpacing = verticalSpacing;
        this.woodTilePrefab = woodTilePrefab;
        this.clayTilePrefab = clayTilePrefab;
        this.sheepTilePrefab = sheepTilePrefab;
        this.wheatTilePrefab = wheatTilePrefab;
        this.oreTilePrefab = oreTilePrefab;
        this.desertTilePrefab = desertTilePrefab;
        this.waterTilePrefab = waterTilePrefab;
        this.hexVertexPrefab = hexVertexPrefab;
        this.hexEdgePrefab = hexEdgePrefab;
        this.robberPrefab = robberPrefab;
    }

    /// <summary>
    /// Populates hexTileMap, vertexMap, and edgeMap with data objects, spawns all GameObjects,
    /// wires neighbor relationships, and spawns the robber on the desert tile.
    /// The caller is responsible for passing the returned robber info to BuildingManager.InitializeRobber().
    /// </summary>
    public void InitializeBoard(
        int shuffleableGridSize,
        int fullGridSize,
        Dictionary<HexCoord, HexTile> hexTileMap,
        Dictionary<VertexCoord, HexVertex> vertexMap,
        Dictionary<EdgeCoord, HexEdge> edgeMap,
        IBoardManager boardManager,
        out RobberObject robberObject,
        out HexTile robberStartTile)
    {
        // Initialize data structure for full grid
        for (int q = -fullGridSize; q <= fullGridSize; q++)
        {
            for (int r = -fullGridSize; r <= fullGridSize; r++)
            {
                if (q + r >= -fullGridSize && q + r <= fullGridSize)
                {
                    // Hex
                    var coord = new HexCoord(q, r);
                    var isValidForBuilding = coord.Ring <= shuffleableGridSize;
                    var hexTile = new HexTile(coord, isValidForBuilding);
                    hexTileMap.Add(coord, hexTile);

                    // Verts
                    var northVertexCoord = new VertexCoord(coord, VertexOrientation.North);
                    var northVertex = new HexVertex(northVertexCoord);
                    vertexMap.Add(northVertexCoord, northVertex);

                    var southVertexCoord = new VertexCoord(coord, VertexOrientation.South);
                    var southVertex = new HexVertex(southVertexCoord);
                    vertexMap.Add(southVertexCoord, southVertex);

                    // Edges
                    var westEdgeCoord = new EdgeCoord(coord, EdgeOrientation.West);
                    var westEdge = new HexEdge(westEdgeCoord);
                    edgeMap.Add(westEdgeCoord, westEdge);

                    var northWestEdgeCoord = new EdgeCoord(coord, EdgeOrientation.NorthWest);
                    var northWestEdge = new HexEdge(northWestEdgeCoord);
                    edgeMap.Add(northWestEdgeCoord, northWestEdge);

                    var northEastEdgeCoord = new EdgeCoord(coord, EdgeOrientation.NorthEast);
                    var northEastEdge = new HexEdge(northEastEdgeCoord);
                    edgeMap.Add(northEastEdgeCoord, northEastEdge);
                }
            }
        }

        // Spawn tile objects for the full grid.
        // Tiles within the inner shuffleable region will get randomized tiles.
        // Tiles outside the shuffleable area will get water tiles.
        var tileTypesList = GetShuffledTileTypes();
        var shuffledTileCount = 0;
        var shuffledDiceNumbers = GetShuffledTileDiceNumbers();
        var shuffledDiceNumbersCount = 0;
        foreach (var hex in hexTileMap.Values)
        {
            var offsetCoord = GridHelpers.AxialHexToOffsetCoord(hex.HexCoordinates);

            var tilePosition = new Vector3(offsetCoord.x * horizontalSpacing, 0, offsetCoord.y * verticalSpacing);
            if (offsetCoord.y % 2 == 0)
            {
                tilePosition.x -= horizontalSpacing / 2;
            }

            var tilePrefab = waterTilePrefab;
            int? diceNumber = null;
            if (hex.Ring <= shuffleableGridSize)
            {
                var tileType = tileTypesList[shuffledTileCount];
                tilePrefab = GetTilePrefab(tileType);
                shuffledTileCount++;

                if (tileType != TileType.Desert && tileType != TileType.Water)
                {
                    diceNumber = shuffledDiceNumbers[shuffledDiceNumbersCount];
                    shuffledDiceNumbersCount++;
                }
            }

            var tileObject = UnityEngine.Object.Instantiate(tilePrefab, tilePosition, Quaternion.identity);
            var tileObjectComponent = tileObject.GetComponent<HexTileObject>();
            hex.TileObject = tileObjectComponent;
            tileObjectComponent.Initialize(boardManager, hex, diceNumber);
        }

        // Spawn vertex objects and initialize neighbors
        foreach (var vertex in vertexMap.Values)
        {
            if (hexTileMap.TryGetValue(vertex.VertexCoord.HexCoord, out var hex))
            {
                vertex.InitializeNeighbors(boardManager);

                if (!vertex.CanHaveBuildings())
                {
                    continue;
                }

                var vertexObject = UnityEngine.Object.Instantiate(hexVertexPrefab, Vector3.zero, Quaternion.identity);

                vertex.VertexObject = vertexObject.GetComponent<HexVertexObject>();
                vertex.VertexObject.Initialize(boardManager, vertex);

                switch (vertex.VertexCoord.Orientation)
                {
                    case VertexOrientation.North:
                        vertexObject.transform.parent = hex.TileObject.NorthVertexTransform;
                        break;
                    case VertexOrientation.South:
                        vertexObject.transform.parent = hex.TileObject.SouthVertexTransform;
                        break;
                    default:
                        Debug.LogError($"Unknown vertex orientation for {vertex}");
                        break;
                }

                vertexObject.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.LogError($"Could not find parent hex for {vertex}");
            }
        }

        // After all vertices are initialized, initialize hex tile neighbors
        foreach (var hexTile in hexTileMap.Values)
        {
            hexTile.InitializeNeighbors(boardManager);
        }

        // Spawn edge objects
        foreach (var edge in edgeMap.Values)
        {
            if (hexTileMap.TryGetValue(edge.EdgeCoord.HexCoord, out var hex))
            {
                edge.InitializeNeighbors(boardManager);

                if (!edge.CanHaveRoads())
                {
                    continue;
                }

                var edgeObject = UnityEngine.Object.Instantiate(hexEdgePrefab, Vector3.zero, Quaternion.identity);

                edge.EdgeObject = edgeObject.GetComponent<HexEdgeObject>();
                edge.EdgeObject.Initialize(boardManager, edge);

                switch (edge.EdgeCoord.Orientation)
                {
                    case EdgeOrientation.West:
                        edgeObject.transform.parent = hex.TileObject.WestEdgeTransform;
                        break;
                    case EdgeOrientation.NorthWest:
                        edgeObject.transform.parent = hex.TileObject.NorthWestEdgeTransform;
                        break;
                    case EdgeOrientation.NorthEast:
                        edgeObject.transform.parent = hex.TileObject.NorthEastEdgeTransform;
                        break;
                    default:
                        Debug.LogError($"Unknown edge orientation for {edge}");
                        break;
                }

                edgeObject.transform.localPosition = Vector3.zero;
                edgeObject.transform.localRotation = Quaternion.identity;
            }
            else
            {
                Debug.LogError($"Could not find parent hex for {edge}");
            }
        }

        // Spawn the robber object and place it on the desert tile
        var robberGO = UnityEngine.Object.Instantiate(robberPrefab, Vector3.zero, Quaternion.identity);
        robberObject = robberGO.GetComponent<RobberObject>();
        robberStartTile = null;

        foreach (var tile in hexTileMap.Values)
        {
            if (tile.TileObject.TileType == TileType.Desert)
            {
                tile.MoveRobberToTile(robberObject);
                robberStartTile = tile;
                break;
            }
        }
    }

    /// <summary>
    /// Destroys all tile, vertex, and edge GameObjects and clears the three maps.
    /// Does not affect turn state — caller is responsible for resetting TurnManager.
    /// </summary>
    public void ClearBoard(
        Dictionary<HexCoord, HexTile> hexTileMap,
        Dictionary<VertexCoord, HexVertex> vertexMap,
        Dictionary<EdgeCoord, HexEdge> edgeMap)
    {
        foreach (var tile in hexTileMap.Values)
        {
            if (tile.TileObject != null)
            {
                UnityEngine.Object.Destroy(tile.TileObject.gameObject);
            }
        }
        hexTileMap.Clear();

        foreach (var vertex in vertexMap.Values)
        {
            if (vertex.VertexObject != null)
            {
                UnityEngine.Object.Destroy(vertex.VertexObject.gameObject);
            }
        }
        vertexMap.Clear();

        foreach (var edge in edgeMap.Values)
        {
            if (edge.EdgeObject != null)
            {
                UnityEngine.Object.Destroy(edge.EdgeObject.gameObject);
            }
        }
        edgeMap.Clear();
    }

    private List<TileType> GetShuffledTileTypes()
    {
        var tileCounts = gameConfig.GetShuffleableTileCounts();
        var tileTypesList = new List<TileType>();
        foreach (var type in tileCounts.Keys)
        {
            var count = tileCounts[type];
            tileTypesList.AddRange(Enumerable.Repeat(type, count));
        }

        Util.Shuffle(tileTypesList);

        return tileTypesList;
    }

    private GameObject GetTilePrefab(TileType type)
    {
        switch (type)
        {
            case TileType.Wood:     return woodTilePrefab;
            case TileType.Clay:     return clayTilePrefab;
            case TileType.Sheep:    return sheepTilePrefab;
            case TileType.Wheat:    return wheatTilePrefab;
            case TileType.Ore:      return oreTilePrefab;
            case TileType.Desert:   return desertTilePrefab;
            case TileType.Water:    return waterTilePrefab;
            default:                return null;
        }
    }

    private List<int> GetShuffledTileDiceNumbers()
    {
        var diceNumbers = gameConfig.TileDiceNumbers.ToList();
        Util.Shuffle(diceNumbers);
        return diceNumbers;
    }
}
