using System.Collections.Generic;

public class BuildingCosts
{
    public static readonly Dictionary<ResourceType, int> RoadCost = new Dictionary<ResourceType, int>
    {
        { ResourceType.Wood, 1 },
        { ResourceType.Clay, 1 }
    };

    public static readonly Dictionary<ResourceType, int> SettlementCost = new Dictionary<ResourceType, int>
    {
        { ResourceType.Wood, 1 },
        { ResourceType.Clay, 1 },
        { ResourceType.Wheat, 1 },
        { ResourceType.Sheep, 1 }
    };

    public static readonly Dictionary<ResourceType, int> CityCost = new Dictionary<ResourceType, int>
    {
        { ResourceType.Wheat, 2 },
        { ResourceType.Ore, 3 }
    };

    public static readonly Dictionary<ResourceType, int> DevelopmentCardCost = new Dictionary<ResourceType, int>
    {
        { ResourceType.Wheat, 1 },
        { ResourceType.Sheep, 1 },
        { ResourceType.Ore, 1 }
    };
}
