using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Bitstamp
{
    [Export(typeof(IHistoryApi))]

    public class HistoryApi : BitstampApi, IHistoryApi
    {
        public void UpdateHistory(ref History history)
        {
            string url = "https://www.bitstamp.net/api/transactions/";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadHistoryCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["History"], Exchange.Name, history.Ticker.CurrencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), history);
            }
        }
        /// <remarks>
        /// [
        ///     {"date": "1383388627", "tid": 1722001, "price": "204.84", "amount": "0.50000000"}, 
        ///     {"date": "1383388674", "tid": 1722002, "price": "204.00", "amount": "2.50000000"}, 
        ///     {"date": "1383388683", "tid": 1722003, "price": "204.00", "amount": "15.30403000"},
        ///     ...
        ///     {"date": "1383392177", "tid": 1722184, "price": "203.87", "amount": "0.17718160"}
        /// ]
        /// </remarsk>     
        private void DownloadHistoryCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            History history = e.UserState as History;
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadDataAsyncError(DataTypeDict["History"], Exchange.Name, currencyPair.Code, e.Error.Message);
                return;
            }
            EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["History"], Exchange.Name, currencyPair.Code);
            string json = e.Result;
            JArray jArray = JArray.Parse(json);
            history.Clear();

            foreach (var item in jArray)
            {
                var itemDef = new { date = 0, price = 0m, amount = 0.0, tid = 1 };
                //注意：{  "date": "1384946344",  "tid": 1998167,  "price": "472.00",  "amount": "0E-8"｝
                //既然有： 0E-8 值
                var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);
                Deal deal = new Deal();

                deal.DealTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(itemResult.date);
                deal.DealType = DealType.Unkown;
                deal.PriceQuantity = new PriceQuantityItem();
                decimal convertRate = history.Ticker.ConvertRate;
                deal.PriceQuantity.ConvertRate = convertRate;
                deal.PriceQuantity.Price = itemResult.price;
                deal.PriceQuantity.Quantity = Convert.ToDecimal(itemResult.amount);
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
