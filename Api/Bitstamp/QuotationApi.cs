using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Bitstamp
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : BitstampApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;

        public void UpdateQuotation(ref Quotation quotation)
        {
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            string url = "https://www.bitstamp.net/api/order_book/";

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadQuotationCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Quotation"],Exchange.Name, currencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), quotation);
            }
        }

        /// <summary>
        /// 获取市场深度。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// {
        ///     "timestamp": "1383391875", 
        ///     "bids": 
        ///     [
        ///         ["204.40", "0.00400000"], 
        ///         ["204.21", "0.75600000"],
        ///         ["204.01", "0.32340000"], 
        ///         ... 
        ///         ["0.01", "34278.00000000"]
        ///      ], 
        ///      "asks": 
        ///      [
        ///         ["204.42", "11.68943217"], 
        ///         ["204.60", "0.04260000"],
        ///         ...
        ///         ["99999.00", "1.11000000"], 
        ///         ["99999.99", "1.50269550"]
        ///      ]
        /// }
        /// </remarks>
        private void DownloadQuotationCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Quotation quotation = e.UserState as Quotation;
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadDataAsyncError(DataTypeDict["Quotation"],Exchange.Name, currencyPair.Code, e.Error.Message);
                return;
            }

            try
            {
                UpdateQuotation(quotation, e.Result);
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Quotation"],Exchange.Name, currencyPair.Code);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["Quotation"],Exchange.Name, currencyPair.Code, exception.Message);
                return;
            }

            var ticker = quotation.Ticker;
            var tickerUpdater = apiManager.GetApi<ITickerApi>(Exchange.AbbrName);
            tickerUpdater.UpdateTicker(ref ticker);

            var refHistory = quotation.History;
            var historyUpdater = apiManager.GetApi<IHistoryApi>(Exchange.AbbrName);
            historyUpdater.UpdateHistory(ref refHistory);
        }

        private void UpdateQuotation(Quotation quotation, string json)
        {
            JObject jObject = JObject.Parse(json);

            decimal convertRate = quotation.Ticker.ConvertRate;

            string asks = jObject["asks"].ToString();
            PriceQuantityCollection sellOrders = GetOrders(asks, convertRate);

            quotation.SellOrders = sellOrders;

            string bids = jObject["bids"].ToString();
            PriceQuantityCollection buyOrders = GetOrders(bids, convertRate);
            quotation.BuyOrders = buyOrders;
        }
        private PriceQuantityCollection GetOrders(string ordersJson, decimal convertRate)
        {
            JArray ordersJArray = JArray.Parse(ordersJson);
            PriceQuantityCollection orders = new PriceQuantityCollection();
            foreach (var item in ordersJArray)
            {
                List<decimal> itemResult = JsonConvert.DeserializeObject<List<decimal>>(item.ToString());
                PriceQuantityItem order = new PriceQuantityItem();
                order.Price = itemResult[0];
                order.ConvertRate = convertRate;
                order.Quantity = itemResult[1];
                orders.Add(order);
            }
            return orders;
        }
    }
}
