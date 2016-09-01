using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Btcltc
{

    [Export(typeof(IHistoryApi))]

    public class HistoryApi : BtcltcApi, IHistoryApi
    {
        public void UpdateHistory(ref History history)
        {
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "http://www.btcltc.com/index.php?g=home&m=api&a=index&type=trade&t=" + codePair;
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
        /// [
        ///     {"date":"1381051603","price":"0.00000026","amount":"6800000.00000000","tid":"95857","type":"sell"},
        ///     {"date":"1381053258","price":"0.00000026","amount":"1572864.00000000","tid":"95881","type":"sell"},
        ///     ...
        ///     {"date":"1381054350","price":"0.00000026","amount":"1572864.00000000","tid":"95895","type":"sell"}
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
