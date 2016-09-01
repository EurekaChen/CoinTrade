using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.OKCoin
{

    [Export(typeof(IHistoryApi))]

    public class HistoryApi : OKCoinApi, IHistoryApi
    {
        public void UpdateHistory(ref History history)
        {
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            //取100条,既然得到的是6.19日的数据？！
            //string url = "https://www.okcoin.com/api/trades.do?symbol=" + codePair + "&since=100";
            string url = "https://www.okcoin.com/api/trades.do?symbol=" + codePair;
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
        ///     {"amount":"17","date":1380171950,"price":"14.11","tid":101,"type":"sell"},
        ///     {"amount":"1.99","date":1380171994,"price":"14.1","tid":103,"type":"buy"},
        ///     {"amount":"1","date":1380172020,"price":"14","tid":105,"type":"buy"},
        ///     ...
        ///     {"amount":"230","date":1380734081,"price":"14.01","tid":217,"type":"sell"},
        ///     {"amount":"184.912","date":1380734242,"price":"14.01","tid":219,"type":"sell"}
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
