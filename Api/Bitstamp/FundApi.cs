using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;

namespace Eureka.CoinTrade.Api.Bitstamp
{
    [Export(typeof(IFundApi))]
    public class FundApi : AuthApi, IFundApi
    {

        public ExchangeFund QueryFund()
        {
            string json = AuthQuery("balance", new Dictionary<string, string>());
            return GetFund(json);
        }

        /// {"error": "Invalid signature"}
        ///成功： {"btc_reserved": "0", "fee": "0.5000", "btc_available": "0", "usd_reserved": "0", "btc_balance": "0", "usd_balance": "0.00", "usd_available": "0.00"}
        private ExchangeFund GetFund(string json)
        {
            var def = new { btc_reserved = 0m, fee = 0m, btc_available = 0m, usd_reserved = 0m, usd_available = 0m };
            var result = JsonConvert.DeserializeAnonymousType(json, def);

            ExchangeFund exchangeFund = new ExchangeFund(Exchange);
            exchangeFund.Available = new Dictionary<Currency, decimal>();
            exchangeFund.Available.Add(Currency.All["BTC"], result.btc_available);
            exchangeFund.Available.Add(Currency.All["USD"], result.usd_available);

            exchangeFund.Frozen = new Dictionary<Currency, decimal>();
            exchangeFund.Frozen.Add(Currency.All["BTC"], result.btc_reserved);
            exchangeFund.Frozen.Add(Currency.All["USD"], result.usd_available);
            return exchangeFund;
        }
    }
}
