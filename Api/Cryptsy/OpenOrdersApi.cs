using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.ApiBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Cryptsy
{
    [Export(typeof(IOpenOrdersApi))]
    public class OpenOrdersApi:AuthApi,IOpenOrdersApi
    { 
        /// <remarks>
        /// {
        ///     "success":"1",
        ///     "return":
        ///     [
        ///         {"orderid":"11090388","created":"2013-10-31 01:25:14","ordertype":"Buy","price":"0.00000010","quantity":"100.00000000","orig_quantity":"100.00000000","total":"0.00001000"},             
        ///         {"orderid":"11091944","created":"2013-10-31 02:06:01","ordertype":"Sell","price":"0.00000034","quantity":"300.00000000","orig_quantity":"300.00000000","total":"0.00010200"},
        ///         {"orderid":"4789607","created":"2013-09-26 08:30:13","ordertype":"Sell","price":"0.00000044","quantity":"20000000.00000000","orig_quantity":"20000000.00000000","total":"8.80000000"}
        ///     ]
        /// }
        /// </remarks>  
        public ObservableCollection<Order> QueryOpenOrders(CurrencyPair pair)
        {
            string json = GetOpenDealsJson(pair);

            ObservableCollection<Order> orders = new ObservableCollection<Order>();
            JObject jObject = JObject.Parse(json);
            JArray jArray = JArray.Parse(jObject["return"].ToString());
            var def = new { orderid = 0, ordertype = "", price = 0m, quantity = 0m, orig_quantity = 0m };
            foreach (var item in jArray)
            {
                var result = JsonConvert.DeserializeAnonymousType(item.ToString(), def);
                Order order = new Order();
                order.OrderTime = DateTime.Parse(item["created"].ToString());
                if (result.ordertype == "Buy")
                {
                    order.OrderType = OrderType.Buy;
                }
                if (result.ordertype == "Sell")
                {
                    order.OrderType = OrderType.Sell;
                }
                order.Id = result.orderid;
                order.PriceQuantity = new PriceQuantityItem();
                order.PriceQuantity.Quantity = result.quantity;
                order.PriceQuantity.Price = result.price;
                //deal.DealOrder.ConvertRate = CurrentTicker.ConvertRate;
                orders.Add(order);
            }
            return orders;
        }

        private string GetOpenDealsJson(CurrencyPair pair)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            int marketId = PairIdDict[pair.Code];
            args.Add("marketid", marketId.ToString());
            return AuthQuery("myorders", args);
        }
    }
}
