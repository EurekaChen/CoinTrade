
namespace Eureka.CoinTrade.Base
{
    /// <summary>
    /// 指特定报价币的交易对。例如人民币市场对CNYAll指的是所有以人民币为报价币的所有货币对。
    /// </summary>
    public class MarketPair
    {
        public MarketPair(Currency quoteCurrency)
        {
            QuoteCurrency = quoteCurrency;
            QuotePairs = new QuotePairs(quoteCurrency);
            foreach (var item in CryptoCoin.All)
            {
                if (item.Code != quoteCurrency.Code)
                {
                    CurrencyPair currencyPair = CurrencyPair.All[item.Code + quoteCurrency.Code];
                    QuotePairs.Add(currencyPair);
                }
            }
        }
        public MarketPair(string code) : this(Currency.All[code]) { }

        public Currency QuoteCurrency { get; private set; }

        public QuotePairs QuotePairs { get; private set; }

        //以下内容已经移至 MaketPairs中了！
        //private static MarketPair usdAll = new MarketPair("USD");
        //public static MarketPair USDAll { get { return usdAll; } }

        //private static MarketPair cnyAll = new MarketPair("CNY");
        //public static MarketPair CNYAll { get { return cnyAll; } }

        //private static MarketPair btcAll = new MarketPair("BTC");
        //public static MarketPair BTCAll { get { return btcAll; } }

        //private static MarketPair ltcAll = new MarketPair("LTC");
        //public static MarketPair LTCAll { get { return ltcAll; } }

        //private static MarketPair xpmAll = new MarketPair("XPM");
        //public static MarketPair XPMAll { get { return xpmAll; } }



    }
}
