using System.Collections.ObjectModel;

namespace Eureka.CoinTrade.Base
{
    public class CurrencyPairs : KeyedCollection<string, CurrencyPair>
    {
        protected override string GetKeyForItem(CurrencyPair item)
        {
            return item.Base.Code + item.Quote.Code;
        }        
    }
}
