using System.Collections.Generic;

public readonly struct TradeOffer
{
    public readonly IPlayer Initiator;
    public readonly Dictionary<ResourceType, int> Offering;
    public readonly Dictionary<ResourceType, int> Requesting;

    public TradeOffer(IPlayer initiator, Dictionary<ResourceType, int> offering, Dictionary<ResourceType, int> requesting)
    {
        Initiator = initiator;
        Offering = offering;
        Requesting = requesting;
    }
}
