public class Port
{
    public PortType PortType { get; private set; }

    public Port(PortType portType)
    {
        PortType = portType;
    }

    /// <summary>
    /// Returns the bank trade rate this port provides for the given resource.
    /// Generic ports return 3 for any resource; specific ports return 2 for
    /// their resource and 4 for everything else.
    /// </summary>
    public int GetTradeRate(ResourceType resource)
    {
        if (PortType == PortType.Generic)
        {
            return 3;
        }

        var portResource = PortTypeToResourceType(PortType);
        return portResource == resource ? 2 : 4;
    }

    private static ResourceType PortTypeToResourceType(PortType portType)
    {
        switch (portType)
        {
            case PortType.Wood:  return ResourceType.Wood;
            case PortType.Clay:  return ResourceType.Clay;
            case PortType.Sheep: return ResourceType.Sheep;
            case PortType.Wheat: return ResourceType.Wheat;
            case PortType.Ore:   return ResourceType.Ore;
            default:             return ResourceType.None;
        }
    }
}
