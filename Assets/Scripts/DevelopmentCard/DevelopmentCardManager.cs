using System.Collections.Generic;
using UnityEngine;

public class DevelopmentCardManager
{
    // TODO: Move all these values to a config
    private const int KnightCount = 14;
    private const int VictoryPointCount = 5;
    private const int YearOfPlentyCount = 2;
    private const int MonopolyCount = 2;
    private const int RoadBuildingCount = 2;
    private const int MinKnightsForLargestArmy = 3;

    private readonly ResourceManager _resourceManager;
    private readonly TurnManager _turnManager;
    private readonly IRandomProvider _random;

    private readonly List<DevelopmentCardType> _deck = new();
    
    private List<IPlayer> _allPlayers = new();
    private readonly Dictionary<IPlayer, DevelopmentCardHand> _playerHands = new();
    
    private readonly Dictionary<IPlayer, int> _knightsPlayed = new();
    private IPlayer _largestArmyHolder;

    public DevelopmentCardManager(ResourceManager resourceManager, TurnManager turnManager, IRandomProvider random)
    {
        _resourceManager = resourceManager;
        _turnManager = turnManager;
        _random = random;
    }

    public void Initialize(IEnumerable<IPlayer> players)
    {
        _allPlayers = new List<IPlayer>(players);
        _playerHands.Clear();
        _knightsPlayed.Clear();
        _largestArmyHolder = null;

        foreach (var player in _allPlayers)
        {
            _playerHands[player] = new DevelopmentCardHand();
            _knightsPlayed[player] = 0;
        }

        BuildAndShuffleDeck();
    }

    private void BuildAndShuffleDeck()
    {
        _deck.Clear();

        for (int i = 0; i < KnightCount; i++) _deck.Add(DevelopmentCardType.Knight);
        for (int i = 0; i < VictoryPointCount; i++) _deck.Add(DevelopmentCardType.VictoryPoint);
        for (int i = 0; i < YearOfPlentyCount; i++) _deck.Add(DevelopmentCardType.YearOfPlenty);
        for (int i = 0; i < MonopolyCount; i++) _deck.Add(DevelopmentCardType.Monopoly);
        for (int i = 0; i < RoadBuildingCount; i++) _deck.Add(DevelopmentCardType.RoadBuilding);

        Util.Shuffle(_deck, _random);
    }

    public bool IsDeckEmpty => _deck.Count == 0;

    public bool CanBuyDevelopmentCard(IPlayer player)
    {
        if (!_turnManager.IsPlayerTurn(player)) return false;
        if (!_turnManager.HasRolledDice) return false;
        if (IsDeckEmpty) return false;
        if (!_resourceManager.HasEnoughResources(player, BuildingCosts.DevelopmentCardCost)) return false;
        return true;
    }

    public DevelopmentCardType BuyDevelopmentCard(IPlayer player)
    {
        if (!CanBuyDevelopmentCard(player))
        {
            Debug.LogError($"Player {player.PlayerId} cannot buy a development card");
            return default;
        }

        _resourceManager.DeductResources(player, BuildingCosts.DevelopmentCardCost);

        var card = _deck[_deck.Count - 1];
        _deck.RemoveAt(_deck.Count - 1);

        _playerHands[player].Add(card, 1);
        _turnManager.SetDevCardBoughtThisTurn();

        Debug.Log($"Player {player.PlayerId} bought a {card} card. Deck has {_deck.Count} cards remaining.");
        return card;
    }

    public bool CanPlayDevCard(IPlayer player, DevelopmentCardType cardType)
    {
        if (!_turnManager.IsPlayerTurn(player)) return false;
        if (_turnManager.DevCardPlayedThisTurn) return false;
        if (!_playerHands.TryGetValue(player, out var hand) || !hand.HasCard(cardType)) return false;

        // Cards bought this turn cannot be played
        if (_turnManager.DevCardBoughtThisTurn && hand.GetCount(cardType) <= 1)
        {
            // Only block if the one card they have was bought this turn.
            // We track this conservatively: if they bought a card this turn,
            // they cannot play any card of the same type until next turn.
            // (Strict Catan rule: the specific card bought cannot be played.)
            return false;
        }

        // Knight can be played before rolling dice; others require it
        if (cardType != DevelopmentCardType.Knight && !_turnManager.HasRolledDice) return false;

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

        _playerHands[player].Remove(cardType, 1);
        _turnManager.SetDevCardPlayedThisTurn();

        Debug.Log($"Player {player.PlayerId} played a {cardType} card.");
        return true;
    }

    public Dictionary<DevelopmentCardType, int> GetPlayerHand(IPlayer player)
    {
        if (_playerHands.TryGetValue(player, out var hand))
        {
            return hand.GetAll();
        }
        return new Dictionary<DevelopmentCardType, int>();
    }

    public int GetVictoryPointBonus(IPlayer player)
    {
        if (!_playerHands.TryGetValue(player, out var hand)) return 0;
        return hand.GetCount(DevelopmentCardType.VictoryPoint);
    }

    public int GetLargestArmyBonus(IPlayer player)
    {
        return _largestArmyHolder == player ? 2 : 0;
    }

    public void RecordKnightPlayed(IPlayer player)
    {
        if (!_knightsPlayed.ContainsKey(player)) return;

        _knightsPlayed[player]++;
        int playerKnights = _knightsPlayed[player];

        Debug.Log($"Player {player.PlayerId} has now played {playerKnights} knight(s).");

        if (playerKnights < MinKnightsForLargestArmy) return;

        int holderKnights = _largestArmyHolder != null ? _knightsPlayed[_largestArmyHolder] : 0;
        if (playerKnights > holderKnights)
        {
            if (_largestArmyHolder != player)
            {
                _largestArmyHolder = player;
                Debug.Log($"Player {player.PlayerId} claims Largest Army with {playerKnights} knights!");
            }
        }
    }

    public void GiveResourcesToPlayer(IPlayer player, ResourceType resourceType, int amount)
    {
        _resourceManager.GiveResourcesToPlayer(player, resourceType, amount);
    }

    public void ExecuteMonopoly(IPlayer player, ResourceType resourceType)
    {
        var totalStolen = 0;
        foreach (var otherPlayer in _allPlayers)
        {
            if (otherPlayer == player) continue;

            var hand = _resourceManager.GetHand(otherPlayer);
            if (hand == null) continue;

            int count = hand.GetCount(resourceType);
            if (count <= 0) continue;

            hand.Remove(resourceType, count);
            _resourceManager.GiveResourcesToPlayer(player, resourceType, count);
            totalStolen += count;

            Debug.Log($"Player {player.PlayerId} stole {count} {resourceType} from Player {otherPlayer.PlayerId}");
        }

        Debug.Log($"Player {player.PlayerId} Monopoly total: {totalStolen} {resourceType}");
    }
}
