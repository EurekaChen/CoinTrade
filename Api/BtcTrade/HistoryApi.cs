using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.BtcTrade
{

    [Export(typeof(IHistoryApi))]

    public class HistoryApi : BtcTradeApi, IHistoryApi
    {
        public void UpdateHistory(ref History history)
        {
            string pairCode = history.Ticker.CurrencyPair.Code;
            int id = PairIdDict[pairCode];
            //string url = "http://www.btcclubs.com/api/trades?coin=" + id.ToString();
            string url = "http://www.btctrade.com/api/trades?coin=" + id.ToString();
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadHistoryCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["History"], Exchange.Name, pairCode);
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
        /// [
        ///     {"date":"1386493773","price":4930,"amount":"0.03000000","tid":"1280540","type":"buy"},
        ///     {"date":"1386493773","price":4930,"amount":"0.02700000","tid":"1280541","type":"buy"},
        ///     {"date":"1386493773","price":4930,"amount":"0.14500000","tid":"1280542","type":"buy"},
        ///     ...
        ///     {"date":"1386494633","price":4900,"amount":"0.65700000","tid":"1281076","type":"buy"},
        ///     {"date":"1386494639","price":4895,"amount":"25.10000000","tid":"1281080","type":"sell"}
        /// ]
        /// </remarks>

        private void UpdateHistory(ref History history, string json)
        {
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            JArray jArray = JArray.Parse(json);
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
                decimal convertRate = SelectedTickers[currencyPair.Code].ConvertRate;

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
