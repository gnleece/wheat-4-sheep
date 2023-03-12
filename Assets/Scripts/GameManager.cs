using System.Collections;
using System.Collections.Generic;
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
    private GridManager gridManager;

    [SerializeField]
    private TextMesh hudDebugText;

    #endregion

    private StateMachine<GameState> gameStateMachine = null;
    private int playerCount = 0;

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
        if (gridManager == null)
        {
            SetHudText("Grid manager cannot be null");
            return;
        }
        
        gameStateMachine = new StateMachine<GameState>("GameState");

        gameStateMachine.AddState(GameState.PlayerSetup, OnEnterPlayerSetup, OnUpdatePlayerSetup, OnExitPlayerSetup);
        gameStateMachine.AddState(GameState.BoardSetup, OnEnterBoardSetup, OnUpdateBoardSetup, OnExitBoardSetup);
        gameStateMachine.AddState(GameState.InitialPlacement, null, null, null);
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
        ClearHudText();
    }

    #endregion

    #region Board setup

    private void OnEnterBoardSetup()
    {
        gridManager.StartNewGame();
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
            gridManager.StartNewGame();
        }
    }

    private void OnExitBoardSetup()
    {
        ClearHudText();
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
