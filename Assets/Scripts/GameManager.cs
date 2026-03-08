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

    public void RegisterUIManager(UIManager uiManager)
    {
        this.uiManager = uiManager;
    }

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
            Debug.LogError("Board manager cannot be null");
            return;
        }
        
        gameStateMachine = new StateMachine<GameState>("GameState");

        gameStateMachine.AddState(GameState.PlayerSetup, OnEnterPlayerSetup, OnUpdatePlayerSetup, OnExitPlayerSetup);
        gameStateMachine.AddState(GameState.BoardSetup, OnEnterBoardSetup, OnUpdateBoardSetup, null);
        gameStateMachine.AddState(GameState.FirstSettlementPlacement, OnEnterFirstSettlementPlacement, OnUpdateFirstSettlementPlacement, null);
        gameStateMachine.AddState(GameState.SecondSettlementPlacement, OnEnterSecondSettlementPlacement, OnUpdateSecondSettlementPlacement, null);
        gameStateMachine.AddState(GameState.Playing, OnEnterPlaying, OnUpdatePlaying, null);
        gameStateMachine.AddState(GameState.GameOver, OnEnterGameOver, null, null);

        gameStateMachine.GoToState(GameState.PlayerSetup);
    }

    #region Player setup

    private void OnEnterPlayerSetup()
    {
        if (uiManager != null)
        {
            uiManager.ShowSetupScreen();
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
            
            if (uiManager != null)
            {
                uiManager.HideSetupScreen();
            }
        }
    }
    
    public void ConfirmBoard()
    {
        boardConfirmed = true;
        Debug.Log("Board layout confirmed");
        
        if (uiManager != null)
        {
            uiManager.HideBoardConfirmationScreen();
        }
    }
    
    public void RegenerateBoard()
    {
        Debug.Log("Regenerating board...");
        boardManager.StartNewGame(this, uiManager);
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
        
        if (uiManager != null)
        {
            Debug.Log("GameManager: Initializing UI player panels");
            uiManager.InitializePlayerPanels(playerList);
        }
        
        playerCountSelected = false; // Reset for next game
        boardConfirmed = false; // Reset for next game
    }

    #endregion

    #region Board setup

    private void OnEnterBoardSetup()
    {
        boardManager.StartNewGame(this, uiManager);

        if (uiManager != null)
        {
            uiManager.ShowBoardConfirmationScreen();
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
        if (uiManager != null)
        {
            uiManager.ShowGameplayUI();
        }
        
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

    private async Task RunPlaying()
    {
        var isGameOver = false;
        while (!isGameOver)
        {
            foreach (var player in playerList)
            {
                Debug.Log($"Player {player.PlayerId} turn");
                
                if (uiManager != null)
                {
                    uiManager.SetActivePlayer(player.PlayerId);
                    uiManager.UpdatePlayerPanels();
                }
                
                await player.PlayTurnAsync();

                isGameOver = IsGameOver();
                if (isGameOver)
                {
                    break;
                }
            }
        }
    }

    private bool IsGameOver()
    {
        foreach (var player in playerList)
        {
            if (boardManager.GetPlayerScore(player) >= 4)  // TODO: make this configurable
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Game Over

    private void OnEnterGameOver()
    {
        Debug.Log("Game over!");
        
        // Determine the winning player
        IPlayer winner = GetWinningPlayer();
        int winningScore = winner != null ? boardManager.GetPlayerScore(winner) : 0;
        
        // Show the game over screen
        if (uiManager != null && winner != null)
        {
            uiManager.ShowGameOverScreen(winner, winningScore);
        }
    }
    
    private IPlayer GetWinningPlayer()
    {
        IPlayer winner = null;
        int highestScore = 0;
        
        foreach (var player in playerList)
        {
            int score = boardManager.GetPlayerScore(player);
            if (score > highestScore)
            {
                highestScore = score;
                winner = player;
            }
        }
        
        return winner;
    }
    
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        
        // Hide game over screen
        if (uiManager != null)
        {
            uiManager.HideGameOverScreen();
        }
        
        // Reset the game state
        Reset();
    }

    #endregion
}
