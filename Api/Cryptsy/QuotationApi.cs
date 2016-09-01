using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Cryptsy
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : CryptsyApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;

        public void UpdateQuotation(ref Quotation quotation)
        {
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            int marketId = PairIdDict[currencyPair.Code];
            string url = "http://pubapi.cryptsy.com/api.php?method=singlemarketdata&marketid=" + marketId.ToString();
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadQuotationCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Quotation"], Exchange.Name, currencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), quotation);
            }
        }


        /// <remarks>
        /// 示例：
        /// {
        ///     "success":1,
        ///     "return":
        ///     {
        ///         "markets":
        ///         {
        ///             "ADT":
        ///             {
        ///                 "marketid":"94","label":"ADT\/LTC","lasttradeprice":"0.00000034","volume":"4939660728.20205000","lasttradetime":"2013-10-08 21:29:25","primaryname":"AndroidsTokens","primarycode":"ADT","secondaryname":"LiteCoin","secondarycode":"LTC",
        ///                 "recenttrades":
        ///                 [
        ///                     {"id":"2799445","time":"2013-10-08 21:30:38","price":"0.00000034","quantity":"3145728.00000000","total":"1.06954752"},
        ///                     {"id":"2799427","time":"2013-10-08 21:30:26","price":"0.00000034","quantity":"43366887.39693867","total":"14.74474171"},
        ///                     ......
        ///                     {"id":"2794595","time":"2013-10-08 17:43:12","price":"0.00000036","quantity":"200.00000000","total":"0.00007200"}
        ///                 ],
        ///                 "sellorders":
        ///                 [
        ///                     {"price":"0.00000034","quantity":"0.00000000","total":"23.04005139"},
        ///                     {"price":"0.00000035","quantity":"14065586.50548920","total":"8.54754779"},
        ///                     ......
        ///                     {"price":"0.00000054","quantity":"20188647.57508300","total":"10.90186969"}
        ///                 ],
        ///                 "buyorders":
        ///                 [
        ///                     {"price":"0.00000034","quantity":"236942380.34686786","total":"96.37469855"},
        ///                     {"price":"0.00000033","quantity":"348878204.21638125","total":"119.17023287"},
        ///                     ......         
        ///                     {"price":"0.00000010","quantity":"80000000.00000000","total":"8.00000000"},
        ///                     {"price":"0.00000008","quantity":"30000000.00000000","total":"2.40000000"}
        ///                 ]
        ///             }
        ///         }
        ///     }
        ///}
        ///   </remarks>
        private void DownloadQuotationCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Quotation quotation = e.UserState as Quotation;

            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadDataAsyncError(DataTypeDict["Quotation"], Exchange.Name, quotation.Ticker.CurrencyPair.Code, e.Error.Message);
                return;
            }

            string json = e.Result;
            try
            {
                UpdateQuotation(quotation, e.Result);
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Quotation"], Exchange.Name, quotation.Ticker.CurrencyPair.Code);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["Quotation"], Exchange.Name, quotation.Ticker.CurrencyPair.Code, exception.Message);
            }
        }

        private void UpdateQuotation(Quotation quotation, string json)
        {
            JObject jObject = JObject.Parse(json);
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            string priceVolumeJson = jObject["return"]["markets"][currencyPair.Base.Code].ToString();
            var priceVolumeDef = new { lasttradeprice = 0m, volume = 0m };
            var priceVolumeResult = JsonConvert.DeserializeAnonymousType(priceVolumeJson, priceVolumeDef);

            decimal convertRate = quotation.Ticker.ConvertRate;

            string asks = jObject["return"]["markets"][currencyPair.Base.Code]["buyorders"].ToString();
            //当心买盘或卖盘为0时出错！
            var buyOrders = GetOrders(asks, convertRate);
            if (buyOrders.Count > 0)
            {
                quotation.Ticker.BidPrice = buyOrders.First().OrginalPrice;
                quotation.BuyOrders = buyOrders;
            }
            string bids = jObject["return"]["markets"][currencyPair.Base.Code]["sellorders"].ToString();
            var sellOrders = GetOrders(bids, convertRate);
            quotation.Ticker.AskPrice = sellOrders.First().OrginalPrice;
            quotation.SellOrders = sellOrders;

            string historyJson = jObject["return"]["markets"][currencyPair.Base.Code]["recenttrades"].ToString();
            History history = GetHistory(historyJson, quotation.Ticker);
            quotation.History = history;
            quotation.Ticker.HighPrice = history.Max(a => a.PriceQuantity.OrginalPrice);
            quotation.Ticker.LowPrice = history.Min(a => a.PriceQuantity.OrginalPrice);
            quotation.Ticker.Volume = history.Sum(a => a.PriceQuantity.Quantity);
            quotation.Ticker.LastTradePrice = history.First().PriceQuantity.OrginalPrice;
        }

        //
        /// <summary>
        /// 从Api的Json数据获取订单。
        /// </summary>
        /// <param name="ordersJson"></param>
        /// <returns></returns>
        /// <example>
        /// {"price":"0.00000034","quantity":"0.00000000","total":"23.04005139"},
        /// </example>
        private PriceQuantityCollection GetOrders(string ordersJson, decimal convertRate)
        {
            //当心买盘为0时orderJson为空！
            try
            {
                JArray asksJArray = JArray.Parse(ordersJson);
                PriceQuantityCollection orders = new PriceQuantityCollection();
                foreach (var item in asksJArray)
                {
                    var itemDef = new { price = 0m, quantity = 0m };
                    var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);
                    PriceQuantityItem order = new PriceQuantityItem();
                    order.ConvertRate = convertRate;
                    order.Price = itemResult.price;
                    order.Quantity = itemResult.quantity;
                    orders.Add(order);
                }
                return orders;
            }
            catch
            {
                return new PriceQuantityCollection();
            }
        }


        /// <summary>
        /// 从Json中获取历史数据.
        /// </summary>
        /// <param name="historyJson">json数据</param>
        /// <returns></returns>
        /// <example>
        /// {"id":"2799445","time":"2013-10-08 21:30:38","price":"0.00000034","quantity":"3145728.00000000","total":"1.06954752"},
        /// </example>
        private History GetHistory(string historyJson, Ticker ticker)
        {
            JArray jArray = JArray.Parse(historyJson);
            History history = new History(ticker);
            foreach (var item in jArray)
            {
                var itemDef = new { id = 1, time = new DateTime(), price = 0m, quantity = 0m };
                var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);
                Deal deal = new Deal();

                DateTime dateTime = DateTime.Parse(item["time"].ToString());
                deal.DealType = DealType.Unkown;
                deal.DealTime = dateTime; // itemDef.time;
                deal.PriceQuantity = new PriceQuantityItem();
                decimal convertRate = ticker.ConvertRate;
                deal.PriceQuantity.ConvertRate = convertRate;
                deal.PriceQuantity.Price = itemResult.price;
                deal.PriceQuantity.Quantity = itemResult.quantity;
                history.Add(deal);
            }
            return history;
        }
    }
}