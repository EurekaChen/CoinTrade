using System;
using System.Collections.Generic;
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
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : RmbtbApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;

        public void UpdateQuotation(ref Quotation quotation)
        {
            string url = "https://www.rmbtb.com/api/thirdparty/depth/";

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadQuotationCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Quotation"], Exchange.Name, quotation.Ticker.CurrencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), quotation);
            }
        }


        private void DownloadQuotationCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Quotation quotation = e.UserState as Quotation;
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadDataAsyncError(DataTypeDict["Quotation"], Exchange.Name, currencyPair.Code, e.Error.Message);
                return;
            }
            try
            {
                UpdateQuotation(quotation, e.Result);
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Quotation"], Exchange.Name, currencyPair.Code);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["Quotation"], Exchange.Name, quotation.Ticker.CurrencyPair.Code, exception.Message);
                return;
            }

            var ticker = quotation.Ticker;
            var tickerUpdater = apiManager.GetApi<ITickerApi>(Exchange.AbbrName);
            tickerUpdater.UpdateTicker(ref ticker);

            var refHistory = quotation.History;
            var historyUpdater = apiManager.GetApi<IHistoryApi>(Exchange.AbbrName);
            historyUpdater.UpdateHistory(ref refHistory);
        }

        /// <summary>
        /// 获取市场深度。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// {
        ///     "asks":
        ///     [
        ///         ["30000.00","1.0000"],
        ///         ["15000.00","5.0000"],
        ///         ["12000.00","4.0000"],
        ///         ...
        ///         ["4892.83","2.5760"],
        ///         ["4857.16","0.7830"]
        ///     ],
        ///     "bids":
        ///     [
        ///         ["4753.97","1.6480"],
        ///         ["4747.14","0.8190"],
        ///         ["4745.15","2.8430"],
        ///         ...
        ///         ["45.00","2.0000"],
        ///         ["0.10","9.0000"]
        ///     ]
        /// }
        /// </remarks>
        private void UpdateQuotation(Quotation quotation, string json)
        {
            JObject jObject = JObject.Parse(json);
            decimal convertRate = quotation.Ticker.ConvertRate;
            string asks = jObject["asks"].ToString();

            PriceQuantityCollection sellOrders = GetOrders(asks, convertRate);
            var reverse = sellOrders.Reverse();
            var reverseOrders = new PriceQuantityCollection();
            foreach (PriceQuantityItem order in reverse)
            {
                reverseOrders.Add(order);
            }
            quotation.SellOrders = reverseOrders;

            string bids = jObject["bids"].ToString();
            quotation.BuyOrders = GetOrders(bids, convertRate);

        }

        private PriceQuantityCollection GetOrders(string ordersJson, decimal convertRate)
        {
            JArray asksJArray = JArray.Parse(ordersJson);
            PriceQuantityCollection orders = new PriceQuantityCollection();
            foreach (var item in asksJArray)
            {
                List<decimal> itemResult = JsonConvert.DeserializeObject<List<decimal>>(item.ToString());
                PriceQuantityItem order = new PriceQuantityItem();
                order.ConvertRate = convertRate;
                order.Price = itemResult[0];
                order.Quantity = itemResult[1];
                orders.Add(order);
            }
            return orders;
        }
    }
}
