
namespace Eureka.CoinTrade.Base
{

    public class CurrencyPair
    {
        /// <summary>
        /// 货币对构造函数。只供CurrencyPairs调用，需要货币对直接访问CurrencyPair.All！
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <param name="quote"></param>
        private CurrencyPair(Currency baseCurrency, Currency quote)
        {
            this.Base = baseCurrency;
            this.Quote = quote;
        }

        public Currency Base { get; private set; }
        public Currency Quote { get; private set; }
        public string Code { get { return ToString(); } }

        public override string ToString()
        {
            return Base.Code + Quote.Code;
        }

        private static CurrencyPairs all;
        /// <summary>
        /// 所有的货币对。
        /// </summary>
        public static CurrencyPairs All
        {
            get
            {
                if (all == null)
                {
                    all = new CurrencyPairs();
                    foreach (var baseCurrency in Currency.All)
                    {
                        foreach (var quoteCurrency in Currency.All)
                        {
                            if (baseCurrency != quoteCurrency)
                            {
                                all.Add(new CurrencyPair(baseCurrency, quoteCurrency));
                            }
                        }
                    }
                }
                return all;
            }
        }
    }
}
