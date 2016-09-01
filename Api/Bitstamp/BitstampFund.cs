using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.MarketInfo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Bitstamp
{
    [Export(typeof(IFundQueryable))]
    public class BitstampFund:BitstampAuthApi,IFundQueryable
    {
        public ExchangeFund QueryFund()
        {
            ExchangeFund exchangeFund=new ExchangeFund(Exchange); 
            string result = AuthQuery("balance", new Dictionary<string, string>());       
            JObject jObject = JObject.Parse(result);

            var avalilable = jObject["return"]["balances_available"].ToString();
            exchangeFund.Available = GetFund(avalilable);

            var locked = jObject["return"]["balances_hold"].ToString();
            exchangeFund.Locked = GetFund(locked);
          
            return exchangeFund;
        }

        /// {
        ///     "success":"1",
        ///     "return":
        ///     {   
        ///         "balances_available":
        ///         {
        ///             "ALF":"0.00000000",
        ///             "AMC":"0.00000000",
        ///             "ADT":"152000000.00000000",
        ///             "ANC":"0.00000000",
        ///             ...
        ///             "LTC":"6.79582003",
        ///         },
        ///         "balances_hold":
        ///         {
        ///             "ALF":"0.00000000",
        ///              "ADT":"20000000.00000000",
        ///              ...
        ///          },
        ///          "servertimestamp":1383108863,
        ///          "servertimezone":"EST",
        ///          "serverdatetime":"2013-10-30 00:54:23",
        ///          "openordercount":1
        ///      }
        /// }            
        Dictionary<Currency, decimal> GetFund(string json)
        {
            Dictionary<string, decimal> data = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(json);
            Dictionary<Currency, decimal> fund = new Dictionary<Currency, decimal>();
            foreach (var item in data)
            {
                if (item.Value == 0m) continue;
                fund.Add(Currency.All[item.Key], item.Value);
            }
            return fund;
        }
    }
}
