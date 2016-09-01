using Eureka.CoinTrade.ApiBase;

namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    internal static class Utility
    {
        public static decimal GetPrice(PriceType priceType, Ticker ticker)
        {
            decimal price = 0m;
            switch (priceType)
            {
                case PriceType.Buy:
                    price = ticker.BidPrice;
                    break;
                case PriceType.Sell:
                    price = ticker.AskPrice;
                    break;
                case PriceType.High:
                    price = ticker.HighPrice;
                    break;
                case PriceType.Low:
                    price = ticker.LowPrice;
                    break;
                case PriceType.Last:
                    price = ticker.LastTradePrice;
                    break;
            }
            return price;
        }
    }
}
