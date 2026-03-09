public class PortAwareBankTradeRateProvider : IBankTradeRateProvider
{
    private readonly IBoardManager _boardManager;

    public PortAwareBankTradeRateProvider(IBoardManager boardManager)
    {
        _boardManager = boardManager;
    }

    public int GetBankTradeRate(IPlayer player, ResourceType resourceType)
    {
        var bestRate = 4;

        foreach (var vertex in _boardManager.VertexMap.Values)
        {
            if (vertex.Owner != player || vertex.Port == null)
            {
                continue;
            }

            var portRate = vertex.Port.GetTradeRate(resourceType);
            if (portRate < bestRate)
            {
                bestRate = portRate;
            }
        }

        return bestRate;
    }
}
