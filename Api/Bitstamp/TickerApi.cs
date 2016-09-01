using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Bitstamp
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : BitstampApi, ITickerApi
    {

        public void UpdateTicker(ref Ticker ticker)
        {
            string url = "https://www.bitstamp.net/api/ticker/";
            using (WebClient webClient = new WebClient())
            {
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Ticker"],Exchange.Name, ticker.CurrencyPair.Code);
                webClient.DownloadStringCompleted += DownloadTickerCompleted;
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
        /// {"high": "205.00", "last": "204.41", "timestamp": "1383391604", "bid": "203.28", "volume": "6530.17139732", "low": "201.14", "ask": "204.41"}
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
                JObject jObject = JObject.Parse(jsonText);
                var itemDef = new { high = 0m, last = 0m, timestamp = 0, bid = 0m, volume = 0m, low = 0m, ask = 0m };
                var result = JsonConvert.DeserializeAnonymousType(jsonText, itemDef);

                ticker.LastTradePrice = result.last;
                ticker.HighPrice = result.high;
                ticker.LowPrice = result.low;
                ticker.AskPrice = result.ask;
                ticker.BidPrice = result.bid;
                ticker.Volume = result.volume;
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
