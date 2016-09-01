using Eureka.CoinTrade.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eureka.CoinTrade.ApiBase
{
    public interface ICancelOrderApi
    {    
        OrderResult CancelOrder(int dealId);
    }
}
