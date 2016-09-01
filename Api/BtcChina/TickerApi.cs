using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.BtcChina
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : BtcChinaApi, ITickerApi
    {

        public void UpdateTicker(ref Ticker ticker)
        {
            string url = "https://data.btcchina.com/data/ticker";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadTickerCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Ticker"],Exchange.Name, ticker.CurrencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), ticker);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// json示例：
        /// {
        ///     "ticker":
        ///     {
        ///         "high":"1328.00","low":"1284.66","buy":"1318.40","sell":"1318.50","last":"1318.40","vol":"14583.27900000"
        ///     }
        /// }
        /// </remarks>
        private void DownloadTickerCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Ticker ticker = e.UserState as Ticker;
            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadDataAsyncError(DataTypeDict["Ticker"],Exchange.Name, ticker.CurrencyPair.Code, e.Error.Message);
                return;
            }

            try
            {
                string jsonText = e.Result;
                JObject jObject = JObject.Parse(jsonText); var tickerDef = new { high = 0m, low = 0m, buy = 0m, sell = 0m, last = 0m, vol = 0m };
                var result = JsonConvert.DeserializeAnonymousType(jObject["ticker"].ToString(), tickerDef);
                ticker.LastTradePrice = result.last;
                ticker.HighPrice = result.high;
                ticker.LowPrice = result.low;
                ticker.AskPrice = result.sell;
                ticker.BidPrice = result.buy;
                ticker.Volume = result.vol;
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Ticker"],Exchange.Name, ticker.CurrencyPair.Code);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["Ticker"],Exchange.Name, ticker.CurrencyPair.Code, exception.Message);
                return;
            }
        }
    }
}
