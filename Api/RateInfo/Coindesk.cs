using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json.Linq;


namespace Eureka.CoinTrade.Api.RateInfo
{
    [Export("Coindesk", typeof(IExchangeRateApi))]
    public class Coindesk : IExchangeRateApi
    {
        public string ApiName { get { return "Coindesk"; } }
        /// <summary>
        /// 更新汇率
        /// </summary>
        /// <param name="exchangeRate">需要更新的汇率</param>
        /// 
        public void Update(ExchangeRate exchangeRate)
        {
            CurrencyPair currencyPair = exchangeRate.Pair;
            string quoteCode = currencyPair.Quote.Code;
            string queryUrl = "http://api.coindesk.com/v1/bpi/currentprice/" + quoteCode + ".json";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadStringCompleted;
                EventSourceLogger.Logger.BeginDownloadRate(ApiName, currencyPair.Code);
                webClient.DownloadStringAsync(new Uri(queryUrl), exchangeRate);
            }
        }

        /// <summary>
        /// 解析Coindesk BPI 的Json数据 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// 获取地址： http://api.coindesk.com/v1/bpi/currentprice/CNY.json
        /// 数据格式： 
        /// {
        ///     "time":
        ///     {
        ///         "updated":"Sep 18, 2013 17:27:00 UTC","updatedISO":"2013-09-18T17:27:00+00:00"
        ///     },
        ///     "disclaimer":"This data ...",
        ///     "bpi":
        ///     {
        ///         "USD":
        ///         {
        ///             "code":"USD","rate":"126.5235","description":"United States Dollar","rate_float":126.5235
        ///         },
        ///         "CNY":
        ///         {
        ///             "code":"CNY","rate":"775.0665","description":"Chinese Yuan","rate_float":"775.0665"             
        ///         }
        ///     }
        /// }
        /// </remarks>
        private void DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            ExchangeRate exchangeRate = e.UserState as ExchangeRate;
            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadRateAsnycError(ApiName, exchangeRate.Pair.Code, e.Error.Message);
                return;
            }
            try
            {
                UpdateRate(exchangeRate, e.Result);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateRateException(ApiName, exchangeRate.Pair.Code, exception.Message);
            }

        }

        private void UpdateRate(ExchangeRate exchangeRate, string json)
        {
            JObject result = JObject.Parse(json);
            string quoteCode = exchangeRate.Pair.Quote.Code;
            decimal rate = Convert.ToDecimal(result["bpi"][quoteCode]["rate"].ToString());
            exchangeRate.Rate = rate;
            EventSourceLogger.Logger.UpdateRateSuccess(ApiName, exchangeRate.Pair.Code, rate.ToString());
        }
    }
}
