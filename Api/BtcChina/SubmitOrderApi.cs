using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Properties;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.BtcChina
{
    [Export(typeof(ISubmitOrderApi))]
    public class SubmitOrderApi : AuthApi, ISubmitOrderApi
    {
        public OrderResult SubmitOrder(CurrencyPair pair, Order order)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            string method = string.Empty;
            if (order.OrderType == OrderType.Buy)
            {
                method = "buyOrder";
            }
            if (order.OrderType == OrderType.Sell)
            {
                method = "sellOrder";

            }
            args.Add("price", order.PriceQuantity.OrginalPrice.ToString());
            args.Add("amount", order.PriceQuantity.Quantity.ToString());

            string json = AuthQuery(method, args);

            //异常由调用者进行处理。
            return GetResult(json);
        }


        /// 成功：
        /// {  "result":true,"id":"1"}
        /// 错误：
        /// {
        ///     "error":
        ///     {
        ///         "code":-32004,"message":"Insufficient BTC balance","data":{}
        ///     },
        ///     "id":"1"
        /// }

        //注：该反馈内容不同其它交易所，不带有挂单ID号！
        private OrderResult GetResult(string json)
        {
            OrderResult orderResult = new OrderResult();
            JObject jObject = JObject.Parse(json);
            //注意：jObject["result"]返回的是JToken True 而不是 true!
            if (jObject["result"] != null && jObject["result"].ToString() == "True")
            {
                orderResult.IsSuccess = true;
                //=-1为不成功，=0为不清楚
                orderResult.OrderId = 0;
                orderResult.Info = Resources.OrderSuccess;
            }
            if (jObject["error"] != null)
            {
                orderResult.IsSuccess = false;
                orderResult.Info = jObject["error"]["message"].ToString();
                EventSourceLogger.Logger.SubmitOrderException(Exchange.Name, orderResult.Info);
            }
            return orderResult;
        }


        public decimal GetBuyFeePercentage(CurrencyPair pair)
        {
            return 0m;
        }

        public decimal GetSellFeePercentage(CurrencyPair pair)
        {
            return 0m;
        }
    }
}
