using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Btce
{
    [Export(typeof(ISubmitOrderApi))]
    public class SubmitOrderApi : AuthApi, ISubmitOrderApi
    {
        public OrderResult SubmitOrder(CurrencyPair pair, Order order)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();

            string pairStr = pair.Base.Code.ToLower() + "_" + pair.Quote.Code.ToLower();
            args.Add("pair", pairStr);

            if (order.OrderType == OrderType.Buy)
            {
                args.Add("type", "buy");
            }
            if (order.OrderType == OrderType.Sell)
            {
                args.Add("type", "sell");
            }
            args.Add("rate", order.PriceQuantity.OrginalPrice.ToString());
            args.Add("amount", order.PriceQuantity.Quantity.ToString());

            string resultJson = AuthQuery("Trade", args);
            return GetResult(resultJson);
        }


        /// 成功：
        /// {
        ///     "success":1,
        ///     "return":
        ///     {
        ///         "received":0,
        ///         "remains":10,
        ///         "order_id":66767908,
        ///         "funds":
        ///         {
        ///             "usd":0,"btc":0,"ltc":0,"nmc":0,"rur":0,"eur":0,"nvc":0,"trc":0,"ppc":0,"ftc":0,"xpm":85
        ///         }
        ///     }
        /// }
        /// 失败：
        /// {
        ///     "success":0,
        ///     "error":"It is not enough XPM in the account for sale."
        /// }

        private OrderResult GetResult(string json)
        {
            OrderResult orderResult = new OrderResult();
            JObject jObject = JObject.Parse(json);
            if (jObject["success"].ToString() == "1")
            {
                var returnJson = jObject["return"].ToString();
                var def = new { received = 0m, order_id = 0, remains = 0m };
                var result = JsonConvert.DeserializeAnonymousType(returnJson, def);
                orderResult.OrderId = result.order_id;
                orderResult.IsSuccess = true;

                orderResult.Info = Resources.OrderSuccess + "id:" + result.order_id.ToString();
                return orderResult;
            }
            else
            {
                orderResult.IsSuccess = false;
                orderResult.Info = jObject["error"].ToString();
                return orderResult;
            }
        }

        private static Dictionary<string, decimal> FeeCache = new Dictionary<string, decimal>();

        public decimal GetBuyFeePercentage(CurrencyPair pair)
        {
            return GetCacheFee(pair);
        }

        public decimal GetSellFeePercentage(CurrencyPair pair)
        {
            return GetCacheFee(pair);
        }

        private decimal GetCacheFee(CurrencyPair pair)
        {
            if (!FeeCache.ContainsKey(pair.Code))
            {
                FeeCache.Add(pair.Code, GetFeePercentage(pair));
            }
            return FeeCache[pair.Code];
        }

        private decimal GetFeePercentage(CurrencyPair pair)
        {
            string codePair = pair.Base.Code.ToLower() + "_" + pair.Quote.Code.ToLower();
            string url = "https://btc-e.com/api/2/" + codePair + "/fee";
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string json = webClient.DownloadString(url);
                    var feeDef = new { trade = 0m };
                    var result = JsonConvert.DeserializeAnonymousType(json, feeDef);
                    EventSourceLogger.Logger.QueryDataSuccess("费率", Exchange.Name, pair.Code);
                    return result.trade;
                }
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.QueryDataException("费率", Exchange.Name, pair.Code, exception.Message);
                return 0m;
            }
        }
    }
}
