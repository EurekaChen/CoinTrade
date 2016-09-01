using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.BtcChina
{
    [Export(typeof(IFundApi))]
    public class FundApi : AuthApi, IFundApi
    {
        public ExchangeFund QueryFund()
        {
            string result = AuthQuery("getAccountInfo");
            return GetFund(result);
        }


        /// {
        ///     "result":
        ///     {
        ///         "profile":
        ///         {
        ///             "username":"cyj100",
        ///             "trade_password_enabled":true,
        ///             "otp_enabled":false,
        ///             "trade_fee":"0",
        ///             "daily_btc_limit":10,
        ///             "btc_deposit_address":"1PgSu942W13GBpJYAaKyMTEMFQTmsmsdsB",
        ///             "btc_withdrawal_address":""
        ///         },
        ///         "balance":
        ///         {
        ///             "btc":{"currency":"BTC","symbol":"\u0e3f","amount":"0.00100000","amount_integer":"100000","amount_decimal":8},
        ///             "cny":{"currency":"CNY","symbol":"\u00a5","amount":"0.00000","amount_integer":"0","amount_decimal":5}
        ///         },
        ///         "frozen":
        ///         {
        ///             "btc":{"currency":"BTC","symbol":"\u0e3f","amount":null,"amount_integer":"0","amount_decimal":8},
        ///             "cny":{"currency":"CNY","symbol":"\u00a5","amount":null,"amount_integer":"0","amount_decimal":5}
        ///         }
        ///     },
        ///     "id":"1"
        ///  }  

        private ExchangeFund GetFund(string result)
        {
            JObject jObject = JObject.Parse(result);

            var btcAvailableText = jObject["result"]["balance"]["btc"]["amount"].ToString();
            decimal btcAvailable = Convert.ToDecimal(btcAvailableText);
            var cnyAvailableText = jObject["result"]["balance"]["cny"]["amount"].ToString();
            decimal cnyAvailable = Convert.ToDecimal(cnyAvailableText);

            var btcFrozenText = jObject["result"]["frozen"]["btc"]["amount"].ToString();

            decimal btcFrozen = Convert.ToDecimal(btcFrozenText == "" ? "0" : btcFrozenText);
            var cnyFrozenText = jObject["result"]["frozen"]["cny"]["amount"].ToString();
            decimal cnyFrozen = Convert.ToDecimal(cnyFrozenText == "" ? "0" : cnyFrozenText);

            ExchangeFund exchangeFund = new ExchangeFund(Exchange);
            exchangeFund.Available = new Dictionary<Currency, decimal>() { { Currency.All["BTC"], btcAvailable }, { Currency.All["CNY"], cnyAvailable } };
            exchangeFund.Frozen = new Dictionary<Currency, decimal>() { { Currency.All["BTC"], btcFrozen }, { Currency.All["CNY"], cnyFrozen } };
            return exchangeFund;
        }

    }
}
