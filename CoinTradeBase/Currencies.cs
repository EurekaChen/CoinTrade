using System.Collections.ObjectModel;

namespace Eureka.CoinTrade.Base
{
    public class Currencies : KeyedCollection<string, Currency>
    {
        protected override string GetKeyForItem(Currency item)
        {
            return item.Code;
        }  
    }
}
