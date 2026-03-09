public interface IBankTradeRateProvider
{
    int GetBankTradeRate(IPlayer player, ResourceType resourceType);
}

public class DefaultBankTradeRateProvider : IBankTradeRateProvider
{
    public int GetBankTradeRate(IPlayer player, ResourceType resourceType) => 4;
}
