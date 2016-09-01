using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Bitstamp
{
    [Export(typeof(ISubmitOrderApi))]
    public class SubmitOrderApi : AuthApi, ISubmitOrderApi
    {
        public OrderResult SubmitOrder(CurrencyPair pair, Order order)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            string pairText = pair.Base.Code.ToLower() + "_" + pair.Quote.Code.ToLower();

            args.Add("amount", order.PriceQuantity.Quantity.ToString());
            args.Add("price", order.PriceQuantity.OrginalPrice.ToString());
            string result = string.Empty;
            if (order.OrderType == OrderType.Buy)
            {
                result = AuthQuery("buy", args);
            }
            if (order.OrderType == OrderType.Sell)
            {
                result = AuthQuery("sell", args);
            }
            //{"error": {"__all__": ["You need $201.0 to open that order. You have only $0.0 available. Check your account balance for details."]}}
            return GetResult(result);
        }


        /// 成功：
        /// {
        ///     "result":true,"msg":"Success","order_id":595750
        /// }

        private OrderResult GetResult(string json)
        {
            OrderResult orderResult = new OrderResult();

            try
            {
                JObject jObject = JObject.Parse(json);
                if (jObject["msg"].ToString() == "Success")
                {
                    var def = new { result = true, order_id = 0, msg = "" };
                    var feedBack = JsonConvert.DeserializeAnonymousType(json, def);
                    orderResult.OrderId = feedBack.order_id;
                    orderResult.IsSuccess = feedBack.result;
                    orderResult.Info = feedBack.msg;
                    EventSourceLogger.Logger.LogSubmitOrderSuccess(Exchange.Name, feedBack.msg);
                    return orderResult;
                }
                else
                {
                    orderResult.IsSuccess = false;
                    orderResult.Info = jObject["error"].ToString();
                    EventSourceLogger.Logger.LogSubmitOrderFail(Exchange.Name, orderResult.Info);
                    return orderResult;
                }
            }
            catch
            {
                orderResult.IsSuccess = false;
                orderResult.Info = "返回的不是json数据，返回信息为：" + json;
                EventSourceLogger.Logger.LogSubmitOrderFail(Exchange.AbbrName, orderResult.Info);
            }
            return orderResult;

        }


        public decimal BuyFeePercentage
        {
            get { return 0m; }
        }

        public decimal SellFeePercentage
        {
            get { return 0m; }
        }
    }
}
