using System.Collections.Generic;
using UnityEngine;

public class TradeManager
{
    private readonly ResourceManager _resourceManager;
    private readonly TurnManager _turnManager;
    private readonly IBankTradeRateProvider _bankRateProvider;
    
    private List<IPlayer> _allPlayers = new();

    public TradeManager(ResourceManager resourceManager, TurnManager turnManager, IBankTradeRateProvider bankRateProvider = null)
    {
        _resourceManager = resourceManager;
        _turnManager = turnManager;
        _bankRateProvider = bankRateProvider ?? new DefaultBankTradeRateProvider();
    }

    public void Initialize(IEnumerable<IPlayer> players)
    {
        _allPlayers = new List<IPlayer>(players);
    }

    public IReadOnlyList<IPlayer> GetOtherPlayers(IPlayer initiator)
    {
        var others = new List<IPlayer>();
        foreach (var player in _allPlayers)
        {
            if (player != initiator)
            {
                others.Add(player);
            }
        }
        return others;
    }

    public bool CanInitiateTrade(IPlayer player)
    {
        return _turnManager.IsPlayerTurn(player) && _turnManager.HasRolledDice;
    }

    public int GetBankTradeRate(IPlayer player, ResourceType resourceType)
    {
        return _bankRateProvider.GetBankTradeRate(player, resourceType);
    }

    public bool CanBankTrade(IPlayer player, ResourceType giving, ResourceType receiving)
    {
        if (!CanInitiateTrade(player))
        {
            return false;
        }

        if (giving == ResourceType.None || receiving == ResourceType.None || giving == receiving)
        {
            return false;
        }

        int rate = GetBankTradeRate(player, giving);
        var hand = _resourceManager.GetHand(player);
        return hand != null && hand.GetCount(giving) >= rate;
    }

    public void ExecuteBankTrade(IPlayer player, ResourceType giving, ResourceType receiving)
    {
        int rate = GetBankTradeRate(player, giving);
        _resourceManager.DeductResources(player, new Dictionary<ResourceType, int> { { giving, rate } });
        _resourceManager.GiveResourcesToPlayer(player, receiving, 1);
        Debug.Log($"Player {player.PlayerId} traded {rate} {giving} to bank for 1 {receiving}");
    }

    public bool CanExecutePlayerTrade(TradeOffer offer, IPlayer acceptor)
    {
        if (acceptor == offer.Initiator)
        {
            return false;
        }

        var acceptorHand = _resourceManager.GetHand(acceptor);
        if (acceptorHand == null)
        {
            return false;
        }

        // Acceptor must have what the initiator is requesting
        foreach (var kvp in offer.Requesting)
        {
            if (kvp.Value > 0 && acceptorHand.GetCount(kvp.Key) < kvp.Value)
            {
                return false;
            }
        }

        // Initiator must still have what they offered
        var initiatorHand = _resourceManager.GetHand(offer.Initiator);
        if (initiatorHand == null)
        {
            return false;
        }

        foreach (var kvp in offer.Offering)
        {
            if (kvp.Value > 0 && initiatorHand.GetCount(kvp.Key) < kvp.Value)
            {
                return false;
            }
        }

        return true;
    }

    public void ExecutePlayerTrade(TradeOffer offer, IPlayer acceptor)
    {
        // Initiator gives Offering, receives Requesting
        _resourceManager.DeductResources(offer.Initiator, offer.Offering);
        _resourceManager.DeductResources(acceptor, offer.Requesting);

        foreach (var kvp in offer.Requesting)
        {
            if (kvp.Value > 0)
            {
                _resourceManager.GiveResourcesToPlayer(offer.Initiator, kvp.Key, kvp.Value);
            }
        }

        foreach (var kvp in offer.Offering)
        {
            if (kvp.Value > 0)
            {
                _resourceManager.GiveResourcesToPlayer(acceptor, kvp.Key, kvp.Value);
            }
        }

        Debug.Log($"Player {offer.Initiator.PlayerId} traded with Player {acceptor.PlayerId}");
    }
}
