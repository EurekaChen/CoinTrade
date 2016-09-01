using Eureka.CoinTrade.Base;

namespace Eureka.CoinTrade.ApiBase
{
    public interface ISubmitOrderApi
    {
        OrderResult SubmitOrder(CurrencyPair pair, Order order);
        decimal GetBuyFeePercentage(CurrencyPair pair);
        decimal GetSellFeePercentage(CurrencyPair pair);
    }
}
