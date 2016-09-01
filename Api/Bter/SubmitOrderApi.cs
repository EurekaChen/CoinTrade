using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Eureka.CoinTrade.Api.Bter
{
    [Export(typeof(ISubmitOrderApi))]
    public class SubmitOrderApi : AuthApi, ISubmitOrderApi
    {
        public OrderResult SubmitOrder(CurrencyPair pair, Order order)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            string pairText = pair.Base.Code.ToLower() + "_" + pair.Quote.Code.ToLower();
            args.Add("pair", pairText);

            if (order.OrderType == OrderType.Buy)
            {
                args.Add("type", "BUY");
            }
            if (order.OrderType == OrderType.Sell)
            {
                args.Add("type", "SELL");
            }

            args.Add("rate", order.PriceQuantity.OrginalPrice.ToString());
            args.Add("amount", order.PriceQuantity.Quantity.ToString());

            string result = AuthQuery("1/private/placeorder", args);

            return GetResult(result);
        }


        /// 成功：
        /// {
        ///     "result":true,"msg":"Success","order_id":595750
        /// }
        /// 失败：
        /// {
        ///     "result":false,"msg":"Error: you don't have enough fund"
        /// }

        private static OrderResult GetResult(string json)
        {
            OrderResult orderResult = new OrderResult();

            JObject jObject = JObject.Parse(json);
            if (jObject["msg"].ToString() == "Success")
            {
                var def = new { result = true, order_id = 0, msg = "" };
                var result = JsonConvert.DeserializeAnonymousType(json, def);
                orderResult.OrderId = result.order_id;
                orderResult.IsSuccess = result.result;
                orderResult.Info = Resources.OrderSuccess + "：" + result.msg;
                return orderResult;
            }
            else
            {
                orderResult.IsSuccess = false;
                orderResult.Info = jObject["msg"].ToString();
                orderResult.OrderId = -1;
                return orderResult;
            }

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
