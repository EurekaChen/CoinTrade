using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.MtGox
{
    [Export(typeof(IFundApi))]
    public class FundApi : AuthApi, IFundApi
    {
        public ExchangeFund QueryFund()
        {
            string result = AuthQuery("money/info", new Dictionary<string, string>());
            return GetFund(result);
        }

        /// <remarks> 返回数据示例
        /// {
        ///     "result":"success",
        ///     "data":
        ///     {
        ///         "Login":"cyj100",
        ///         "Index":"561759",
        ///         "Id":"7e476c78-377e-4773-8321-41df0bc63a6c",
        ///         "Link":"M33561759X",
        ///         "Rights":["deposit","get_info","merchant","trade","withdraw"],
        ///         "Language":"en_US",
        ///         "Created":"2013-06-20 02:13:41",
        ///         "Last_Login":"2013-11-02 07:23:19",
        ///         "Wallets":
        ///         {
        ///             "BTC":
        ///             {
        ///                 "Balance":
        ///                 {
        ///                     "value":"0.00500000","value_int":"500000","display":"0.00500000\u00a0BTC","display_short":"0.01\u00a0BTC","currency":"BTC"
        ///                 },
        ///                 "Operations":1,
        ///                 "Daily_Withdraw_Limit":
        ///                 {
        ///                     "value":"100.00000000","value_int":"10000000000","display":"100.00000000\u00a0BTC","display_short":"100.00\u00a0BTC","currency":"BTC"
        ///                 },
        ///                 "Monthly_Withdraw_Limit":null,
        ///                 "Max_Withdraw":
        ///                 {
        ///                     "value":"100.00000000","value_int":"10000000000","display":"100.00000000\u00a0BTC","display_short":"100.00\u00a0BTC","currency":"BTC"
        ///                 },
        ///                 "Open_Orders":
        ///                 {
        ///                     "value":"0.00000000","value_int":"0","display":"0.00000000\u00a0BTC","display_short":"0.00\u00a0BTC","currency":"BTC"
        ///                 }
        ///             },
        ///             "USD":
        ///             {
        ///                 "Balance":
        ///                 {
        ///                     "value":"0.00000","value_int":"0","display":"$0.00000","display_short":"$0.00","currency":"USD"
        ///                 },
        ///                 "Operations":0,
        ///                 "Daily_Withdraw_Limit":
        ///                 {
        ///                     "value":"1000.00000","value_int":"100000000","display":"$1,000.00000","display_short":"$1,000.00","currency":"USD"
        ///                 },
        ///                 "Monthly_Withdraw_Limit":
        ///                 {
        ///                     "value":"10000.00000","value_int":"1000000000","display":"$10,000.00000","display_short":"$10,000.00","currency":"USD"
        ///                 },
        ///                 "Max_Withdraw":
        ///                 {
        ///                     "value":"1000.00000","value_int":"100000000","display":"$1,000.00000","display_short":"$1,000.00","currency":"USD"
        ///                 },
        ///                 "Open_Orders":
        ///                 {
        ///                     "value":"0.00000","value_int":"0","display":"$0.00000","display_short":"$0.00","currency":"USD"
        ///                 }
        ///             }
        ///         },
        ///         "Monthly_Volume":
        ///         {
        ///             "value":"0.00000000","value_int":"0","display":"0.00000000\u00a0BTC","display_short":"0.00\u00a0BTC","currency":"BTC"
        ///         },
        ///         "Trade_Fee":0.6
        ///     }
        /// } 
        /// </remarks>
        private ExchangeFund GetFund(string result)
        {
            JObject jObject = JObject.Parse(result);

            var available = new Dictionary<Currency, decimal>();
            var frozen = new Dictionary<Currency, decimal>();

            string btcAvailableText = jObject["data"]["Wallets"]["BTC"]["Balance"]["value"].ToString();
            decimal btcAvailable = Convert.ToDecimal(btcAvailableText);
            available.Add(Currency.All["BTC"], btcAvailable);

            string btcLockedText = jObject["data"]["Wallets"]["BTC"]["Open_Orders"]["value"].ToString();
            decimal btcLocked = Convert.ToDecimal(btcLockedText);
            frozen.Add(Currency.All["BTC"], btcLocked);

            string usdAvailableText = jObject["data"]["Wallets"]["USD"]["Balance"]["value"].ToString();
            decimal usdAvailable = Convert.ToDecimal(usdAvailableText);
            available.Add(Currency.All["USD"], usdAvailable);

            string usdLockedText = jObject["data"]["Wallets"]["USD"]["Open_Orders"]["value"].ToString();
            decimal usdLocked = Convert.ToDecimal(usdLockedText);
            frozen.Add(Currency.All["USD"], btcLocked);
            ExchangeFund exchangeFund = new ExchangeFund(Exchange);
            exchangeFund.Available = available;
            exchangeFund.Frozen = frozen;
            return exchangeFund;
        }
    }
}
