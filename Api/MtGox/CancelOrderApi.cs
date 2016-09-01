using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.MtGox
{
    [Export(typeof(ICancelOrderApi))]
    public class CancelOrderApi : AuthApi, ICancelOrderApi
    {

        /// <remarks>
        ///{"result":"success","data":{"oid":"210d68bb-a9ed-4741-a17c-24affe7b832b","qid":"d1aeb389-aa59-4958-837e-2cdb2bf79357"}}   
        /// </remarks>
        public OrderResult CancelOrder(int orderId)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("oid", OrderIds.Singleton[orderId - 1]);
            string result = AuthQuery("BTCUSD/money/order/cancel", args);
            JObject jobject = JObject.Parse(result);
            OrderResult orderResult = new OrderResult();
            if (jobject["result"] != null && jobject["result"].ToString() == "success")
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
