using System.Collections.Generic;

public class DevelopmentCardHand
{
    private readonly Dictionary<DevelopmentCardType, int> _cardCounts = new Dictionary<DevelopmentCardType, int>();

    public int GetCount(DevelopmentCardType type)
    {
        _cardCounts.TryGetValue(type, out int count);
        return count;
    }

    public bool HasCard(DevelopmentCardType type) => GetCount(type) > 0;

    public void Add(DevelopmentCardType type, int amount)
    {
        if (!_cardCounts.ContainsKey(type))
        {
            _cardCounts[type] = 0;
        }
        _cardCounts[type] += amount;
    }

    public void Remove(DevelopmentCardType type, int amount)
    {
        if (!_cardCounts.ContainsKey(type) || _cardCounts[type] < amount)
        {
            throw new System.InvalidOperationException($"Cannot remove {amount} {type} cards: only {GetCount(type)} available.");
        }
        _cardCounts[type] -= amount;
    }

    // Returns a copy of all card counts (including types with 0 count are excluded)
    public Dictionary<DevelopmentCardType, int> GetAll()
    {
        var result = new Dictionary<DevelopmentCardType, int>();
        foreach (var kvp in _cardCounts)
        {
            if (kvp.Value > 0)
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        return result;
    }
}
