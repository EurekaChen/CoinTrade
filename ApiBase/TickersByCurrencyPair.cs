using System.Collections.ObjectModel;

namespace Eureka.CoinTrade.ApiBase
{
    /// <summary>
    ///  指同一交易所的交易对报价。
    /// </summary>
    public  class TickersByCurrencyPair : KeyedCollection<string, Ticker>
    {
        protected override string GetKeyForItem(Ticker item)
        {
            return item.CurrencyPair.Code;
        }
    }
}
