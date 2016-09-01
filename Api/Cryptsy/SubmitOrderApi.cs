using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Cryptsy
{
    [Export(typeof(ISubmitOrderApi))]
    public class SubmitOrderApi : AuthApi, ISubmitOrderApi
    {
        public OrderResult SubmitOrder(CurrencyPair pair, Order order)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            int marketId = PairIdDict[pair.Code];
            args.Add("marketid", marketId.ToString());
            if (order.OrderType == OrderType.Buy)
            {
                args.Add("ordertype", "Buy");
            }
            if (order.OrderType == OrderType.Sell)
            {
                args.Add("ordertype", "Sell");
            }
            args.Add("quantity", order.PriceQuantity.Quantity.ToString());
            args.Add("price", order.PriceQuantity.OrginalPrice.ToString());
            string resultJson = AuthQuery("createorder", args);
            return GetResult(resultJson);
        }


        /// 成功：
        /// {
        ///     "success":"1",
        ///     "orderid":"11090388",
        ///     "moreinfo":"Your Buy order has been placed for 100.00000000 ADT @ 0.00000010 LTC<\/b> each.Order ID: 11090388<\/b>"
        /// }
        /// 失败：
        /// {
        ///     "success":"0","error":"Unknown Order Type. Must be \"Buy\" or \"Sell\""\
        /// }

        private OrderResult GetResult(string json)
        {
            OrderResult orderResult = new OrderResult();

            JObject jObject = JObject.Parse(json);
            if (jObject["success"].ToString() == "1")
            {
                var def = new { success = 0, orderid = 0, moreinfo = "" };
                var feedBack = JsonConvert.DeserializeAnonymousType(json, def);
                orderResult.OrderId = feedBack.orderid;
                orderResult.IsSuccess = true;
                orderResult.Info = feedBack.moreinfo;
                return orderResult;
            }
            else
            {
                orderResult.IsSuccess = false;
                orderResult.Info = jObject["error"].ToString();
                return orderResult;
            }
        }


        public decimal GetBuyFeePercentage(CurrencyPair pair)
        {
            return 0.2m;
        }

        public decimal GetSellFeePercentage(CurrencyPair pair)
        {
            return 0.3m;
        }
    }
}
