
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.ApiBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.ComponentModel.Composition;
using System.Reflection;
using System.IO;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Events;
namespace Eureka.CoinTrade.Api.Btce
{
    [Export(typeof(ExchangeApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BtceApi : ExchangeApi
    {         
        public BtceApi()
        {
            Exchange = GetExchange();          
            SupportPairs = GetSupportPairs();
            SelectedPairs = GetSelectedPairs();          
        }       

        private Exchange GetExchange()
        {
            Exchange btce = new Exchange();
            btce.AbbrName = "Btce";
            btce.Name = "Btce";
            btce.Url = "https://btc-e.com";         
            return btce;
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
