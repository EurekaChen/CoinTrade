using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Btcltc
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : BtcltcApi, ITickerApi
    {
        public void UpdateTicker(ref Ticker ticker)
        {
            CurrencyPair currencyPair = ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            // string url = "https://btcltc.com/home/api/index/type/" + codePair;
            string url = "http://www.btcltc.com/index.php?g=home&m=api&a=index&type=" + codePair;

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
        ///     "ticker":
        ///     {   
        ///         "new":"0.00000034","high":"0.00000036","low":"0.00000024","vol":"262149197.52501380","buy":"0.00000050","sell":"0.00000016"
        ///     }
        /// }
        /// </remarks>
        private void UpdateTicker(Ticker ticker, string jsonText)
        {
            JObject jObject = JObject.Parse(jsonText);
            string tikerJson = jObject["ticker"].ToString();

            var btcltcTicker = new { New = 0m, high = 0m, low = 0m, vol = 0.0, buy = 0m, sell = 0m };
            var result = JsonConvert.DeserializeAnonymousType(tikerJson, btcltcTicker);

            ticker.LastTradePrice = result.New;
            ticker.HighPrice = result.high;
            ticker.LowPrice = result.low;
            ticker.AskPrice = result.sell;
            ticker.BidPrice = result.buy;
            EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code);
        }
    }
}
