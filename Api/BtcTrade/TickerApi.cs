using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;

namespace Eureka.CoinTrade.Api.BtcTrade
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : BtcTradeApi, ITickerApi
    {
        public void UpdateTicker(ref Ticker ticker)
        {
            CurrencyPair currencyPair = ticker.CurrencyPair;
            int id = PairIdDict[currencyPair.Code];
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "http://www.btctrade.com/api/ticker?coin=" + id.ToString();
            //转向：基础连接已经关闭: 连接被意外关闭。
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
        /// {"high":5090,"low":4311,"buy":"4890.00","sell":"4900.00","last":4900,"vol":14807.15}
        /// </remarks>
        private void UpdateTicker(Ticker ticker, string jsonText)
        {
            var btcltcTicker = new { high = 0m, low = 0m, buy = 0m, sell = 0m, last = 0m, vol = 0m };
            var result = JsonConvert.DeserializeAnonymousType(jsonText, btcltcTicker);

            ticker.LastTradePrice = result.last;
            ticker.HighPrice = result.high;
            ticker.LowPrice = result.low;
            ticker.AskPrice = result.sell;
            ticker.BidPrice = result.buy;
            EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code);
        }
    }
}
