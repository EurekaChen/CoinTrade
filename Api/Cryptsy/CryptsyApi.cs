
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
namespace Eureka.CoinTrade.Api.Cryptsy
{
    [Export(typeof(ExchangeApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CryptsyApi : ExchangeApi
    {
        public CryptsyApi()
        {
            Exchange = GetExchange();
            SupportPairs = GetSupportPairs();
            SelectedPairs = GetSelectedPairs();
            Data = PairIdDict;
        }

        private Exchange GetExchange()
        {
            Exchange cryptsy = new Exchange();
            cryptsy.AbbrName = "Cryptsy";
            cryptsy.Name = "Cryptsy";
            cryptsy.ReferralUrl = "https://www.cryptsy.com/users/register?refid=5029";
            cryptsy.Url = "https://cryptsy.com";

            return cryptsy;
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
