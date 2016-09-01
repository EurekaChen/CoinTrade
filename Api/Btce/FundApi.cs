using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Btce
{
    [Export(typeof(IFundApi))]
    public class FundApi : AuthApi, IFundApi
    {
        public ExchangeFund QueryFund()
        {
            ExchangeFund exchangeFund = new ExchangeFund(Exchange);

            string result = AuthQuery("getInfo");

            JObject jObject = JObject.Parse(result);

            var avalilable = jObject["return"]["funds"].ToString();
            exchangeFund.Available = GetFund(avalilable);

            return exchangeFund;
        }

        /// 通过Tick获得的整数对Btce来说还是太大了！
        /// {"success":0,"error":"invalid nonce parameter; on key:0, you sent:17715774017712"}   
        /// {"success":0,"error":"invalid nonce parameter; on key:100, you sent:43"}
        /// 方法写成了getinfo.注意应该是getInfo
        /// {"success":0,"error":"invalid method"}
        /// 正确数据：
        /// {
        ///     "success":1,
        ///     "return":
        ///     {
        ///         "funds":
        ///         {
        ///             "usd":0,"btc":0,"ltc":0,"nmc":0,"rur":0,"eur":0,"nvc":0,"trc":0,"ppc":0,"ftc":0,"xpm":100
        ///         },
        ///         "rights":
        ///         {
        ///             "info":1,"trade":1,"withdraw":1
        ///         },
        ///         "transaction_count":1,
        ///         "open_orders":0,
        ///         "server_time":1384662887
        ///     }
        /// }
        /// 没有显示冻结资金！
        ///{"success":1,"return":{"funds":{"usd":0,"btc":0,"ltc":0,"nmc":0,"rur":0,"eur":0,"nvc":0,"trc":0,"ppc":0,"ftc":0,"xpm":95},"rights":{"info":1,"trade":1,"withdraw":1},"transaction_count":1,"open_orders":1,"server_time":1384664826}}
        private static Dictionary<Currency, decimal> GetFund(string json)
        {
            Dictionary<string, decimal> data = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(json);
            Dictionary<Currency, decimal> fund = new Dictionary<Currency, decimal>();
            foreach (var item in data)
            {
                if (item.Value == 0m) continue;
                string code = item.Key.ToUpper();
                fund.Add(Currency.All[code], item.Value);
            }
            return fund;
        }
    }
}
