using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Btce
{
    [Export(typeof(IHistoryApi))]

    public class HistoryApi : BtceApi, IHistoryApi
    {

        public void UpdateHistory(ref History history)
        {
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://btc-e.com/api/2/" + codePair + "/trades";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadHistoryCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["History"],Exchange.Name, currencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), history);
            }
        }
        /// <remarks>
        /// [
        ///     {"date":1383221301,"price":196.458,"amount":0.0218811,"tid":10376726,"price_currency":"USD","item":"BTC","trade_type":"ask"},
        ///     {"date":1383221293,"price":196.458,"amount":0.0268061,"tid":10376725,"price_currency":"USD","item":"BTC","trade_type":"ask"},
        ///     ...
        ///     {"date":1383221290,"price":196.458,"amount":0.8,"tid":10376724,"price_currency":"USD","item":"BTC","trade_type":"bid"}
        ///  ]
        /// </remarsk>     
        private void DownloadHistoryCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            History history = e.UserState as History;
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadDataAsyncError(DataTypeDict["History"],Exchange.Name, currencyPair.Code, e.Error.Message);
                return;
            }
           
            try
            {
                UpdateHistory(history, e.Result);
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["History"],Exchange.Name, currencyPair.Code);
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
                var itemDef = new { date = 0, price = 0m, amount = 0m, tid = 1, trade_type = "" };
                var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);
                Deal deal = new Deal();

                deal.DealTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(itemResult.date);
                if (itemResult.trade_type == "ask")
                {
                    deal.DealType = DealType.Sell;
                }
                if (itemResult.trade_type == "bid")
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
        }
    }


}
