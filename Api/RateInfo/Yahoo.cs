using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;


namespace Eureka.CoinTrade.Api.Info
{
    [Export("Yahoo", typeof(IExchangeRateApi))]
    public class Yahoo : IExchangeRateApi
    {
        public string ApiName { get { return "Yahoo"; } }

        /// <summary>
        ///  从Yahoo财经数据中获取汇率。
        /// </summary>
        /// <param name="pair"><example>USDCNY</example></param>
        /// <remarks>
        /// USDCNY的获取URL："http://download.finance.yahoo.com/d/quotes.csv?e=.csv&f=sl1d1t1&s=USDCNY=x";
        /// 获得的结果为: "USDCNY=X",6.1265,"10/8/2013","8:40pm" 
        /// </remarks>
        public void Update(ExchangeRate exchangeRate)
        {
            string queryUrl = "http://download.finance.yahoo.com/d/quotes.csv?e=.csv&f=sl1d1t1&s=" + exchangeRate.Pair.Code + "=x";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadStringCompleted;
                EventSourceLogger.Logger.BeginDownloadRate(ApiName, exchangeRate.Pair.Code);
                webClient.DownloadStringAsync(new Uri(queryUrl), exchangeRate);
            }
        }

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

        private void UpdateRate(ExchangeRate exchangeRate, string result)
        {
            string[] data = result.Trim().Split(',');
            decimal rate = Convert.ToDecimal(data[1]);
            exchangeRate.Rate = rate;
            EventSourceLogger.Logger.UpdateRateSuccess(ApiName, exchangeRate.Pair.Code, rate.ToString());
        }
    }
}
