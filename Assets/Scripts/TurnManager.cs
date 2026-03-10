using UnityEngine;

public class TurnManager
{
    private PlayerTurn _currentTurn = null;

    public IPlayer CurrentPlayer => _currentTurn?.Player;
    public PlayerTurnType? CurrentTurnType => _currentTurn?.TurnType;
    public bool HasRolledDice => _currentTurn?.HasRolledDice ?? false;
    public bool DevCardBoughtThisTurn => _currentTurn?.DevCardBoughtThisTurn ?? false;
    public bool DevCardPlayedThisTurn => _currentTurn?.DevCardPlayedThisTurn ?? false;
    public bool IsActive => _currentTurn != null;

    public bool BeginTurn(IPlayer player, PlayerTurnType turnType)
    {
        if (_currentTurn != null)
        {
            Debug.LogError($"Cannot begin player turn: a player turn is already in progress for player {_currentTurn.Player.PlayerId}");
            return false;
        }

        _currentTurn = new PlayerTurn { Player = player, TurnType = turnType };
        Debug.Log($"Player {player.PlayerId} turn started");
        return true;
    }

    public bool EndTurn(IPlayer player)
    {
        if (_currentTurn == null || _currentTurn.Player != player)
        {
            Debug.LogError($"Cannot end player turn: no player turn in progress for player {player.PlayerId}");
            return false;
        }

        if (!_currentTurn.CanEndTurn)
        {
            Debug.LogError($"Cannot end player turn");
            return false;
        }

        Debug.Log($"Player {player.PlayerId} turn ended");
        _currentTurn = null;
        return true;
    }

    public bool IsPlayerTurn(IPlayer player) => _currentTurn != null && _currentTurn.Player == player;

    public bool CanEndTurn(IPlayer player)
    {
        if (_currentTurn == null || _currentTurn.Player != player) return false;
        return _currentTurn.CanEndTurn;
    }

    public void SetHasRolledDice()
    {
        if (_currentTurn != null)
        {
            _currentTurn.HasRolledDice = true;
        }
    }

    public void SetDevCardBoughtThisTurn()
    {
        if (_currentTurn != null)
        {
            _currentTurn.DevCardBoughtThisTurn = true;
        }
    }

    public void SetDevCardPlayedThisTurn()
    {
        if (_currentTurn != null)
        {
            _currentTurn.DevCardPlayedThisTurn = true;
        }
    }

    public int FreeRoadsRemaining => _currentTurn?.FreeRoadsRemaining ?? 0;

    public void AddFreeRoads(int count)
    {
        if (_currentTurn != null)
        {
            _currentTurn.FreeRoadsRemaining += count;
        }
    }

    public void UseOneRoad()
    {
        if (_currentTurn != null && _currentTurn.FreeRoadsRemaining > 0)
        {
            _currentTurn.FreeRoadsRemaining--;
        }
    }

    public void Clear() => _currentTurn = null;
}
