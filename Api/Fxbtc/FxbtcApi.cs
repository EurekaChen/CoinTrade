using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;

namespace Eureka.CoinTrade.Api.Fxbtc
{
    [Export(typeof(ExchangeApi))]
    public class FxbtcApi : ExchangeApi
    {
        public FxbtcApi()
        {
            Exchange = GetExchange();
            SupportPairs = GetSupportPairs();
            SelectedPairs = GetSelectedPairs();
        }

        private Exchange GetExchange()
        {
            Exchange btcltc = new Exchange();
            btcltc.AbbrName = "FXBTC";
            btcltc.Name = "FXBTC";
            btcltc.ChineseName = "FXBTC";
            btcltc.Url = "https://fxbtc.com";
            return btcltc;
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
                List<string> pairs = JsonConvert.DeserializeObject<List<string>>(json);
                foreach (string item in pairs)
                {
                    supportPairs.Add(CurrencyPair.All[item]);
                }
            }
            return supportPairs;
        }
    }
}
