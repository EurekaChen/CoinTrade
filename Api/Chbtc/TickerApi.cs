using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Chbtc
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : ChbtcApi, ITickerApi
    {

        public void UpdateTicker(ref Ticker ticker)
        {
            string url = "http://api.chbtc.com/data/ticker";
            if (ticker.CurrencyPair.Base.Code == "LTC")
            {
                url = " http://api.chbtc.com/data/ltc/ticker";
                //返回最近历史,可以跟：?since=5000 .
            }
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadTickerCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code);
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
        ///         "high":"3945.0","low":"3672.01","buy":"3830.00","sell":"3849.99","last":"3849.0","vol":"31718.026"
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
                JObject jObject = JObject.Parse(jsonText); var tickerDef = new { high = 0m, low = 0m, buy = 0m, sell = 0m, last = 0m, vol = 0m };
                var result = JsonConvert.DeserializeAnonymousType(jObject["ticker"].ToString(), tickerDef);
                ticker.LastTradePrice = result.last;
                ticker.HighPrice = result.high;
                ticker.LowPrice = result.low;
                ticker.AskPrice = result.sell;
                ticker.BidPrice = result.buy;
                ticker.Volume = result.vol;
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
