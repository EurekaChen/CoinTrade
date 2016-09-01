using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Btce
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : BtceApi, ITickerApi
    {

        public void UpdateTicker(ref Ticker ticker)
        {
            CurrencyPair currencyPair = ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://btc-e.com/api/2/" + codePair + "/ticker";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadTickerCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Ticker"],Exchange.Name, currencyPair.Code);
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
        ///         "high":0.00368,"low":0.0034,"avg":0.00354,"vol":69.43449,"vol_cur":19591.53887,"last":0.00355,"buy":0.00357,"sell":0.00355,"updated":1383218988,"server_time":1383218990
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
                JObject jObject = JObject.Parse(jsonText);
                var tickerDef = new { high = 0m, low = 0m, avg = 0m, vol = 0m, vol_cur = 0m, last = 0m, buy = 0m, sell = 0m, update = 0, server_time = 0 };
                var result = JsonConvert.DeserializeAnonymousType(jObject["ticker"].ToString(), tickerDef);

                ticker.LastTradePrice = result.last;
                ticker.HighPrice = result.high;
                ticker.LowPrice = result.low;
                ticker.AskPrice = result.sell;
                ticker.BidPrice = result.buy;
                ticker.Volume = result.vol;
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Ticker"],Exchange.Name, ticker.CurrencyPair.Code);
            }
            catch (Exception ex)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["Ticker"],Exchange.Name, ticker.CurrencyPair.Code, ex.Message);
                return;
            }
        }
    }
}
