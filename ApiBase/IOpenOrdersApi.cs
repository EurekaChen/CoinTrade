using Eureka.CoinTrade.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Eureka.CoinTrade.ApiBase
{
    public interface IOpenOrdersApi
    {
        ObservableCollection<Order> QueryOpenOrders(CurrencyPair pair);
    }
}
