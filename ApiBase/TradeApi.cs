using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.ApiBase;
using Newtonsoft.Json;


namespace Eureka.CoinTrade.ApiBase
{
    public abstract class ExchangeApi
    {              
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

        public object Data { get; set; }
    }
}
