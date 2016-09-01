using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.BtcChina
{
    [Export(typeof(ICancelOrderApi))]
    public class CancelOrderApi : AuthApi, ICancelOrderApi
    {

        /// <remarks>
        /// {"result":true,"id":"1"}
        /// </remarks>
        public OrderResult CancelOrder(int orderlId)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();

            args.Add("id", orderlId.ToString());
            string result = AuthQuery("cancelOrder", args);
            JObject jobject = JObject.Parse(result);
            OrderResult orderResult = new OrderResult();
            if (jobject["result"] != null && jobject["result"].ToString() == "true")
            {
                orderResult.OrderId = orderlId;
                orderResult.IsSuccess = true;
            }
            else
            {
                orderResult.IsSuccess = false;
            }
            return orderResult;
        }
    }
}
