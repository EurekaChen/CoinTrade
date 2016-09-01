using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.OKCoin
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : OKCoinApi, ITickerApi
    {
        public void UpdateTicker(ref Ticker ticker)
        {
            CurrencyPair currencyPair = ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://www.okcoin.com/api/ticker.do?symbol=" + codePair;
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadTickerCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Ticker"], Exchange.Name, currencyPair.Code);
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
        ///     "ticker":
        ///     {
        ///         "buy":"175.4","high":"222.0","last":"173.59","low":"128.0","sell":"172.5","vol":"8048805.41099948"
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
                string tikerJson = jObject["ticker"].ToString();

                var btcltcTicker = new { buy = 0m, high = 0m, last = 0m, low = 0m, sell = 0m, vol = 0m };
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
