using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Bter
{

    [Export(typeof(IHistoryApi))]

    public class HistoryApi : BterApi, IHistoryApi
    {

        //https://bter.com/api/1/trade/btc_cny
        public void UpdateHistory(ref History history)
        {
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://bter.com/api/1/trade/" + codePair;
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadHistoryCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["History"],Exchange.Name, currencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), history);
            }
        }
        /// <remarks>
        /// {
        ///     "result":"true",
        ///     "data":
        ///     [
        ///         {"date":"1381638220","price":806,"amount":6.2039,"tid":"254250","type":"buy"},
        ///         {"date":"1381638268","price":806.98,"amount":0.5,"tid":"254255","type":"buy"},
        ///         {"date":"1381638268","price":806.5,"amount":3,"tid":"254254","type":"buy"},
        ///         ...
        ///         {"date":"1381638268","price":806,"amount":2.2107,"tid":"254251","type":"buy"},
        ///         {"date":"1381638268","price":806.3,"amount":0.0089,"tid":"254253","type":"buy"},
        ///         {"date":"1383466241","price":1264,"amount":3.05,"tid":"270756","type":"buy"}
        ///     ],
        ///     "elapsed":"12.399ms"
        /// }
        /// </remarks>
        private void DownloadHistoryCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            History history = e.UserState as History;
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadDataAsyncError(DataTypeDict["History"],Exchange.Name, currencyPair.Code, e.Error.Message);
                return;
            }
            EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["History"],Exchange.Name, currencyPair.Code);
            string json = e.Result;

            JObject jObject = JObject.Parse(json);
            JArray jArray = JArray.Parse(jObject["data"].ToString());

            history.Clear();
            foreach (var item in jArray)
            {
                var itemDef = new { date = 0, price = 0m, amount = 0m, tid = 1, type = "" };
                var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);
                Deal deal = new Deal();

                deal.DealTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(itemResult.date);
                if (itemResult.type == "sell")
                {
                    deal.DealType = DealType.Sell;
                }
                if (itemResult.type == "buy")
                {
                    deal.DealType = DealType.Buy;
                }
                deal.PriceQuantity = new PriceQuantityItem();
                decimal convertRate = history.Ticker.ConvertRate;
                deal.PriceQuantity.ConvertRate = convertRate;
                deal.PriceQuantity.Price = itemResult.price;
                deal.PriceQuantity.Quantity = itemResult.amount;
                history.Add(deal);
            }
            var reverse = history.Reverse();
            var reverseHistory = new History(history.Ticker);
            foreach (Deal deal in reverse)
            {
                reverseHistory.Add(deal);
            }
            history = reverseHistory;
        }
    }
}
