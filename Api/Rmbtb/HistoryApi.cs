using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Rmbtb
{

    [Export(typeof(IHistoryApi))]

    public class HistoryApi : RmbtbApi, IHistoryApi
    {
        public void UpdateHistory(ref History history)
        {
            string url = "http://www.rmbtb.com/api/thirdparty/lasttrades/";
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
        /// [
        ///     {"date":"1386491671","price":"4881.82","tid":"206805","amount":"2.8460","type":"buy"},
        ///     {"date":"1386491974","price":"4894.22","tid":"206818","amount":"0.7180","type":"buy"},
        ///     {"date":"1386492147","price":"4868.24","tid":"206827","amount":"0.4860","type":"sell"},
        ///     ...
        ///     {"date":"1386507178","price":"4835.88","tid":"207798","amount":"0.3200","type":"buy"},
        ///     {"date":"1386507377","price":"4820.02","tid":"207808","amount":"1.0870","type":"sell"}
        /// ]
        /// 
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
