
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
namespace Eureka.CoinTrade.Api.Vircurex
{
    [Export(typeof(ExchangeApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VircurexApi : ExchangeApi
    {
        public VircurexApi()
        {
            Exchange = GetExchange();
            SupportPairs = GetSupportPairs();
            SelectedPairs = GetSelectedPairs();
        }

        private Exchange GetExchange()
        {
            Exchange vircurex = new Exchange();
            vircurex.AbbrName = "Vircurex";
            vircurex.Name = "Vircurex";
            vircurex.ReferralUrl = "https://vircurex.com/welcome/index?referral_id=225-31753";
            vircurex.Url = "https://vircurex.com";

            return vircurex;
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
