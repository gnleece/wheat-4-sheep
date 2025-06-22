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
        FirstSettlementPlacement,
        SecondSettlementPlacement,
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

    public IReadOnlyList<IPlayer> PlayerList => playerList.AsReadOnly();

    public GameState CurrentGameState => gameStateMachine != null ? gameStateMachine.CurrentState : GameState.None;

    public bool SettlementsMustConnectToRoad => CurrentGameState != GameState.FirstSettlementPlacement && 
                                                CurrentGameState != GameState.SecondSettlementPlacement;

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
        gameStateMachine.AddState(GameState.FirstSettlementPlacement, OnEnterFirstSettlementPlacement, OnUpdateFirstSettlementPlacement, OnExitFirstSettlementPlacement);
        gameStateMachine.AddState(GameState.SecondSettlementPlacement, OnEnterSecondSettlementPlacement, OnUpdateSecondSettlementPlacement, OnExitSecondSettlementPlacement);
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
            IPlayer player = i == 0 ? new HumanPlayer() : new AIPlayer();
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
            gameStateMachine.GoToState(GameState.FirstSettlementPlacement);
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

    #region First Settlement Placement

    private Task placementTask;

    private void OnEnterFirstSettlementPlacement()
    {
        placementTask = RunFirstSettlementPlacement();
    }

    private void OnUpdateFirstSettlementPlacement()
    {
        if (placementTask != null && placementTask.IsCompleted)
        {
            placementTask = null; // Clear the task reference
            gameStateMachine.GoToState(GameState.SecondSettlementPlacement);
        }
    }

    private void OnExitFirstSettlementPlacement()
    {
        ClearHudText();
    }

    private async Task RunFirstSettlementPlacement()
    {
        foreach (var player in playerList)
        {
            SetHudText($"Player {player.PlayerId} placing first settlement and road");
            await player.PlaceFirstSettlementAndRoadAsync();
        }
    }

    #endregion

    #region Second Settlement Placement

    private void OnEnterSecondSettlementPlacement()
    {
        placementTask = RunSecondSettlementPlacement();
    }

    private void OnUpdateSecondSettlementPlacement()
    {
        if (placementTask != null && placementTask.IsCompleted)
        {
            placementTask = null; // Clear the task reference
            gameStateMachine.GoToState(GameState.Playing);
        }
    }

    private void OnExitSecondSettlementPlacement()
    {
        ClearHudText();
    }

    private async Task RunSecondSettlementPlacement()
    {
        for (int i = playerCount - 1; i >= 0; i--)
        {
            var player = playerList[i];

            SetHudText($"Player {player.PlayerId} placing second settlement and road");

            await player.PlaceSecondSettlementAndRoadAsync();
        }
    }

    #endregion

    #region Playing

    Task playingTask = null;

    private void OnEnterPlaying()
    {
        playingTask = RunPlaying();
    }

    private void OnUpdatePlaying()
    {
        if (playingTask != null && playingTask.IsCompleted)
        {
            playingTask = null; // Clear the task reference
            gameStateMachine.GoToState(GameState.GameOver);
        }
    }

    private void OnExitPlaying()
    {
        ClearHudText();
    }

    private async Task RunPlaying()
    {
        while (true)
        {
            foreach (var player in playerList)
            {
                SetHudText($"Player {player.PlayerId} turn");
                await player.PlayTurnAsync();
            }
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
