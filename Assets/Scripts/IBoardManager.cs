public enum BoardMode
{
    Idle,
    ChooseSettlementLocation,
    ChooseRoadLocation,
    ChooseSettlementToUpgrade,
    ChooseRobberLocation,
}

public enum PlayerAction
{
    None,
    RollDice,
    BuildSettlement,
    BuildRoad,
    UpgradeSettlementToCity,
    MoveRobber,
    Trade,
    PlayDevelopmentCard,
}

public enum PlayerTurnType
{
    InitialPlacement,
    RegularTurn
}

/// <summary>
/// Full board contract. Combines IBoardActions (mutating), IBoardQuery (read-only),
/// and IBoardSelection (async UI-driven selections) into a single interface so that
/// existing consumers continue to compile unchanged. Prefer the narrower sub-interfaces
/// when a consumer only needs one group.
/// </summary>
public interface IBoardManager : IBoardActions, IBoardQuery, IBoardSelection
{
}
