using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.MtGox
{
    [Export(typeof(IOpenOrdersApi))]
    public class OpenOrdersApi : AuthApi, IOpenOrdersApi
    {
        /// <remarks>
        /// {
        ///     "result":"success",
        ///     "data":
        ///     [
        ///         {
        ///             "oid":"2ecd4473-be85-4a4a-a637-5d67869f6446",
        ///             "currency":"USD",
        ///             "item":"BTC",
        ///             "type":"ask",
        ///             "amount":{"value":"0.01000000","value_int":"1000000","display":"0.01000000\u00a0BTC","display_short":"0.01\u00a0BTC","currency":"BTC"},
        ///             "effective_amount":{"value":"0.00500000","value_int":"500000","display":"0.00500000\u00a0BTC","display_short":"0.01\u00a0BTC","currency":"BTC"},
        ///             "price":{"value":"695.00716","value_int":"69500716","display":"$695.00716","display_short":"$695.01","currency":"USD"},
        ///             "status":"open",
        ///             "date":1384592457,
        ///             "priority":"1384592458011800",
        ///             "actions":[],
        ///             "invalid_amount":{"value":"0.00500000","value_int":"500000","display":"0.00500000\u00a0BTC","display_short":"0.01\u00a0BTC","currency":"BTC" }
        ///         }
        ///     ]
        /// }
        /// </remarks>  
        /// 注：好象返回的订单只有一个！好象是自动取消了最近的订单！
        public ObservableCollection<Order> QueryOpenOrders(CurrencyPair pair)
        {

            string json = AuthQuery(pair.Code + "/money/orders", new Dictionary<string, string>());

            ObservableCollection<Order> orders = new ObservableCollection<Order>();
            JObject jObject = JObject.Parse(json);
            JArray jArray = JArray.Parse(jObject["data"].ToString());
            var def = new
            {
                oid = "",
                currency = "",
                item = "",
                type = "",
                amount = new ValueItem(),
                effective_amount = new ValueItem(),
                price = new ValueItem(),
                status = "",
                date = 0,
                priority = "",
                invalid_amount = new ValueItem()
            };
            foreach (var item in jArray)
            {
                var result = JsonConvert.DeserializeAnonymousType(item.ToString(), def);
                Order order = new Order();
                order.OrderTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(result.date);
                if (result.type == "ask")
                {
                    order.OrderType = OrderType.Sell;
                }
                if (result.type == "bid")
                {
                    order.OrderType = OrderType.Buy;
                }
                if (!OrderIds.Singleton.Contains(result.oid))
                {
                    OrderIds.Singleton.Add(result.oid);
                }
                int orderId = OrderIds.Singleton.IndexOf(result.oid) + 1;
                order.Id = orderId;
                order.PriceQuantity = new PriceQuantityItem();
                order.PriceQuantity.Quantity = result.amount.value;
                order.PriceQuantity.Price = result.price.value;
                orders.Add(order);
            }
            return orders;
        }

    }
}
