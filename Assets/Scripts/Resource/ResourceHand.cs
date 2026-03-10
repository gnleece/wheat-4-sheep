using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceHand
{
    private Dictionary<ResourceType, int> _resourceCounts = new Dictionary<ResourceType, int>();

    public ResourceHand()
    {
        // Initialize all resource types to 0 (except None)
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (type != ResourceType.None)
                _resourceCounts[type] = 0;
        }
    }

    public int GetCount(ResourceType type)
    {
        if (type == ResourceType.None) return 0;
        return _resourceCounts.TryGetValue(type, out int count) ? count : 0;
    }

    public void Add(ResourceType type, int amount)
    {
        if (type == ResourceType.None) return;
        if (!_resourceCounts.ContainsKey(type))
            _resourceCounts[type] = 0;
        _resourceCounts[type] += amount;
    }

    public void Remove(ResourceType type, int amount)
    {
        if (type == ResourceType.None) return;
        if (!_resourceCounts.ContainsKey(type))
            _resourceCounts[type] = 0;
        _resourceCounts[type] = Mathf.Max(0, _resourceCounts[type] - amount);
    }

    public void Remove(Dictionary<ResourceType, int> costs)
    {
        foreach (var cost in costs)
        {
            Remove(cost.Key, cost.Value);
        }
    }

    public Dictionary<ResourceType, int> GetAll()
    {
        // Return a copy to prevent external modification
        return new Dictionary<ResourceType, int>(_resourceCounts);
    }
    
    public bool HasEnoughResources(Dictionary<ResourceType, int> costs)
    {
        foreach (var cost in costs)
        {
            if (GetCount(cost.Key) < cost.Value)
                return false;
        }
        return true;
    }

    public static bool HasEnoughResources(Dictionary<ResourceType, int> hand, Dictionary<ResourceType, int> costs)
    {
        foreach (var cost in costs)
        {
            if (!hand.TryGetValue(cost.Key, out int count) || count < cost.Value)
                return false;
        }
        return true;
    }
} 