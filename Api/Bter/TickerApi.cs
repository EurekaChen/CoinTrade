using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Bter
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : BterApi, ITickerApi
    {
        public void UpdateTicker(ref Ticker ticker)
        {
            CurrencyPair currencyPair = ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://bter.com/api/1/ticker/" + codePair;
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
        ///     "result":"true","last":13.19,"high":13.45,"low":12.68,"avg":13.08,"sell":13.35,"buy":13.18,"vol_ltc":939.5685,"vol_cny":12291.28
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
                var bterTicker = new { result = false, last = 0m, high = 0m, low = 0m, avg = 0m, sell = 0m, buy = 0m };
                var result = JsonConvert.DeserializeAnonymousType(jsonText, bterTicker);

                ticker.LastTradePrice = result.last;
                ticker.HighPrice = result.high;
                ticker.LowPrice = result.low;
                ticker.AskPrice = result.sell;
                ticker.BidPrice = result.buy;

                JObject volObject = JObject.Parse(jsonText);
                string volName = "vol_" + ticker.CurrencyPair.Base.Code.ToLower();
                string volText = volObject[volName].ToString();
                ticker.Volume = Convert.ToDecimal(volText);
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
