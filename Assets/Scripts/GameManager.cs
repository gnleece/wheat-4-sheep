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

    #endregion

    private StateMachine<GameState> gameStateMachine = null;

    private int playerCount = 0;
    private List<IPlayer> playerList = new List<IPlayer>();
    private UIManager uiManager;
    private bool playerCountSelected = false;
    private bool boardConfirmed = false;

    public IReadOnlyList<IPlayer> PlayerList => playerList.AsReadOnly();

    public GameState CurrentGameState => gameStateMachine != null ? gameStateMachine.CurrentState : GameState.None;

    public bool SettlementsMustConnectToRoad => CurrentGameState != GameState.FirstSettlementPlacement && 
                                                CurrentGameState != GameState.SecondSettlementPlacement;

    private void Start()
    {
        // UIManager will be found when needed since it's created dynamically
        Reset();
    }
    
    private UIManager GetUIManager()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogWarning("GameManager: UIManager not found! UI features will not work.");
            }
        }
        return uiManager;
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
            Debug.LogError("Board manager cannot be null");
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
        var ui = GetUIManager();
        if (ui != null)
        {
            ui.ShowSetupScreen();
        }
    }

    private void OnUpdatePlayerSetup()
    {
        // Check if player count has been selected via UI buttons
        if (playerCountSelected)
        {
            gameStateMachine.GoToState(GameState.BoardSetup);
        }
        
        // Keep legacy keyboard support as backup
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectPlayerCount(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectPlayerCount(4);
        }
    }
    
    public void SelectPlayerCount(int count)
    {
        if (count >= 3 && count <= 4)
        {
            playerCount = count;
            playerCountSelected = true;
            Debug.Log($"Player count selected: {playerCount}");
            
            var ui = GetUIManager();
            if (ui != null)
            {
                ui.HideSetupScreen();
            }
        }
    }
    
    public void ConfirmBoard()
    {
        boardConfirmed = true;
        Debug.Log("Board layout confirmed");
        
        var ui = GetUIManager();
        if (ui != null)
        {
            ui.HideBoardConfirmationScreen();
        }
    }
    
    public void RegenerateBoard()
    {
        Debug.Log("Regenerating board...");
        boardManager.StartNewGame(this);
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

        var ui = GetUIManager();
        if (ui != null)
        {
            Debug.Log("GameManager: Initializing UI player panels");
            ui.InitializePlayerPanels(playerList);
        }

        ClearHudText();
        playerCountSelected = false; // Reset for next game
        boardConfirmed = false; // Reset for next game
    }

    #endregion

    #region Board setup

    private void OnEnterBoardSetup()
    {
        boardManager.StartNewGame(this);
        var ui = GetUIManager();
        if (ui != null)
        {
            ui.ShowBoardConfirmationScreen();
        }
    }

    private void OnUpdateBoardSetup()
    {
        // Check if board has been confirmed via UI buttons
        if (boardConfirmed)
        {
            gameStateMachine.GoToState(GameState.FirstSettlementPlacement);
        }
        
        // Keep legacy keyboard support as backup
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ConfirmBoard();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RegenerateBoard();
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
            Debug.Log($"Player {player.PlayerId} placing first settlement and road");
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

            Debug.Log($"Player {player.PlayerId} placing second settlement and road");

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
                Debug.Log($"Player {player.PlayerId} turn");
                
                var ui = GetUIManager();
                if (ui != null)
                {
                    ui.SetActivePlayer(player.PlayerId);
                    ui.UpdatePlayerPanels();
                }
                
                await player.PlayTurnAsync();
            }
        }
    }

    #endregion

    #region Helpers

    private void ClearHudText()
    {
        // Legacy method kept for compatibility - now does nothing
    }

    #endregion
}
