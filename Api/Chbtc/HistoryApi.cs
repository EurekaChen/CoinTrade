using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Chbtc
{
    [Export(typeof(IHistoryApi))]

    public class HistoryApi : ChbtcApi, IHistoryApi
    {

        public void UpdateHistory(ref History history)
        {
            string url;
            if (history.Ticker.CurrencyPair.Base.Code == "LTC")
            {
                url = " http://api.chbtc.com/data/ltc/trades";
                //返回最近历史,可以跟：?since=5000 .
            }
            else
            {
                //BTC:
                url = "http://api.chbtc.com/data/trades";
            }
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadHistoryCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["History"], Exchange.Name, history.Ticker.CurrencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), history);
            }
        }
        /// <remarks>
        /// [
        ///     {"date":"1387876816","price":"3867.5","amount":"0.0010","tid":"348959","type":"sell"},
        ///     {"date":"1387876816","price":"3861.0","amount":"0.6620","tid":"348960","type":"sell"},
        ///     {"date":"1387876816","price":"3861.0","amount":"1.3370","tid":"348961","type":"sell"},
        ///     ...
        ///     {"date":"1387879025","price":"3836.02","amount":"0.0300","tid":"349037","type":"sell"},
        ///     {"date":"1387879025","price":"3836.0","amount":"0.1690","tid":"349038","type":"sell"}
        ///]
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

            try
            {
                UpdateHistory(history, e.Result);
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["History"], Exchange.Name, currencyPair.Code);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["History"], Exchange.Name, currencyPair.Code, exception.Message);
            }
        }

        private static void UpdateHistory(History history, string json)
        {
            JArray jArray = JArray.Parse(json);

            history.Clear();
            foreach (var item in jArray)
            {
                var itemDef = new { date = 0, price = 0m, amount = 0m, tid = 1, type = "" };
                var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);
                Deal deal = new Deal();

                deal.DealTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(itemResult.date);
                if (itemResult.type == "buy")
                {
                    deal.DealType = DealType.Buy;
                }
                if (itemResult.type == "sell")
                {
                    deal.DealType = DealType.Sell;
                }
                deal.PriceQuantity = new PriceQuantityItem();
                decimal convertRate = history.Ticker.ConvertRate;
                deal.PriceQuantity.ConvertRate = convertRate;
                deal.PriceQuantity.Price = itemResult.price;
                deal.PriceQuantity.Quantity = itemResult.amount;
                history.Add(deal);
            }
        }
    }
}
