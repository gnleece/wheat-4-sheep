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
    private GameConfig gameConfig;

    #endregion

    #region Private members
    
    private IUIManager _uiManager;
    private IRandomProvider _randomProvider;
    
    private StateMachine<GameState> _gameStateMachine;

    private int _playerCount;
    private List<IPlayer> _playerList = new();
    
    private bool _playerCountSelected;
    private bool _boardConfirmed;

    #endregion
    
    public void RegisterUIManager(IUIManager uiManager)
    {
        this._uiManager = uiManager;
    }

    public IReadOnlyList<IPlayer> PlayerList => _playerList.AsReadOnly();

    public GameState CurrentGameState => _gameStateMachine != null ? _gameStateMachine.CurrentState : GameState.None;

    public bool SettlementsMustConnectToRoad => CurrentGameState != GameState.FirstSettlementPlacement && 
                                                CurrentGameState != GameState.SecondSettlementPlacement;

    private void Start()
    {
        Reset();
    }

    private void Update()
    {
        if (_gameStateMachine != null)
        {
            _gameStateMachine.Update();
        }
    }

    private void Reset()
    {
        if (boardManager == null)
        {
            Debug.LogError("Board manager cannot be null");
            return;
        }
        
        _gameStateMachine = new StateMachine<GameState>("GameState");

        _gameStateMachine.AddState(GameState.PlayerSetup, OnEnterPlayerSetup, OnUpdatePlayerSetup, OnExitPlayerSetup);
        _gameStateMachine.AddState(GameState.BoardSetup, OnEnterBoardSetup, OnUpdateBoardSetup, null);
        _gameStateMachine.AddState(GameState.FirstSettlementPlacement, OnEnterFirstSettlementPlacement, OnUpdateFirstSettlementPlacement, null);
        _gameStateMachine.AddState(GameState.SecondSettlementPlacement, OnEnterSecondSettlementPlacement, OnUpdateSecondSettlementPlacement, null);
        _gameStateMachine.AddState(GameState.Playing, OnEnterPlaying, OnUpdatePlaying, null);
        _gameStateMachine.AddState(GameState.GameOver, OnEnterGameOver, null, null);

        _gameStateMachine.GoToState(GameState.PlayerSetup);
    }

    #region Player setup

    private void OnEnterPlayerSetup()
    {
        if (_uiManager != null)
        {
            _uiManager.ShowSetupScreen();
        }
    }

    private void OnUpdatePlayerSetup()
    {
        // Check if player count has been selected via UI buttons
        if (_playerCountSelected)
        {
            _gameStateMachine.GoToState(GameState.BoardSetup);
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
            _playerCount = count;
            _playerCountSelected = true;
            Debug.Log($"Player count selected: {_playerCount}");
            
            if (_uiManager != null)
            {
                _uiManager.HideSetupScreen();
            }
        }
    }
    
    public void ConfirmBoard()
    {
        _boardConfirmed = true;
        Debug.Log("Board layout confirmed");
        
        if (_uiManager != null)
        {
            _uiManager.HideBoardConfirmationScreen();
        }
    }
    
    public void RegenerateBoard()
    {
        Debug.Log("Regenerating board...");
        boardManager.StartNewGame(this, _uiManager);
    }

    private void OnExitPlayerSetup()
    {
        _randomProvider = new SystemRandomProvider();
        _playerList = new List<IPlayer>(_playerCount);

        for (int i = 0; i < _playerCount; i++)
        {
            IPlayer player = i == 0 ? new HumanPlayer() : new AIPlayerRandom(_randomProvider);
            player.Initialize(i, boardManager);
            _playerList.Add(player);
        }

        boardManager.InitializePlayerResourceHands(_playerList, _randomProvider);
        
        if (_uiManager != null)
        {
            Debug.Log("GameManager: Initializing UI player panels");
            _uiManager.InitializePlayerPanels(_playerList);
        }
        
        _playerCountSelected = false; // Reset for next game
        _boardConfirmed = false; // Reset for next game
    }

    #endregion

    #region Board setup

    private void OnEnterBoardSetup()
    {
        boardManager.StartNewGame(this, _uiManager);

        if (_uiManager != null)
        {
            _uiManager.ShowBoardConfirmationScreen();
        }
    }

    private void OnUpdateBoardSetup()
    {
        // Check if board has been confirmed via UI buttons
        if (_boardConfirmed)
        {
            _gameStateMachine.GoToState(GameState.FirstSettlementPlacement);
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
            _gameStateMachine.GoToState(GameState.SecondSettlementPlacement);
        }
    }

    private async Task RunFirstSettlementPlacement()
    {
        foreach (var player in _playerList)
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
            _gameStateMachine.GoToState(GameState.Playing);
        }
    }

    private async Task RunSecondSettlementPlacement()
    {
        for (int i = _playerCount - 1; i >= 0; i--)
        {
            var player = _playerList[i];

            Debug.Log($"Player {player.PlayerId} placing second settlement and road");

            await player.PlaceSecondSettlementAndRoadAsync();
        }
    }

    #endregion

    #region Playing

    Task playingTask = null;

    private void OnEnterPlaying()
    {
        if (_uiManager != null)
        {
            _uiManager.ShowGameplayUI();
        }
        
        playingTask = RunPlaying();
    }

    private void OnUpdatePlaying()
    {
        if (playingTask != null && playingTask.IsCompleted)
        {
            playingTask = null; // Clear the task reference
            _gameStateMachine.GoToState(GameState.GameOver);
        }
    }

    private async Task RunPlaying()
    {
        var isGameOver = false;
        while (!isGameOver)
        {
            foreach (var player in _playerList)
            {
                Debug.Log($"Player {player.PlayerId} turn");
                
                if (_uiManager != null)
                {
                    _uiManager.SetActivePlayer(player.PlayerId);
                    _uiManager.UpdatePlayerPanels();
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
        foreach (var player in _playerList)
        {
            if (boardManager.GetPlayerScore(player) >= gameConfig.VictoryPointsToWin)
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
        if (_uiManager != null && winner != null)
        {
            _uiManager.ShowGameOverScreen(winner, winningScore);
        }
    }
    
    private IPlayer GetWinningPlayer()
    {
        IPlayer winner = null;
        int highestScore = 0;
        
        foreach (var player in _playerList)
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
        if (_uiManager != null)
        {
            _uiManager.HideGameOverScreen();
        }
        
        // Reset the game state
        Reset();
    }

    #endregion
}
