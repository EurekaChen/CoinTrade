using Eureka.CoinTrade.Base;
using System.Collections.ObjectModel;

namespace Eureka.CoinTrade.ApiBase
{
    /// <summary>
    /// 指同一对货币在不同交易所的报价。
    /// </summary>
    public class TickersByExchange : KeyedCollection<string, Ticker>
    {
        protected override string GetKeyForItem(Ticker item)
        {
            return item.Exchange.AbbrName;
        }

        public CurrencyPair CurrencyPair
        {
            get
            {
                if (this.Count > 0)
                {
                    return this[0].CurrencyPair;
                }
                return null;
            }
        }
    }
}
