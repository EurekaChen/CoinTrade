using System.Collections.ObjectModel;

namespace Eureka.CoinTrade.Base
{
    public class MarketPairs : KeyedCollection<string, MarketPair>
    {
        protected override string GetKeyForItem(MarketPair item)
        {
            return item.QuoteCurrency.Code;
        }

        private static MarketPairs all;
        public static MarketPairs All
        {
            get
            {
                if (all == null)
                {
                    all = new MarketPairs();
                    all.Add(new MarketPair("USD"));
                    all.Add(new MarketPair("CNY"));
                    all.Add(new MarketPair("BTC"));
                    all.Add(new MarketPair("LTC"));
                    all.Add(new MarketPair("XPM"));
                }
                return all;
            }
        }
    }
}
