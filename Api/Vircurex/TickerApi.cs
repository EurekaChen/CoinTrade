using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Vircurex
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    // https://vircurex.com/api/get_info_for_1_currency.json?base=BTC&alt=USD
    public class TickerApi : VircurexApi, ITickerApi
    {
        public void UpdateTicker(ref Ticker ticker)
        {
            CurrencyPair currencyPair = ticker.CurrencyPair;

            string url = "https://vircurex.com/api/get_info_for_1_currency.json?base=" + currencyPair.Base.Code + "&alt=" + currencyPair.Quote.Code;
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
        /// {"base":"BTC","alt":"USD","lowest_ask":"199.888","highest_bid":"191.4916433","last_trade":"199.88806268","volume":"0.36157682"}
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
                UpdateTicker(e.Result, ref ticker);
            }
            catch (Exception ex)
            {
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Ticker"],Exchange.Name, ticker.CurrencyPair.Code);
                return;
            }
        }

        private static void UpdateTicker(string json, ref Ticker ticker)
        {
            JObject jObject;
            jObject = JObject.Parse(json); var tickerDef = new { lowest_ask = 0m, highest_bid = 0m, last_trade = 0m, volume = 0m };
            var result = JsonConvert.DeserializeAnonymousType(json, tickerDef);

            ticker.LastTradePrice = result.last_trade;
            ticker.AskPrice = result.lowest_ask;
            ticker.BidPrice = result.highest_bid;
            ticker.Volume = result.volume;
        }
    }
}
