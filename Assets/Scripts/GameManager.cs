using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Main controller that manages game state and player setup.
/// </summary>
public class GameManager : MonoBehaviour, IGameManager
{
    #region Enums

    public enum GameState
    {
        None,
        PlayerSetup,
        BoardSetup,
        InitialPlacement,
        Playing,
        GameOver
    }

    #endregion

    #region Serialized fields

    [SerializeField]
    private BoardManager boardManager;

    [SerializeField]
    private TextMesh hudDebugText;

    #endregion

    private StateMachine<GameState> gameStateMachine = null;

    private int playerCount = 0;
    private List<IPlayer> playerList = new List<IPlayer>();

    public GameState CurrentGameState => gameStateMachine != null ? gameStateMachine.CurrentState : GameState.None;

    private void Start()
    {
        Reset();
    }

    private void Update()
    {
        if (gameStateMachine != null)
        {
            gameStateMachine.Update();
        }
    }

    private void Reset()
    {
        if (boardManager == null)
        {
            SetHudText("Board manager cannot be null");
            return;
        }
        
        gameStateMachine = new StateMachine<GameState>("GameState");

        gameStateMachine.AddState(GameState.PlayerSetup, OnEnterPlayerSetup, OnUpdatePlayerSetup, OnExitPlayerSetup);
        gameStateMachine.AddState(GameState.BoardSetup, OnEnterBoardSetup, OnUpdateBoardSetup, OnExitBoardSetup);
        gameStateMachine.AddState(GameState.InitialPlacement, OnEnterInitialPlacement, OnUpdateInitialPlacement, OnExitInitialPlacement);
        gameStateMachine.AddState(GameState.Playing, OnEnterPlaying, OnUpdatePlaying, OnExitPlaying);
        gameStateMachine.AddState(GameState.GameOver, null, null, null);

        gameStateMachine.GoToState(GameState.PlayerSetup);
    }

    #region Player setup

    private void OnEnterPlayerSetup()
    {
        SetHudText("Select number of players (3 or 4)");
    }

    private void OnUpdatePlayerSetup()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            playerCount = 3;
            gameStateMachine.GoToState(GameState.BoardSetup);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            playerCount = 4;
            gameStateMachine.GoToState(GameState.BoardSetup);
        }
    }

    private void OnExitPlayerSetup()
    {
        playerList = new List<IPlayer>(playerCount);

        for (int i = 0; i < playerCount; i++)
        {
            var player = new HumanPlayer();             // Only human players for now
            player.Initialize(i, boardManager);
            playerList.Add(player);
        }

        boardManager.InitializePlayerResourceHands(playerList);

        ClearHudText();
    }

    #endregion

    #region Board setup

    private void OnEnterBoardSetup()
    {
        boardManager.StartNewGame(this);
        SetHudText("Press 1 to accept board, press 2 to reset");
    }

    private void OnUpdateBoardSetup()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gameStateMachine.GoToState(GameState.InitialPlacement);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            boardManager.StartNewGame(this);
        }
    }

    private void OnExitBoardSetup()
    {
        ClearHudText();
    }

    #endregion

    #region Initial Placement

    private Task placementTask;

    private void OnEnterInitialPlacement()
    {
        placementTask = RunInitialPlacement();
    }

    private void OnUpdateInitialPlacement()
    {
        if (placementTask != null && placementTask.IsCompleted)
        {
            gameStateMachine.GoToState(GameState.Playing);
        }
    }

    private void OnExitInitialPlacement()
    {
        ClearHudText();
    }

    private async Task RunInitialPlacement()
    {
        foreach (var player in playerList)
        {
            SetHudText($"Player {player.PlayerId} placing first settlement and road");

            await player.PlaceFirstSettlementAndRoadAsync();
        }

        for (int i = playerCount - 1; i >= 0; i--)
        {
            var player = playerList[i];

            SetHudText($"Player {player.PlayerId} placing second settlement and road");

            await player.PlaceSecondSettlementAndRoadAsync();
        }
    }

    #endregion

    #region Playing

    private void OnEnterPlaying()
    {
        
    }

    private void OnUpdatePlaying()
    {

    }

    private void OnExitPlaying()
    {

    }

    #endregion

    #region Helpers

    private void SetHudText(string text)
    {
        if (hudDebugText != null)
        {
            hudDebugText.text = text;
        }
        Debug.Log(text);
    }

    private void ClearHudText()
    {
        if (hudDebugText != null)
        {
            hudDebugText.text = string.Empty;
        }
    }

    #endregion
}
