public class PlayerTurn
{
    public IPlayer Player;
    public PlayerTurnType TurnType;
    public bool HasRolledDice = false;

    public bool CanEndTurn
    {
        get
        {
            switch (TurnType)
            {
                case PlayerTurnType.InitialPlacement:
                    // TODO: validate that player has successfully placed their settlement and road
                    return true;
                case PlayerTurnType.RegularTurn:
                    // In regular turns, players must roll the dice before they can end their turn
                    return HasRolledDice;
                default:
                    return false;
            }
        }
    }
}
