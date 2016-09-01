using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Fxbtc
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : FxbtcApi, ITickerApi
    {
        public void UpdateTicker(ref Ticker ticker)
        {
            CurrencyPair currencyPair = ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://data.fxbtc.com/api?op=query_ticker&symbol=" + codePair;
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadTickerCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Ticker"], Exchange.Name, currencyPair.Code);
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
        ///     "result":true,
        ///     "ticker":
        ///     {
        ///         "high":5300,"low":4550,"vol":21447.828078887,"last_rate":4918.8888,"ask":4960,"bid":4889.01
        ///     },
        ///     "params":
        ///     {
        ///         "symbol":"btc_cny"
        ///     }
        /// }
        /// </remarks>
        private void UpdateTicker(Ticker ticker, string jsonText)
        {
            JObject jObject = JObject.Parse(jsonText);
            string tikerJson = jObject["ticker"].ToString();

            var btcltcTicker = new { last_rate = 0m, high = 0m, low = 0m, vol = 0.0, bid = 0m, ask = 0m };
            var result = JsonConvert.DeserializeAnonymousType(tikerJson, btcltcTicker);

            ticker.LastTradePrice = result.last_rate;
            ticker.HighPrice = result.high;
            ticker.LowPrice = result.low;
            ticker.AskPrice = result.ask;
            ticker.BidPrice = result.bid;
            EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code);
        }
    }
}
