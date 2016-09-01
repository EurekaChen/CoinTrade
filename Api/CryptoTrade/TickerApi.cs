using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.CryptoTrade
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : CryptoTradeApi, ITickerApi
    {
        public void UpdateTicker(ref Ticker ticker)
        {
            CurrencyPair currencyPair = ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://crypto-trade.com/api/1/ticker/" + codePair;
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
        ///     "status":"success",
        ///     "data":
        ///     {
        ///         "last":"872.99999","low":"701.20000001","high":"874","vol_btc":"24.89176105","vol_usd":"18054.17636478","min_ask":"873","max_bid":"801.00001"
        ///     }
        /// }
        /// </remarks>
        private void UpdateTicker(Ticker ticker, string jsonText)
        {
            JObject jObject = JObject.Parse(jsonText);
            string tikerJson = jObject["data"].ToString();

            var btcltcTicker = new { last = 0m, high = 0m, low = 0m, vol = 0.0, max_bid = 0m, min_ask = 0m };
            var result = JsonConvert.DeserializeAnonymousType(tikerJson, btcltcTicker);

            ticker.LastTradePrice = result.last;
            ticker.HighPrice = result.high;
            ticker.LowPrice = result.low;
            ticker.AskPrice = result.min_ask;
            ticker.BidPrice = result.max_bid;

            JObject volObject = JObject.Parse(jsonText);
            string volName = "vol_" + ticker.CurrencyPair.Base.Code.ToLower();
            string volText = volObject["data"][volName].ToString();
            ticker.Volume = Convert.ToDecimal(volText);
            EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code);
        }
    }
}
