using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceManager
{
    private readonly IRandomProvider _random;
    private readonly Dictionary<IPlayer, ResourceHand> _playerResourceHands = new();
    
    public ResourceManager(IRandomProvider random)
    {
        _random = random;
    }

    public void Initialize(IEnumerable<IPlayer> players, Dictionary<ResourceType, int> extraStartingResources = null)
    {
        _playerResourceHands.Clear();
        foreach (var player in players)
        {
            _playerResourceHands[player] = new ResourceHand();
            if (extraStartingResources != null)
            {
                foreach (var kvp in extraStartingResources)
                {
                    if (kvp.Value > 0)
                    {
                        _playerResourceHands[player].Add(kvp.Key, kvp.Value);
                    }
                }
            }
        }
    }

    public bool ContainsPlayer(IPlayer player) => _playerResourceHands.ContainsKey(player);

    // Returns a copy of the resource hand for the given player, or null if not found
    public Dictionary<ResourceType, int> GetResourceHandForPlayer(IPlayer player)
    {
        if (_playerResourceHands.TryGetValue(player, out var hand))
        {
            return hand.GetAll(); // returns a copy
        }
        return null;
    }

    public ResourceHand GetHand(IPlayer player)
    {
        _playerResourceHands.TryGetValue(player, out var hand);
        return hand;
    }

    public bool HasEnoughResources(IPlayer player, Dictionary<ResourceType, int> cost)
    {
        var hand = _playerResourceHands.GetValueOrDefault(player);
        return hand != null && hand.HasEnoughResources(cost);
    }

    public void DeductResources(IPlayer player, Dictionary<ResourceType, int> cost)
    {
        if (_playerResourceHands.TryGetValue(player, out var hand))
        {
            hand.Remove(cost);
        }
    }

    public void GivePlayerResourcesForNeighborHexTiles(IPlayer player, HexVertex hexVertex)
    {
        if (player == null || hexVertex == null) return;
        if (!_playerResourceHands.TryGetValue(player, out var hand)) return;
        if (hexVertex.Building == null) return;

        int amount = hexVertex.Building.Type == Building.BuildingType.City ? 2 : 1;

        foreach (var hex in hexVertex.NeighborHexTiles)
        {
            var resourceType = hex.ResourceType;
            if (resourceType != ResourceType.None)
            {
                hand.Add(resourceType, amount);
            }
        }
    }

    public void GiveAllPlayersResourcesForHexTile(HexTile hexTile, HexTile robberTile)
    {
        if (hexTile == null) return;
        var resourceType = hexTile.ResourceType;
        if (resourceType == ResourceType.None) return;

        foreach (var vertex in hexTile.NeighborVertices)
        {
            if (vertex == null || vertex.Building == null || vertex.Owner == null) continue;
            if (hexTile == robberTile) continue;
            if (!_playerResourceHands.TryGetValue(vertex.Owner, out var hand)) continue;

            int amount = vertex.Building.Type == Building.BuildingType.City ? 2 : 1;
            hand.Add(resourceType, amount);

            Debug.Log($"Player {vertex.Owner.PlayerId} awarded {amount} {resourceType} from hex {hexTile.HexCoordinates}");
        }
    }

    public async Task HandleSevenRollDiscard()
    {
        Debug.Log("Seven rolled! Checking for players with more than 7 cards...");

        // Find all players with more than 7 cards
        var playersToDiscard = new List<IPlayer>();
        foreach (var kvp in _playerResourceHands)
        {
            var player = kvp.Key;
            var hand = kvp.Value;
            var totalCards = hand.GetAll().Values.Sum();

            if (totalCards > 7)
            {
                playersToDiscard.Add(player);
                Debug.Log($"Player {player.PlayerId} has {totalCards} cards and must discard {totalCards / 2}");
            }
        }

        // Handle discard for each player
        foreach (var player in playersToDiscard)
        {
            var hand = _playerResourceHands[player];
            var totalCards = hand.GetAll().Values.Sum();
            var cardsToDiscard = totalCards / 2;

            Debug.Log($"Player {player.PlayerId} must discard {cardsToDiscard} cards");

            await player.DiscardOnSevenRoll(hand, cardsToDiscard);
        }
    }

    public void GiveResourcesToPlayer(IPlayer player, ResourceType resourceType, int amount)
    {
        if (_playerResourceHands.TryGetValue(player, out var hand))
        {
            hand.Add(resourceType, amount);
        }
    }

    public ResourceType? StealRandomResourceFromPlayer(IPlayer fromPlayer, IPlayer toPlayer)
    {
        if (fromPlayer == null || toPlayer == null) return null;
        if (!_playerResourceHands.TryGetValue(fromPlayer, out var fromHand)) return null;
        if (!_playerResourceHands.TryGetValue(toPlayer, out var toHand)) return null;

        // Get all resources the player has
        var allResources = fromHand.GetAll();
        var resourcesList = new List<ResourceType>();

        // Create a list of all resources (including duplicates)
        foreach (var kvp in allResources)
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                resourcesList.Add(kvp.Key);
            }
        }

        if (resourcesList.Count == 0)
        {
            Debug.Log($"Player {fromPlayer.PlayerId} has no resources to steal");
            return null;
        }

        // Randomly select a resource to steal
        var randomIndex = _random.Next(resourcesList.Count);
        var resourceToSteal = resourcesList[randomIndex];

        // Remove from the victim and add to the thief
        fromHand.Remove(resourceToSteal, 1);
        toHand.Add(resourceToSteal, 1);

        Debug.Log($"Player {toPlayer.PlayerId} stole 1 {resourceToSteal} from Player {fromPlayer.PlayerId}");

        return resourceToSteal;
    }

    // Returns a string with the current resource hands of each player for debugging
    public string GetAllPlayerResourceHandsDebugString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var kvp in _playerResourceHands)
        {
            var player = kvp.Key;
            var hand = kvp.Value;
            sb.AppendLine($"Player {player.PlayerId}:");
            foreach (var resourceKvp in hand.GetAll())
            {
                sb.AppendLine($"  {resourceKvp.Key}: {resourceKvp.Value}");
            }
        }
        return sb.ToString();
    }
}
