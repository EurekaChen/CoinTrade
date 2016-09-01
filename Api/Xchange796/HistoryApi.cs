using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Xchange796
{

    [Export(typeof(IHistoryApi))]

    public class HistoryApi : Xchange796cApi, IHistoryApi
    {
        public void UpdateHistory(ref History history)
        {
            string url = "http://api.796.com/apiV2/trade/50.html?op=futures";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadHistoryCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["History"], Exchange.Name, history.Ticker.CurrencyPair.Code);
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
        ///{
        ///     "result":"success",
        ///     "return":
        ///     [
        ///         {"time":1386489021,"price":"784.00","amount":"2","type":"sell"},
        ///         {"time":1386488972,"price":"785.00","amount":"1.78","type":"sell"},
        ///         {"time":1386488943,"price":"785.00","amount":"6.22","type":"sell"},
        ///         ...
        ///         {"time":1386487879,"price":"776.11","amount":"2","type":"sell"},
        ///         {"time":1386487873,"price":"776.11","amount":"18","type":"sell"},
        ///         {"time":1386487872,"price":"780.00","amount":"0.2","type":"buy"}
        ///     ]
        ///  }
        /// </remarks>
        private void UpdateHistory(ref History history, string json)
        {
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            JObject jObject = JObject.Parse(json);
            JArray jArray = JArray.Parse(jObject["return"].ToString());
            history.Clear();

            foreach (var item in jArray)
            {
                var itemDef = new { time = 0, price = 0m, amount = 0m, type = "" };
                var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);

                Deal deal = new Deal();

                deal.DealTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(itemResult.time);
                if (itemResult.type == "sell")
                {
                    deal.DealType = DealType.Sell;
                }
                if (itemResult.type == "buy")
                {
                    deal.DealType = DealType.Buy;
                }
                deal.PriceQuantity = new PriceQuantityItem();
                decimal convertRate = SelectedTickers[currencyPair.Code].ConvertRate;

                deal.PriceQuantity.Price = itemResult.price;
                deal.PriceQuantity.Quantity = itemResult.amount;

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
