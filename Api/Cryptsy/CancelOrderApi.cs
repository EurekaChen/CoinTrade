using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Cryptsy
{
    [Export(typeof(ICancelOrderApi))]
    public class CancelOrderApi : AuthApi, ICancelOrderApi
    {

        /// <remarks>
        /// 把cancelorder写错时既然也返回了真值：
        /// {"success":"1","return":[]}
        /// 反回应该为:
        /// {"success":"1","return":"Your order #13990764 has been cancelled."}
        /// </remarks>
        public OrderResult CancelOrder(int orderlId)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();

            args.Add("orderid", orderlId.ToString());
            string result = AuthQuery("cancelorder", args);
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
