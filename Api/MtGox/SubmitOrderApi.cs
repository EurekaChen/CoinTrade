using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.MtGox
{
    [Export(typeof(ISubmitOrderApi))]
    public class SubmitOrderApi : AuthApi, ISubmitOrderApi
    {
        public OrderResult SubmitOrder(CurrencyPair pair, Order order)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();

            if (order.OrderType == OrderType.Buy)
            {
                args.Add("type", "bid");

            }
            if (order.OrderType == OrderType.Sell)
            {
                args.Add("type", "ask");
            }
            //BTC放大10e8，美元放大10E5
            // int priceInt =(int) (order.PriceQuantity.OrginalPrice * 100000000);
            // int amountInt = (int)(order.PriceQuantity.Quantity * 100000);          

            //废弃！
            args.Add("price", order.PriceQuantity.OrginalPrice.ToString());
            args.Add("amount", order.PriceQuantity.Quantity.ToString());

            string result = AuthQuery(pair.Code + "/money/order/add", args);
            return GetResult(result);
        }




        ///<remarks>
        ///{"result":"success","data":"2ecd4473-be85-4a4a-a637-5d67869f6446"}
        ///</remarks> 
        private OrderResult GetResult(string json)
        {
            OrderResult orderResult = new OrderResult();

            JObject jObject = JObject.Parse(json);
            if (jObject["result"].ToString() == "success")
            {
                var def = new { result = "", data = "" };
                var result = JsonConvert.DeserializeAnonymousType(json, def);
                orderResult.IsSuccess = true;
                orderResult.Info = Resources.OrderSuccess + "id:" + result.data;
                return orderResult;
            }
            else
            {
                orderResult.IsSuccess = false; ;
                return orderResult;
            }
        }

        private static decimal? fee;
        public decimal GetBuyFeePercentage(CurrencyPair pair)
        {
            return GetCacheFee(pair);
        }

        private decimal GetCacheFee(CurrencyPair pair)
        {
            if (fee == null)
            {
                fee = GetFee(pair);
            }
            return fee ?? 0m;
        }


        public decimal GetSellFeePercentage(CurrencyPair pair)
        {
            return GetCacheFee(pair);
        }

        /// <seealso cref="FundApi"/>
        ///  <remarks> 返回数据示例
        /// {
        ///     "result":"success",
        ///     "data":
        ///     {
        ///          ...
        ///         "Trade_Fee":0.6
        ///     }
        /// } 
        /// </remarks>
        private decimal GetFee(CurrencyPair pair)
        {
            try
            {
                string result = AuthQuery("money/info");
                JObject jObject = JObject.Parse(result);
                var tradeFee = jObject["data"]["Trade_Fee"].ToString();
                EventSourceLogger.Logger.QueryDataSuccess(DataTypeDict["Fee"], Exchange.Name, pair.Code);
                return Convert.ToDecimal(tradeFee);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.QueryDataException(DataTypeDict["Fee"], Exchange.Name, pair.Code, exception.Message);
                return 0m;
            }

        }

    }
}
