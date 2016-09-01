using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.BtcChina
{
    [Export(typeof(IHistoryApi))]

    public class HistoryApi : BtcChinaApi, IHistoryApi
    {

        public void UpdateHistory(ref History history)
        {
            //string url = "https://data.btcchina.com/data/trades"; 返回近10万数据。
            string url = "https://data.btcchina.com/data/historydata"; //返回最近历史,可以跟：?since=5000 .
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadHistoryCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["History"], Exchange.Name, history.Ticker.CurrencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), history);
            }
        }
        /// <remarks>
        /// trades:
        /// [
        ///     {"date":"1383442174","price":1277.3,"amount":0.5,"tid":"686526"},
        ///     {"date":"1383442203","price":1277.22,"amount":0.1,"tid":"686527"},
        ///     {"date":"1383442246","price":1277.22,"amount":1.3,"tid":"686528"},
        ///     ...
        ///     {"date":"1383528476","price":1318.5,"amount":0.962,"tid":"692477"},
        ///     {"date":"1383528478","price":1318.5,"amount":4.5,"tid":"692478"}
        /// ]
        /// </remarsk>  
        /// historydata:
        /// [
        ///     {"date":"1385119134","price":4981,"amount":2.202,"tid":"1416266","type":"buy"},
        ///     {"date":"1385119134","price":4981,"amount":0.1,"tid":"1416267","type":"buy"},
        ///     {"date":"1385119134","price":4981.5,"amount":0.059,"tid":"1416268","type":"sell"},
        ///     ...
        ///     {"date":"1385119377","price":4982.99,"amount":0.2,"tid":"1416364","type":"buy"},
        ///     {"date":"1385119378","price":4982.99,"amount":1,"tid":"1416365","type":"buy"},
        ///     {"date":"1385119399","price":4982.99,"amount":0.02,"tid":"1416366","type":"buy"}
        /// ]
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
