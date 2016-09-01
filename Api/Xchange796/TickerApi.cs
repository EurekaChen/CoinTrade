using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Xchange796
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : Xchange796cApi, ITickerApi
    {
        public void UpdateTicker(ref Ticker ticker)
        {
            string url = "http://api.796.com/apiV2/ticker.html?op=futures";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadTickerCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Ticker"], Exchange.Name,ticker.CurrencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), ticker);
            }
        }

        /// <summary>
        /// 获得报价数据完成。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>json示例：
        /// {
        ///     "result":"success",
        ///     "return":
        ///     {
        ///         "last":"748.00","high":"840.00","low":"610.00","vol":"13058.86","buy":"747.00","sell":"750.00"
        ///     }
        /// }
        /// </remarks>
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
                JObject jObject = JObject.Parse(jsonText);
                string tikerJson = jObject["return"].ToString();

                var btcltcTicker = new { last = 0m, high = 0m, low = 0m, vol = 0.0, buy = 0m, sell = 0m };
                var result = JsonConvert.DeserializeAnonymousType(tikerJson, btcltcTicker);

                ticker.LastTradePrice = result.last;
                ticker.HighPrice = result.high;
                ticker.LowPrice = result.low;
                ticker.AskPrice = result.sell;
                ticker.BidPrice = result.buy;
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code, exception.Message);
                return;
            }
        }
    }
}
