# Wheat-4-Sheep Architecture Analysis

*Analyzed: 2026-03-06*

---

## Project Overview

A Unity implementation of Catan with 42 C# scripts. Supports 3–4 players (human + AI mix), standard hex board, resource distribution, building placement, robber mechanics, and city upgrades. Dev cards, longest road, and largest army are not yet implemented.

---

## What's Working Well

- **Async/Await turn flow** — `Task`-based async throughout player turns, selection, and modals is a smart choice for turn-based games. The `TaskCompletionSource` pattern in `DiscardUIController.cs` and `PlayerSelectionUIController.cs` is clean.
- **Interface segregation** — `IPlayer`, `IBoardManager`, `IGameManager`, `IInteractable` give good seams for swapping implementations.
- **Generic `StateMachine<T>`** — Reusable, with enter/update/exit hooks.
- **Data/view separation** — Pure data (`HexTile`, `HexVertex`, `HexEdge`) is separated from visuals (`HexTileObject`, `HexVertexObject`, `HexEdgeObject`).

---

## Bugs

### ~~Dice roll hardcoded to 7~~ ✅ Fixed
~~`BoardManager.cs:882` — `var diceRoll = 7;` means every roll triggers the robber.~~
Fixed: now correctly uses `dieA + dieB`.

### Discard on 7 was commented out ✅ Fixed
~~`BoardManager.cs:898` — `HandleSevenRollDiscard()` was commented out, so players never discard when holding >7 cards.~~
Fixed (uncommented by linter/user).

---

## High Priority Issues

### `BoardManager` is a God Object

> **Refactoring complete** — `GridInitializer` ✅, `TurnManager` ✅, `ResourceManager` ✅, `BuildingManager` ✅. `BoardManager` is now a thin coordinator.


At ~1,430 lines, `BoardManager.cs` owned too many responsibilities:

- Hex grid initialization and neighbor setup
- Turn state tracking and validation (`PlayerTurn` nested class)
- Resource distribution
- Building and road placement validation
- Robber movement
- Dice rolling
- Player stealing
- Victory point scoring queries

**Extracted classes:**

| Extracted class | Responsibilities |
|---|---|
| `GridInitializer` | Tile/vertex/edge creation, neighbor wiring |
| `TurnManager` | `PlayerTurn`, `BeginPlayerTurn`/`EndPlayerTurn`, `CanEndTurn` |
| `ResourceManager` | `playerResourceHands`, distribution, discard, stealing |
| `BuildingManager` | Placement validation, cost deduction, upgrade logic |

`BoardManager` coordinates these rather than implementing them.

---

### ~~Coupling via `FindAnyObjectByType`~~ ✅ Fixed

~~`UIManager.cs:35-36`, `DiscardUIController`, and `PlayerSelectionUIController` all use `FindAnyObjectByType` to locate `GameManager` and `BoardManager`.~~

Fixed: All 12 usages across 7 files eliminated. `UISetup` is now the single wiring point — it runs in `Awake()`, creates `UIManager`, and injects dependencies explicitly:
- `UIManager.Initialize(IBoardManager)` replaces `Start()` discovery
- `GameManager.RegisterUIManager(UIManager)` replaces lazy `FindAnyObjectByType`
- `BoardManager.StartNewGame(IGameManager, IUIManager)` receives a narrow `IUIManager` interface, breaking the circular dep
- `PlayerUIPanel.Initialize(IPlayer, IBoardManager)` receives board manager directly
- `UISetup` button lambdas capture the serialized `GameManager` field instead of searching at click time
- `UIDebugHelper` uses a `[SerializeField]` field for `GameUIController`

---

## Medium Priority Issues

### 4 Separate "manually selected" fields in `BoardManager`

`BoardManager.cs:118-121` maintains parallel fields:
- `manuallySelectedSettlementLocation`
- `manuallySelectedRoadLocation`
- `manuallySelectedSettlementToUpgrade`
- `manuallySelectedRobberLocation`

These exist because board mode and awaited selection type track parallel state. A single `TaskCompletionSource<object>` or generic `GetManualSelection<T>()` would collapse these.

---

### ~~Brittle neighbor logic in `HexVertex` and `HexEdge`~~ ✅ Fixed

~~`HexVertex.InitializeNeighbors()` (lines 132–169) and `HexEdge.InitializeNeighbors()` (lines 161–211) contain hardcoded coordinate offsets for specific vertex/edge orientations. If the coordinate system changes, both files break independently.~~

Fixed: All coordinate offset tables moved to `GridHelpers` as six static methods (`GetVertexNeighborHexCoords`, `GetVertexNeighborVertexCoords`, `GetVertexNeighborEdgeCoords`, `GetEdgeNeighborHexCoords`, `GetEdgeNeighborVertexCoords`, `GetEdgeNeighborEdgeCoords`). `InitializeNeighbors` in both classes now iterates the results — no coordinate math in either.

---

### Conflicting input systems

`HumanPlayer.cs:61-88` — keyboard keys 1–4 for actions, 0 to end turn. UI buttons also exist and run concurrently with no coordination.

**Fix:** Pick one canonical input path.

---

### Magic string UI paths

`DiscardUIController.InitializeResourceButtons()` navigates the UI hierarchy via hardcoded transform paths. Renaming a GameObject in the scene silently breaks it.

**Fix:** Use serialized direct references from the inspector.

---

## Low Priority / Future Work

| Issue | Detail |
|---|---|
| Victory threshold hardcoded | `GameManager.cs:353` — set to 4, standard Catan is 10. Move to `GameConfig`. |
| Initial resources hardcoded | `BoardManager.cs:541-545` — 2 of each at game start. Fine as-is but worth a named constant. |
| AI action cap | `AIPlayer.cs:7` — `MAX_ACTIONS_PER_TURN = 10` is arbitrary. AI should act until it can't, not count iterations. |
| Longest road | Commented TODO at `BoardManager.cs:573`. Not implemented. |
| Largest army | Commented TODO at `BoardManager.cs:574`. Not implemented. |
| Development cards | `BuildingCosts.cs` defines dev card cost but the system is entirely unimplemented. |
| Robber visual | `RobberObject.cs` is an empty 6-line placeholder. |

---

## File Reference

### Core Game Logic
| File | Purpose |
|---|---|
| `GameManager.cs` | Main game flow, state machine, player creation |
| `BoardManager.cs` | Board state coordinator (thin — delegates to sub-managers) |
| `GridInitializer.cs` | Tile/vertex/edge creation, GameObject spawning, neighbor wiring |
| `StateMachine.cs` | Generic reusable state machine |
| `IGameManager.cs` | GameManager interface |
| `IBoardManager.cs` | BoardManager interface |

### Players
| File | Purpose |
|---|---|
| `IPlayer.cs` | Player interface |
| `HumanPlayer.cs` | Keyboard-driven human player |
| `AIPlayer.cs` | Random-decision AI player |

### Resources & Buildings
| File | Purpose |
|---|---|
| `ResourceType.cs` | Resource enum (Wood, Clay, Sheep, Wheat, Ore) |
| `ResourceHand.cs` | Resource collection wrapper |
| `BuildingCosts.cs` | Static cost definitions |
| `Building.cs` | Building entity (Settlement/City) |

### Board Data Structures
| File | Purpose |
|---|---|
| `Grid.cs` | Hex coordinate system (axial, vertex, edge coords) |
| `HexTile.cs` | Tile data |
| `HexVertex.cs` | Vertex (settlement location) data |
| `HexEdge.cs` | Edge (road location) data |
| `Road.cs` | Road entity |
| `TileType.cs` | Tile type enum |

### UI
| File | Purpose |
|---|---|
| `UIManager.cs` | Central UI controller |
| `PlayerUIPanel.cs` | Per-player status display |
| `DiscardUIController.cs` | 7-roll discard modal |
| `PlayerSelectionUIController.cs` | Steal target selection modal |

### Visualization
| File | Purpose |
|---|---|
| `HexTileObject.cs` | Tile MonoBehaviour |
| `HexVertexObject.cs` | Vertex MonoBehaviour |
| `HexEdgeObject.cs` | Edge MonoBehaviour |
| `SettlementObject.cs` | Settlement visual |
| `CityObject.cs` | City visual |
| `RoadObject.cs` | Road visual |
| `RobberObject.cs` | Robber visual (placeholder) |

### Selection & Input
| File | Purpose |
|---|---|
| `BuildingLocationSelectionObject.cs` | Settlement click handler |
| `RoadLocationSelectionObject.cs` | Road click handler |
| `HexTileSelectionObject.cs` | Tile click handler (robber) |
| `InputManager.cs` | Raycast-based input system |
| `IInteractable.cs` | Selection interface |

### Configuration & Utilities
| File | Purpose |
|---|---|
| `GameConfig.cs` | ScriptableObject for tile/dice config |
| `PlayerColorManager.cs` | Player color assignment |
| `DebugSettings.cs` | Debug toggles ScriptableObject |
| `Util.cs` | Fisher-Yates shuffle |

---

## State Machine Overview

```
GameManager (primary flow)
├── PlayerSetup
├── BoardSetup
├── FirstSettlementPlacement  (async, in order)
├── SecondSettlementPlacement (async, reverse order)
├── Playing                   (async, infinite turn loop)
└── GameOver

BoardManager (board interaction mode)
├── Idle
├── ChooseSettlementLocation
├── ChooseRoadLocation
├── ChooseSettlementToUpgrade
└── ChooseRobberLocation
```

## Turn Data Flow

```
GameManager.RunPlaying()
  └─> for each player:
      ├─> boardManager.BeginPlayerTurn(player)
      ├─> await player.PlayTurnAsync()
      │   ├─> (Human) keyboard loop: roll → build → end
      │   └─> (AI) delay → roll → greedy build loop
      └─> boardManager.EndPlayerTurn()
```

## Resource Flow

```
Game start:         InitializePlayerResourceHands()  [2 of each]
Second settlement:  GivePlayerResourcesForNeighborHexTiles()
Each turn:          RollDice() → GiveAllPlayersResourcesForHexTile()
Building:           currentPlayerHand.Remove(BuildingCosts.*)
7-roll:             StealRandomResourceFromPlayer()
```
