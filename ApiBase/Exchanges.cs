using Eureka.CoinTrade.Base;
using System.Collections.ObjectModel;

namespace Eureka.CoinTrade.ApiBase
{
    public  class Exchanges : KeyedCollection<string, Exchange>
    {
        protected override string GetKeyForItem(Exchange item)
        {
            return item.AbbrName;
        }      
    }
}
