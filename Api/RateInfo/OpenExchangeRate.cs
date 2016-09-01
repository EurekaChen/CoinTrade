using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json.Linq;


namespace Eureka.CoinTrade.Api.RateInfo
{
    [Export("OpenExchangeRate", typeof(IExchangeRateApi))]
    public class OpenExchangeRate : IExchangeRateApi
    {
        public string ApiName { get { return "OpenExchangeRate"; } }

        public void Update(ExchangeRate exchangeRate)
        {
            string queryUrl = "http://openexchangerates.org/api/latest.json?app_id=90249ea0533248678edf3ba4e1d78f4b";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadStringCompleted;
                EventSourceLogger.Logger.BeginDownloadRate(ApiName, exchangeRate.Pair.Code);
                webClient.DownloadStringAsync(new Uri(queryUrl), exchangeRate);
            }
        }

        /// <summary>
        ///  从http://OpenExchangeRate.org获取汇率。
        /// </summary>
        /// <remarks>
        /// 获取URL：" string url = "http://openexchangerates.org/api/latest.json?app_id=90249ea0533248678edf3ba4e1d78f4b";
        /// 获得的结果为:
        /// {
        ///     "disclaimer": "Exchange rates...",
        ///     "license": "Data sourced ...",
        ///     "timestamp": 1381482060, 
        ///     "base": "USD",  
        ///     "rates":
        ///     {
        ///         "AED": 3.673263,    "AFN": 56.494675,    "ALL": 104.4568,    "AMD": 410.992,    "ANG": 1.785883,    "AOA": 97.590399, ...
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
            catch (Exception ex)
            {
                EventSourceLogger.Logger.DownloadRateAsnycError(ApiName, exchangeRate.Pair.Code, ex.Message);
            }
        }

        private void UpdateRate(ExchangeRate exchangeRate, string result)
        {
            JObject jObject = JObject.Parse(result);
            string quoteCode = exchangeRate.Pair.Quote.Code;
            string priceText = jObject["rates"][quoteCode].ToString();
            decimal rate = Convert.ToDecimal(priceText);
            exchangeRate.Rate = rate;
            EventSourceLogger.Logger.UpdateRateSuccess(ApiName, exchangeRate.Pair.Code, rate.ToString());
        }
    }
}
