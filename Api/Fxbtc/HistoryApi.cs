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

    [Export(typeof(IHistoryApi))]
    public class HistoryApi : FxbtcApi, IHistoryApi
    {
        public void UpdateHistory(ref History history)
        {
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://data.fxbtc.com/api?op=query_last_trades&symbol=" + codePair + "&count=100";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadHistoryCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["History"], Exchange.Name, currencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), history);
            }
        }

        private void DownloadHistoryCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            History history = e.UserState as History;
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadDataAsyncError(DataTypeDict["History"], Exchange.Name, currencyPair.Code, e.Error.Message);
                return;
            }
            try
            {
                UpdateHistory(ref history, e.Result);
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["History"], Exchange.Name, currencyPair.Code);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["History"], Exchange.Name, currencyPair.Code, exception.Message);
            }
        }

        /// <remarks>
        /// {
        ///     "result":true,
        ///     "datas":
        ///     [
        ///         {"date":1386503347,"rate":"4930","vol":"0.1256","order":"2","type":"ask","ticket":"566477"},
        ///         {"date":1386502596,"rate":"5000","vol":"0.02","order":"1","type":"bid","ticket":"566461"},
        ///         {"date":1386502272,"rate":"4960","vol":"0.0009","order":"1","type":"bid","ticket":"566444"},
        ///         ...
        ///         {"date":1386494371,"rate":"5099.9999","vol":"0.0175","order":"1","type":"bid","ticket":"565807"},
        ///         {"date":1386494371,"rate":"5099","vol":"1.1773","order":"1","type":"bid","ticket":"565806"}
        ///     ],
        ///     "params":
        ///     {
        ///         "symbol":"btc_cny","count":"100"
        ///     }
        /// }
        /// </remarks>
        private void UpdateHistory(ref History history, string json)
        {
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            JObject jObject = JObject.Parse(json);
            JArray jArray = JArray.Parse(jObject["datas"].ToString());
            history.Clear();

            foreach (var item in jArray)
            {
                var itemDef = new { date = 0, rate = 0m, vol = 0m, order = 0, ticket = 1, type = "" };
                var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);

                Deal deal = new Deal();

                deal.DealTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(itemResult.date);
                if (itemResult.type == "ask")
                {
                    deal.DealType = DealType.Sell;
                }
                if (itemResult.type == "bid")
                {
                    deal.DealType = DealType.Buy;
                }
                deal.PriceQuantity = new PriceQuantityItem();
                decimal convertRate = SelectedTickers[currencyPair.Code].ConvertRate;

                deal.PriceQuantity.Price = itemResult.rate;
                deal.PriceQuantity.Quantity = itemResult.vol;

                history.Add(deal);
            }

            //var reverse = history.Reverse();
            //var reverseHistory = new History(history.Ticker);
            //foreach (Deal deal in reverse)
            //{
            //    reverseHistory.Add(deal);
            //}
            //history = reverseHistory;
        }
    }
}
