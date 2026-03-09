using System.Collections.Generic;

public class DevelopmentCardHand
{
    private readonly Dictionary<DevelopmentCardType, int> cardCounts = new Dictionary<DevelopmentCardType, int>();

    public int GetCount(DevelopmentCardType type)
    {
        cardCounts.TryGetValue(type, out int count);
        return count;
    }

    public bool HasCard(DevelopmentCardType type) => GetCount(type) > 0;

    public void Add(DevelopmentCardType type, int amount)
    {
        if (!cardCounts.ContainsKey(type))
        {
            cardCounts[type] = 0;
        }
        cardCounts[type] += amount;
    }

    public void Remove(DevelopmentCardType type, int amount)
    {
        if (!cardCounts.ContainsKey(type) || cardCounts[type] < amount)
        {
            throw new System.InvalidOperationException($"Cannot remove {amount} {type} cards: only {GetCount(type)} available.");
        }
        cardCounts[type] -= amount;
    }

    // Returns a copy of all card counts (including types with 0 count are excluded)
    public Dictionary<DevelopmentCardType, int> GetAll()
    {
        var result = new Dictionary<DevelopmentCardType, int>();
        foreach (var kvp in cardCounts)
        {
            if (kvp.Value > 0)
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        return result;
    }
}
