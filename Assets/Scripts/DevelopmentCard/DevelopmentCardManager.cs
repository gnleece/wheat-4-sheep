using System.Collections.Generic;
using UnityEngine;

public class DevelopmentCardManager
{
    private const int KnightCount = 14;
    private const int VictoryPointCount = 5;
    private const int YearOfPlentyCount = 2;
    private const int MonopolyCount = 2;
    private const int RoadBuildingCount = 2;
    private const int MinKnightsForLargestArmy = 3;

    private readonly ResourceManager resourceManager;
    private readonly TurnManager turnManager;
    private readonly System.Random random;

    private List<DevelopmentCardType> deck = new List<DevelopmentCardType>();
    private Dictionary<IPlayer, DevelopmentCardHand> playerHands = new Dictionary<IPlayer, DevelopmentCardHand>();
    private Dictionary<IPlayer, int> knightsPlayed = new Dictionary<IPlayer, int>();
    private IPlayer largestArmyHolder = null;
    private List<IPlayer> allPlayers = new List<IPlayer>();

    public DevelopmentCardManager(ResourceManager resourceManager, TurnManager turnManager, System.Random random)
    {
        this.resourceManager = resourceManager;
        this.turnManager = turnManager;
        this.random = random;
    }

    public void Initialize(IEnumerable<IPlayer> players)
    {
        allPlayers = new List<IPlayer>(players);
        playerHands.Clear();
        knightsPlayed.Clear();
        largestArmyHolder = null;

        foreach (var player in allPlayers)
        {
            playerHands[player] = new DevelopmentCardHand();
            knightsPlayed[player] = 0;
        }

        BuildAndShuffleDeck();
    }

    private void BuildAndShuffleDeck()
    {
        deck.Clear();

        for (int i = 0; i < KnightCount; i++) deck.Add(DevelopmentCardType.Knight);
        for (int i = 0; i < VictoryPointCount; i++) deck.Add(DevelopmentCardType.VictoryPoint);
        for (int i = 0; i < YearOfPlentyCount; i++) deck.Add(DevelopmentCardType.YearOfPlenty);
        for (int i = 0; i < MonopolyCount; i++) deck.Add(DevelopmentCardType.Monopoly);
        for (int i = 0; i < RoadBuildingCount; i++) deck.Add(DevelopmentCardType.RoadBuilding);

        // Fisher-Yates shuffle
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }

    public bool IsDeckEmpty => deck.Count == 0;

    public bool CanBuyDevelopmentCard(IPlayer player)
    {
        if (!turnManager.IsPlayerTurn(player)) return false;
        if (!turnManager.HasRolledDice) return false;
        if (IsDeckEmpty) return false;
        if (!resourceManager.HasEnoughResources(player, BuildingCosts.DevelopmentCardCost)) return false;
        return true;
    }

    public DevelopmentCardType BuyDevelopmentCard(IPlayer player)
    {
        if (!CanBuyDevelopmentCard(player))
        {
            Debug.LogError($"Player {player.PlayerId} cannot buy a development card");
            return default;
        }

        resourceManager.DeductResources(player, BuildingCosts.DevelopmentCardCost);

        var card = deck[deck.Count - 1];
        deck.RemoveAt(deck.Count - 1);

        playerHands[player].Add(card, 1);
        turnManager.SetDevCardBoughtThisTurn();

        Debug.Log($"Player {player.PlayerId} bought a {card} card. Deck has {deck.Count} cards remaining.");
        return card;
    }

    public bool CanPlayDevCard(IPlayer player, DevelopmentCardType cardType)
    {
        if (!turnManager.IsPlayerTurn(player)) return false;
        if (turnManager.DevCardPlayedThisTurn) return false;
        if (!playerHands.TryGetValue(player, out var hand) || !hand.HasCard(cardType)) return false;

        // Cards bought this turn cannot be played
        if (turnManager.DevCardBoughtThisTurn && hand.GetCount(cardType) <= 1)
        {
            // Only block if the one card they have was bought this turn.
            // We track this conservatively: if they bought a card this turn,
            // they cannot play any card of the same type until next turn.
            // (Strict Catan rule: the specific card bought cannot be played.)
            return false;
        }

        // Knight can be played before rolling dice; others require it
        if (cardType != DevelopmentCardType.Knight && !turnManager.HasRolledDice) return false;

        // VP cards cannot be actively played
        if (cardType == DevelopmentCardType.VictoryPoint) return false;

        return true;
    }

    public bool CanPlayAnyDevCard(IPlayer player)
    {
        foreach (DevelopmentCardType cardType in System.Enum.GetValues(typeof(DevelopmentCardType)))
        {
            if (CanPlayDevCard(player, cardType)) return true;
        }
        return false;
    }

    // Removes the card from the player's hand and records the play.
    // The BoardManager is responsible for executing card effects.
    public bool ConsumeDevCard(IPlayer player, DevelopmentCardType cardType)
    {
        if (!CanPlayDevCard(player, cardType))
        {
            Debug.LogError($"Player {player.PlayerId} cannot play {cardType}");
            return false;
        }

        playerHands[player].Remove(cardType, 1);
        turnManager.SetDevCardPlayedThisTurn();

        Debug.Log($"Player {player.PlayerId} played a {cardType} card.");
        return true;
    }

    public Dictionary<DevelopmentCardType, int> GetPlayerHand(IPlayer player)
    {
        if (playerHands.TryGetValue(player, out var hand))
        {
            return hand.GetAll();
        }
        return new Dictionary<DevelopmentCardType, int>();
    }

    public int GetVictoryPointBonus(IPlayer player)
    {
        if (!playerHands.TryGetValue(player, out var hand)) return 0;
        return hand.GetCount(DevelopmentCardType.VictoryPoint);
    }

    public int GetLargestArmyBonus(IPlayer player)
    {
        return largestArmyHolder == player ? 2 : 0;
    }

    public void RecordKnightPlayed(IPlayer player)
    {
        if (!knightsPlayed.ContainsKey(player)) return;

        knightsPlayed[player]++;
        int playerKnights = knightsPlayed[player];

        Debug.Log($"Player {player.PlayerId} has now played {playerKnights} knight(s).");

        if (playerKnights < MinKnightsForLargestArmy) return;

        int holderKnights = largestArmyHolder != null ? knightsPlayed[largestArmyHolder] : 0;
        if (playerKnights > holderKnights)
        {
            if (largestArmyHolder != player)
            {
                largestArmyHolder = player;
                Debug.Log($"Player {player.PlayerId} claims Largest Army with {playerKnights} knights!");
            }
        }
    }

    public void GiveResourcesToPlayer(IPlayer player, ResourceType resourceType, int amount)
    {
        resourceManager.GiveResourcesToPlayer(player, resourceType, amount);
    }

    public void ExecuteMonopoly(IPlayer player, ResourceType resourceType)
    {
        var totalStolen = 0;
        foreach (var otherPlayer in allPlayers)
        {
            if (otherPlayer == player) continue;

            var hand = resourceManager.GetHand(otherPlayer);
            if (hand == null) continue;

            int count = hand.GetCount(resourceType);
            if (count <= 0) continue;

            hand.Remove(resourceType, count);
            resourceManager.GiveResourcesToPlayer(player, resourceType, count);
            totalStolen += count;

            Debug.Log($"Player {player.PlayerId} stole {count} {resourceType} from Player {otherPlayer.PlayerId}");
        }

        Debug.Log($"Player {player.PlayerId} Monopoly total: {totalStolen} {resourceType}");
    }
}
