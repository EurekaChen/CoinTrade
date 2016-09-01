using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.MtGox
{

    [Export(typeof(IHistoryApi))]

    public class HistoryApi : MtGoxApi, IHistoryApi
    {

        public void UpdateHistory(ref History history)
        {
            string pairCode = history.Ticker.CurrencyPair.Code;

            //这样一次获取了12463数据，大于1万条！，太多并且太慢！所以改掉！
            //string url = "http://data.mtgox.com/api/2/" + pairCode + "/money/trades/fetch";
            ulong unixNowTimeStamp = GetStamp(DateTime.Now);
            //只获得一个小时的数据。
            DateTime sinceDateTime = DateTime.Now - new TimeSpan(1, 0, 0);
            ulong sinceStamp = GetStamp(sinceDateTime);
            ulong since = GetStamp(sinceDateTime) * 1000000ul;
            //例：BTCUSD/money/trades/fetch?since=1364767190000000
            string url = "http://data.mtgox.com/api/2/" + pairCode + "/money/trades/fetch?since=" + since.ToString();
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadHistoryCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["History"],Exchange.Name, pairCode);
                webClient.DownloadStringAsync(new Uri(url), history);
            }
        }


        /// <summary>
        /// 将时间转换成UNIX时间戳
        /// </summary>
        /// <param name="dt">时间</param>
        /// <returns>UNIX时间戳</returns>
        public static ulong GetStamp(DateTime dt)
        {
            TimeSpan ts = dt - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            ulong uiStamp = Convert.ToUInt64(ts.TotalSeconds);
            return uiStamp;
        }



        /// <remarks>
        /// {
        ///     "result":"success",
        ///     "data":
        ///     [
        ///         {"date":1383139254,"price":"215.9","amount":"0.06044328","price_int":"21590000","amount_int":"6044328","tid":"1383139254387265","price_currency":"USD","item":"BTC","trade_type":"bid","primary":"Y","properties":"limit"},
        ///         {"date":1383139254,"price":"215.98999","amount":"0.01","price_int":"21598999","amount_int":"1000000","tid":"1383139254673847","price_currency":"USD","item":"BTC","trade_type":"bid","primary":"Y","properties":"limit"},
        ///         ...
        ///         {"date":1383225209,"price":"208.94719","amount":"0.4627","price_int":"20894719","amount_int":"46270000","tid":"1383225209557747","price_currency":"USD","item":"BTC","trade_type":"bid","primary":"Y","properties":"limit"},         
        ///         {"date":1383225477,"price":"208.89","amount":"1","price_int":"20889000","amount_int":"100000000","tid":"1383225477491209","price_currency":"USD","item":"BTC","trade_type":"ask","primary":"Y","properties":"limit"}
        ///      ]
        /// }
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
            EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["History"],Exchange.Name, currencyPair.Code);
            string json = e.Result;



            history.Clear();
            JObject jObject = JObject.Parse(json);
            JArray jArray = JArray.Parse(jObject["data"].ToString());

            foreach (var item in jArray)
            {
                var itemDef = new { date = 0, price = 0m, amount = 0f, trade_type = "" };
                var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);
                Deal deal = new Deal();

                deal.DealTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(itemResult.date);
                if (itemResult.trade_type == "bid")
                {
                    deal.DealType = DealType.Buy;
                }
                if (itemResult.trade_type == "ask")
                {
                    deal.DealType = DealType.Sell;
                }
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
