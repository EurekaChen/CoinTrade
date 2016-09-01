using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Btce
{
    [Export(typeof(ICancelOrderApi))]
    public class CancelOrderApi : AuthApi, ICancelOrderApi
    {

        /// <remarks>
        /// {
        ///     "success":1,
        ///     "return":
        ///     {
        ///         "order_id":67171178,
        ///         "funds":{"usd":0,"btc":0,"ltc":0,"nmc":0,"rur":0,"eur":0,"nvc":0,"trc":0,"ppc":0,"ftc":0,"xpm":90}
        ///     }
        /// }
        /// </remarks>
        public OrderResult CancelOrder(int orderlId)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();

            args.Add("order_id", orderlId.ToString());
            string result = AuthQuery("CancelOrder", args);
            JObject jobject = JObject.Parse(result);
            OrderResult orderResult = new OrderResult();
            if (jobject["success"] != null && jobject["success"].ToString() == "1")
            {
                orderResult.IsSuccess = true;
                orderResult.OrderId = orderlId;
            }
            else
            {
                orderResult.IsSuccess = false;
            }
            return orderResult;
        }
    }
}
