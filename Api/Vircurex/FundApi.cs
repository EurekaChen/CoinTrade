using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;

namespace Eureka.CoinTrade.Api.Vircurex
{
    [Export(typeof(IFundApi))]
    public class FundApi : AuthApi, IFundApi
    {
        public ExchangeFund QueryFund()
        {
            ExchangeFund exchangeFund = new ExchangeFund(Exchange); ;

            string result = AuthQuery("get_balances", new Dictionary<string, string>());

            return exchangeFund;
        }



    }
}
