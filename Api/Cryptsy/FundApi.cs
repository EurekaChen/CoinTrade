using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Cryptsy
{
    [Export(typeof(IFundApi))]
    public class FundApi : AuthApi, IFundApi
    {
        public ExchangeFund QueryFund()
        {
            ExchangeFund exchangeFund = new ExchangeFund(Exchange);

            string result = AuthQuery("getinfo", new Dictionary<string, string>());

            JObject jObject = JObject.Parse(result);

            var avalilable = jObject["return"]["balances_available"].ToString();
            exchangeFund.Available = GetFund(avalilable);

            var fozen = jObject["return"]["balances_hold"].ToString();
            exchangeFund.Frozen = GetFund(fozen);

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
                //没有货币关键字Bug，C网点数                
                if (Currency.All.Contains(item.Key))
                {
                    fund.Add(Currency.All[item.Key], item.Value);
                }
            }
            return fund;
        }
    }
}
