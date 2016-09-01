
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
namespace Eureka.CoinTrade.Api.BtcTrade
{
    [Export(typeof(ExchangeApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BtcTradeApi : ExchangeApi
    {
        public BtcTradeApi()
        {
            Exchange = GetExchange();
            SupportPairs = GetSupportPairs();
            SelectedPairs = GetSelectedPairs();
            Data = PairIdDict;
        }

        private Exchange GetExchange()
        {
            Exchange btcTrade = new Exchange();
            btcTrade.AbbrName = "BtcTrade";
            btcTrade.Name = "BtcTrade";
            btcTrade.ChineseName = "比特币交易网";
            btcTrade.Url = "https://btctrade.com";

            return btcTrade;
        }

        private CurrencyPairs GetSupportPairs()
        {
            CurrencyPairs supportPairs = new CurrencyPairs();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetName().Name + ".SupportPair.txt";
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            using (TextReader textReader = new StreamReader(stream))
            {
                string json = textReader.ReadToEnd();
                PairIdDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                foreach (var item in PairIdDict)
                {
                    supportPairs.Add(CurrencyPair.All[item.Key]);
                }
            }
            return supportPairs;
        }

        protected Dictionary<string, int> PairIdDict { get; private set; }

    }
}
