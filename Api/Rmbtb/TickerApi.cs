using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Rmbtb
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : RmbtbApi, ITickerApi
    {
        public void UpdateTicker(ref Ticker ticker)
        {
            string url = "https://www.rmbtb.com/api/thirdparty/ticker/";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadTickerCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), ticker);
            }
        }


        private void DownloadTickerCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Ticker ticker = e.UserState as Ticker;
            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadDataAsyncError(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code, e.Error.Message);
                return;
            }

            try
            {
                string jsonText = e.Result;
                UpdateTicker(ticker, jsonText);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code, exception.Message);
                return;
            }
        }

        /// <summary>
        /// 获得报价数据完成。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>json示例：
        /// {
        ///     "ticker":
        ///     {
        ///         "high":"5180.0210","low":"4544.4950","buy":"4753.9660","sell":"4857.1590","last":"4783.3120","vol":"568.2345"
        ///     }
        /// }
        /// </remarks>
        private void UpdateTicker(Ticker ticker, string jsonText)
        {
            JObject jObject = JObject.Parse(jsonText);
            string tikerJson = jObject["ticker"].ToString();

            var btcltcTicker = new { last = 0m, high = 0m, low = 0m, vol = 0.0, buy = 0m, sell = 0m };
            var result = JsonConvert.DeserializeAnonymousType(tikerJson, btcltcTicker);

            ticker.LastTradePrice = result.last;
            ticker.HighPrice = result.high;
            ticker.LowPrice = result.low;
            ticker.AskPrice = result.sell;
            ticker.BidPrice = result.buy;
            EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code);
        }
    }
}
