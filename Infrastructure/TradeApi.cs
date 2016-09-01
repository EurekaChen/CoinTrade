using System.Collections.Generic;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure.Properties;

namespace Eureka.CoinTrade.Infrastructure
{
    /// <summary>
    /// 由于使用了Setting，所以没放在ApiBase可移植库里
    /// </summary>
    public abstract class ExchangeApi
    {
        //注：预先获得的数据，这样在更新语言后不会列新这些文字。有需要时可以考虑在使用时获取。现在影响不大暂且不改。
        protected Dictionary<string, string> DataTypeDict = new Dictionary<string, string>()
        {
            {"History",Resources.TradeHistory},
            {"Ticker",Resources.Ticker},
            {"Quotation",Resources.Quotation},
            {"Fee", Resources.Fee},
            {"Fund",Resources.Fund}
        };
        public Exchange Exchange { get; protected set; }
        public CurrencyPairs SupportPairs { get; set; }
        public CurrencyPairs SelectedPairs { get; set; }

        private TickersByCurrencyPair selectedTickers;
        public TickersByCurrencyPair SelectedTickers
        {
            get
            {
                if (selectedTickers == null)
                {
                    selectedTickers = new TickersByCurrencyPair();
                    foreach (CurrencyPair pair in SelectedPairs)
                    {
                        Ticker ticker = new Ticker() { CurrencyPair = pair, Exchange = Exchange };
                        selectedTickers.Add(ticker);
                    }
                }
                return selectedTickers;
            }
        }

        protected CurrencyPairs GetSelectedPairs()
        {
            return Setting.Singleton.GetSelectedPairs(Exchange.AbbrName);
        }

        protected int Timeout
        {
            get
            {
                return Setting.Singleton.Option.Timeout;
            }
        }

        public object Data { get; set; }
    }
}
