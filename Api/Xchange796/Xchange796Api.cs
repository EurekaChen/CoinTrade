using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;

namespace Eureka.CoinTrade.Api.Xchange796
{
    [Export(typeof(ExchangeApi))]
    public class Xchange796cApi : ExchangeApi
    {
        public Xchange796cApi()
        {
            Exchange = GetExchange();
            SupportPairs = GetSupportPairs();
            SelectedPairs = GetSelectedPairs();
        }

        private Exchange GetExchange()
        {
            Exchange xchange796 = new Exchange();
            xchange796.AbbrName = "796Xchange";
            xchange796.Name = "796Xchange";
            xchange796.ChineseName = "796交易所";
            //广告联盟
            xchange796.ReferralUrl = "https://796.com/reg/index/100140";
            //xchange796.ReferralUrl = "https://796.com/invite/do/104163";
            xchange796.Url = "https://796.com";
            return xchange796;
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
