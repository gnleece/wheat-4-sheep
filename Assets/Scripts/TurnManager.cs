using UnityEngine;

public class TurnManager
{
    private PlayerTurn currentTurn = null;

    public IPlayer CurrentPlayer => currentTurn?.Player;
    public PlayerTurnType? CurrentTurnType => currentTurn?.TurnType;
    public bool HasRolledDice => currentTurn?.HasRolledDice ?? false;
    public bool DevCardBoughtThisTurn => currentTurn?.DevCardBoughtThisTurn ?? false;
    public bool DevCardPlayedThisTurn => currentTurn?.DevCardPlayedThisTurn ?? false;
    public bool IsActive => currentTurn != null;

    public bool BeginTurn(IPlayer player, PlayerTurnType turnType)
    {
        if (currentTurn != null)
        {
            Debug.LogError($"Cannot begin player turn: a player turn is already in progress for player {currentTurn.Player.PlayerId}");
            return false;
        }

        currentTurn = new PlayerTurn { Player = player, TurnType = turnType };
        Debug.Log($"Player {player.PlayerId} turn started");
        return true;
    }

    public bool EndTurn(IPlayer player)
    {
        if (currentTurn == null || currentTurn.Player != player)
        {
            Debug.LogError($"Cannot end player turn: no player turn in progress for player {player.PlayerId}");
            return false;
        }

        if (!currentTurn.CanEndTurn)
        {
            Debug.LogError($"Cannot end player turn");
            return false;
        }

        Debug.Log($"Player {player.PlayerId} turn ended");
        currentTurn = null;
        return true;
    }

    public bool IsPlayerTurn(IPlayer player) => currentTurn != null && currentTurn.Player == player;

    public bool CanEndTurn(IPlayer player)
    {
        if (currentTurn == null || currentTurn.Player != player) return false;
        return currentTurn.CanEndTurn;
    }

    public void SetHasRolledDice()
    {
        if (currentTurn != null)
        {
            currentTurn.HasRolledDice = true;
        }
    }

    public void SetDevCardBoughtThisTurn()
    {
        if (currentTurn != null)
        {
            currentTurn.DevCardBoughtThisTurn = true;
        }
    }

    public void SetDevCardPlayedThisTurn()
    {
        if (currentTurn != null)
        {
            currentTurn.DevCardPlayedThisTurn = true;
        }
    }

    public int FreeRoadsRemaining => currentTurn?.FreeRoadsRemaining ?? 0;

    public void AddFreeRoads(int count)
    {
        if (currentTurn != null)
        {
            currentTurn.FreeRoadsRemaining += count;
        }
    }

    public void UseOneRoad()
    {
        if (currentTurn != null && currentTurn.FreeRoadsRemaining > 0)
        {
            currentTurn.FreeRoadsRemaining--;
        }
    }

    public void Clear() => currentTurn = null;
}
