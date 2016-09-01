using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.CryptoTrade
{
    [Export(typeof(IOpenOrdersApi))]
    public class OpenOrdersApi : AuthApi, IOpenOrdersApi
    {

        /// <remarks>
        /// {
        ///     "result":true,
        ///     "orders":
        ///     [
        ///         {
        ///             "id":"15088",			
        ///             "sell_type":"BTC",
        ///             "buy_type":"LTC",	
        ///             "sell_amount":"0.39901357",
        ///             "buy_amount":"12.0"
        ///          },
        ///          {
        ///             "id":"15092",			
        ///             "sell_type":"LTC",
        ///             "buy_type":"BTC",	
        ///             "sell_amount":"13.0",
        ///             "buy_amount":"0.4210"
        ///         }
        ///    ]
        ///    "msg":"Success"
        /// }
        /// 
        /// 
        /// </remarks>
        public ObservableCollection<Order> QueryOpenOrders(CurrencyPair pair)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            string json = AuthQuery("1/private/orderlist", args);

            ObservableCollection<Order> orders = new ObservableCollection<Order>();
            JObject jObject = JObject.Parse(json);
            JArray jArray = JArray.Parse(jObject["orders"].ToString());
            var def = new { id = "", sell_type = "", buy_type = "", sell_amount = 0m, buy_amount = 0m };
            foreach (var item in jArray)
            {
                var result = JsonConvert.DeserializeAnonymousType(item.ToString(), def);
                if ((result.buy_type == pair.Base.Code && result.sell_type == pair.Quote.Code) ||
                    (result.buy_type == pair.Quote.Code && result.sell_type == pair.Base.Code))
                {
                    string orderId = result.id;
                    Order order = GetOrder(orderId);
                    orders.Add(order);
                }
            }
            return orders;
        }

        /// <remarks>
        /// {
        ///     "result":true,
        ///     "order":
        ///     {
        ///         "id":"15088",
        ///         "status":"cancelled",
        ///         "pair":"btc_cny",
        ///         "type":"sell",
        ///         "rate":811,
        ///         "amount":"0.39901357",
        ///         "initial_rate":811,
        ///         "initial_amount":"1"
        ///     },
        /// "msg":"Success"
        /// }
        /// </remarks>

        private Order GetOrder(string orderId)
        {
            Dictionary<string, string> orderArgs = new Dictionary<string, string>();
            orderArgs.Add("order_id", orderId);
            string json = AuthQuery("1/private/getorder", orderArgs);
            JObject jObject = JObject.Parse(json);
            Order order = new Order();
            order.Id = Convert.ToInt32(jObject["order"]["id"].ToString());
            if (jObject["order"]["type"].ToString() == "sell")
            {
                order.OrderType = OrderType.Sell;
            }
            if (jObject["order"]["type"].ToString() == "buy")
            {
                order.OrderType = OrderType.Buy;
            }
            PriceQuantityItem pq = new PriceQuantityItem();
            double price = Convert.ToDouble(jObject["order"]["rate"].ToString());
            pq.Price = Convert.ToDecimal(price);
            pq.Quantity = Convert.ToDecimal(jObject["order"]["amount"].ToString());
            order.PriceQuantity = pq;
            return order;
        }

    }
}
