using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Enums

    private enum GameState
    {
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
            SetHudText("Grid manager cannot be null");
            return;
        }
        
        gameStateMachine = new StateMachine<GameState>("GameState");

        gameStateMachine.AddState(GameState.PlayerSetup, OnEnterPlayerSetup, OnUpdatePlayerSetup, OnExitPlayerSetup);
        gameStateMachine.AddState(GameState.BoardSetup, OnEnterBoardSetup, OnUpdateBoardSetup, OnExitBoardSetup);
        gameStateMachine.AddState(GameState.InitialPlacement, OnEnterInitialPlacment, OnUpdateInitialPlacement, OnExitInitialPlacement);
        gameStateMachine.AddState(GameState.Playing, null, null, null);
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

        ClearHudText();
    }

    #endregion

    #region Board setup

    private void OnEnterBoardSetup()
    {
        boardManager.StartNewGame();
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
            boardManager.StartNewGame();
        }
    }

    private void OnExitBoardSetup()
    {
        ClearHudText();
    }

    #endregion

    #region Initial Placement

    private Task placementTask;

    private void OnEnterInitialPlacment()
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
            SetHudText($"Player {player.PlayerId} placing first road and settlement");

            var positions = await player.SelectFirstSettlementAndRoadPositions();

            Debug.Log($"Player {player.PlayerId} selected: {positions.Item1}, {positions.Item2}");
        }
    }

    #endregion

    #region Helpers

    private void SetHudText(string text)
    {
        if (hudDebugText != null)
        {
            hudDebugText.text = text;
        }
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
