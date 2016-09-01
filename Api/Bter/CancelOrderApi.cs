using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Bter
{
    [Export(typeof(ICancelOrderApi))]
    public class CancelOrderApi : AuthApi, ICancelOrderApi
    {

        /// <remarks>
        /// {"result":true,"msg":"Success"}
        /// </remarks>
        public OrderResult CancelOrder(int orderId)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("order_id", orderId.ToString());
            string result = AuthQuery("1/private/cancelorder", args);
            JObject jobject = JObject.Parse(result);
            OrderResult orderResult = new OrderResult();
            if (jobject["msg"] != null && jobject["msg"].ToString() == "Success")
            {
                orderResult.IsSuccess = true;
                orderResult.OrderId = orderId;
            }
            else
            {
                orderResult.IsSuccess = false;
            }
            return orderResult;
        }
    }
}
