using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eureka.CoinTrade.Base;

namespace Eureka.CoinTrade.ApiBase
{
    public interface IExchangeRateApi
    {
        string ApiName { get; }
        void Update(ExchangeRate exchangeRate);        
    }
}
