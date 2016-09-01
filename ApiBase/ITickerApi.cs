using Eureka.CoinTrade.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eureka.CoinTrade.ApiBase
{
    public interface ITickerApi
    {    
        void UpdateTicker(ref Ticker ticker);
    }
}
