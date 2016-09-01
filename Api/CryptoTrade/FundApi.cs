using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.CryptoTrade
{
    [Export(typeof(IFundApi))]
    public class FundApi : AuthApi, IFundApi
    {
        public ExchangeFund QueryFund()
        {

            string result = AuthQuery("1/private/getinfo", new Dictionary<string, string>());
            return GetFund(result);
        }


        /// {
        ///     "result":"true",
        ///     "available_funds":
        ///     {
        ///         "BTC":"1.52980541",
        ///         "LTC":"0.02",
        ///         "CNY":"495.871",
        ///         "XPM":"50",
        ///         "ZCC":"0"
        ///     },
        ///     "locked_funds":
        ///     {
        ///         "XPM":"50"
        ///     }
        /// }    
        /// 有可能没有数据：
        /// { "BTC": "0.52980541", "LTC": "0.02", "CNY": "495.871", "XPM": "100", "ZCC": "0" }

        private ExchangeFund GetFund(string result)
        {
            JObject jObject = JObject.Parse(result);
            ExchangeFund exchangeFund = new ExchangeFund(Exchange);
            if (jObject["available_funds"] != null)
            {
                var avalilable = jObject["available_funds"].ToString();
                exchangeFund.Available = GetFundDict(avalilable);
            }
            else
            {
                exchangeFund.Available = new Dictionary<Currency, decimal>();
            }
            if (jObject["locked_funds"] != null)
            {
                var lockFund = jObject["locked_funds"].ToString();
                exchangeFund.Frozen = GetFundDict(lockFund);
            }
            else
            {
                exchangeFund.Frozen = new Dictionary<Currency, decimal>();
            }
            return exchangeFund;
        }

        private Dictionary<Currency, decimal> GetFundDict(string json)
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
