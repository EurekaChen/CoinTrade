using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Btce
{
    [Export(typeof(IOpenOrdersApi))]
    public class OpenOrdersApi : AuthApi, IOpenOrdersApi
    {
        /// <remarks>
        /// {
        ///     "success":1,
        ///     "return":
        ///     {
        ///         "61727591":
        ///         {"pair":"xpm_btc","type":"sell","amount":5.00000000,"rate":0.02000000,"timestamp_created":1384663571,"status":0},
        ///         "66767908":
        ///         {"pair":"xpm_btc","type":"sell","amount":10.00000000,"rate":0.02000000,"timestamp_created":1385127001,"status":0}
        ///     }
        /// }
        /// </remarks>  
        /// 
        public ObservableCollection<Order> QueryOpenOrders(CurrencyPair pair)
        {
            string pairText = pair.Base.Code.ToLower() + "_" + pair.Quote.Code.ToLower();
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("pair", pairText);
            string json = AuthQuery("ActiveOrders", args);

            ObservableCollection<Order> orders = new ObservableCollection<Order>();
            JObject jObject = JObject.Parse(json);
            string returnText = jObject["return"].ToString();

            var def = new { pair = "", type = "", amount = 0m, rate = 0m, timestamp_created = 0, status = 0 };
            var orderDict = JsonConvert.DeserializeObject<Dictionary<int, object>>(returnText);
            foreach (var item in orderDict)
            {
                var orderJson = item.Value.ToString();
                var orderContent = JsonConvert.DeserializeAnonymousType(orderJson, def);
                Order order = new Order();
                order.Id = item.Key;
                order.OrderTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(orderContent.timestamp_created);
                if (orderContent.type == "buy")
                {
                    order.OrderType = OrderType.Buy;
                }
                if (orderContent.type == "sell")
                {
                    order.OrderType = OrderType.Sell;
                }
                order.PriceQuantity = new PriceQuantityItem();
                order.PriceQuantity.Quantity = orderContent.amount;
                order.PriceQuantity.Price = orderContent.rate;
                orders.Add(order);
            }
            return orders;
        }
    }
}
