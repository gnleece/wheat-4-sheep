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

    private readonly BoardPrefabConfig prefabConfig;

    #endregion

    public GridInitializer(
        GameConfig gameConfig,
        System.Random random,
        float horizontalSpacing,
        float verticalSpacing,
        BoardPrefabConfig prefabConfig)
    {
        this.gameConfig = gameConfig;
        this.random = random;
        this.horizontalSpacing = horizontalSpacing;
        this.verticalSpacing = verticalSpacing;
        this.prefabConfig = prefabConfig;
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

            var tilePrefab = prefabConfig.WaterTilePrefab;
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

                var vertexObject = UnityEngine.Object.Instantiate(prefabConfig.HexVertexPrefab, Vector3.zero, Quaternion.identity);

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

                var edgeObject = UnityEngine.Object.Instantiate(prefabConfig.HexEdgePrefab, Vector3.zero, Quaternion.identity);

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

        // Assign ports to boundary vertices and spawn port indicators
        AssignPorts(edgeMap);

        // Spawn the robber object and place it on the desert tile
        var robberGO = UnityEngine.Object.Instantiate(prefabConfig.RobberPrefab, Vector3.zero, Quaternion.identity);
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

    private void AssignPorts(Dictionary<EdgeCoord, HexEdge> edgeMap)
    {
        // Find all boundary edges: adjacent to at least one land tile and at least one non-land tile.
        var boundaryEdges = new List<HexEdge>();
        foreach (var edge in edgeMap.Values)
        {
            if (edge.NeighborHexTiles == null)
            {
                continue;
            }

            var hasLand = false;
            var hasNonLand = false;
            foreach (var hex in edge.NeighborHexTiles)
            {
                if (hex.CanHaveBuildingsAndRoads)
                {
                    hasLand = true;
                }
                else
                {
                    hasNonLand = true;
                }
            }

            // An edge with only land neighbors is interior; skip edges with no neighbor at all.
            if (hasLand && hasNonLand)
            {
                boundaryEdges.Add(edge);
            }
        }

        Util.Shuffle(boundaryEdges);

        var portTypes = GetShuffledPortTypes();
        var portCount = Mathf.Min(portTypes.Count, boundaryEdges.Count);

        for (int i = 0; i < portCount; i++)
        {
            var edge = boundaryEdges[i];
            var port = new Port(portTypes[i]);

            // Assign the port to both vertices of this boundary edge.
            if (edge.NeighborVertices != null)
            {
                foreach (var vertex in edge.NeighborVertices)
                {
                    if (vertex.CanHaveBuildings())
                    {
                        vertex.Port = port;
                    }
                }
            }

            // Spawn a port indicator on the land-side tile if a prefab is configured.
            if (prefabConfig.PortIndicatorPrefab != null)
            {
                HexTile landTile = null;
                foreach (var hex in edge.NeighborHexTiles)
                {
                    if (hex.CanHaveBuildingsAndRoads)
                    {
                        landTile = hex;
                        break;
                    }
                }

                if (landTile?.TileObject != null)
                {
                    var indicatorGO = UnityEngine.Object.Instantiate(
                        prefabConfig.PortIndicatorPrefab,
                        landTile.TileObject.transform.position,
                        Quaternion.identity,
                        landTile.TileObject.transform);
                    var indicator = indicatorGO.GetComponent<PortIndicatorObject>();
                    if (indicator != null)
                    {
                        indicator.Initialize(port.PortType);
                    }
                }
            }
        }
    }

    private List<PortType> GetShuffledPortTypes()
    {
        var types = new List<PortType>
        {
            PortType.Generic,
            PortType.Generic,
            PortType.Generic,
            PortType.Generic,
            PortType.Wood,
            PortType.Clay,
            PortType.Sheep,
            PortType.Wheat,
            PortType.Ore,
        };
        Util.Shuffle(types);
        return types;
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
            case TileType.Wood:     return prefabConfig.WoodTilePrefab;
            case TileType.Clay:     return prefabConfig.ClayTilePrefab;
            case TileType.Sheep:    return prefabConfig.SheepTilePrefab;
            case TileType.Wheat:    return prefabConfig.WheatTilePrefab;
            case TileType.Ore:      return prefabConfig.OreTilePrefab;
            case TileType.Desert:   return prefabConfig.DesertTilePrefab;
            case TileType.Water:    return prefabConfig.WaterTilePrefab;
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
