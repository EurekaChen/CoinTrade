using System.Collections.ObjectModel;

namespace Eureka.CoinTrade.Base
{
    public class QuotePairs : KeyedCollection<string, CurrencyPair>
    {
        public QuotePairs(Currency baseCurrency)
        {
            this.BaseCurrency = baseCurrency;
        }

        public QuotePairs(string baseCode)
        {           
            BaseCurrency = Currency.All[baseCode];
        }

        public Currency BaseCurrency { get; set; }

        protected override string GetKeyForItem(CurrencyPair item)
        {
            return item.Base.Code;
        }
    }
}
