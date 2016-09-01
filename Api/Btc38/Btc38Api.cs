using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;

namespace Eureka.CoinTrade.Api.Btc38
{
    [Export(typeof(ExchangeApi))]
    public class Btc38Api : ExchangeApi
    {
        public Btc38Api()
        {
            Exchange = GetExchange();
            SupportPairs = GetSupportPairs();
            SelectedPairs = GetSelectedPairs();
        }

        private Exchange GetExchange()
        {
            Exchange Btc38 = new Exchange();
            Btc38.AbbrName = "Btc38";
            Btc38.Name = "Btc38";
            Btc38.ChineseName = "比特时代";
            Btc38.Url = "https://btc38.com";

            return Btc38;
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
