using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.BtcChina
{
    [Export(typeof(IOpenOrdersApi))]
    public class OpenOrdersApi : AuthApi, IOpenOrdersApi
    {
        /// <remarks>
        /// {
        ///     "result":
        ///     { 
        ///         "order":        
        ///         [
        ///             {"id":2,"type":"bid","price":500,"currency":"cny","amount":0.9,"amount_original":0.9,"date":1377671476,status":"cancelled"},
        ///             {"id":3,"type":"bid","price":501,"currency":"cny","amount":0.8,"amount_original":0.8,"date":1377671475,"status":"cancelled"}
        ///         ],
        ///     }
        ///     "id":"1"    
        /// }
        /// 
        /// {
        ///     "result":
        ///     {
        ///         "order":[]
        ///     },
        ///     "id":"1"
        /// }
        /// </remarks>  
        /// 注：暂且只有BTCCNY，所以无需判断。
        /// 返回莫名错误：
        /// <html>
        /// <head>
        /// <META NAME="ROBOTS" CONTENT="NOINDEX, NOFOLLOW">
        /// </head>
        /// <iframe src="/_Incapsula_Resource?CWUDNSAI=23&incident_id=32000750349061940-889192843139089589&edet=12&cinfo=3da45782c753855e08000000" frameborder=0 width="100%" height="100%" marginheight="0px" marginwidth="0px">
        /// Request unsuccessful. Incapsula incident ID: 32000750349061940-889192843139089589
        /// </iframe>
        /// </html>
        public ObservableCollection<Order> QueryOpenOrders(CurrencyPair pair)
        {
            //默认{"openonly","true"}
            string json = AuthQuery("getOrders");
            JObject jObject = JObject.Parse(json);

            JArray jArray = JArray.Parse(jObject["result"]["order"].ToString());
            var def = new { id = 0, type = "", price = 0m, currency = "", amount = 0m, amount_original = 0m, date = 0, status = "" };

            ObservableCollection<Order> orders = new ObservableCollection<Order>();
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
                order.Id = result.id;
                order.PriceQuantity = new PriceQuantityItem();
                order.PriceQuantity.Quantity = result.amount;
                order.PriceQuantity.Price = result.price;
                orders.Add(order);
            }
            return orders;
        }
    }
}
