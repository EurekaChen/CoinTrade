using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Xchange796
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : Xchange796cApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;

        public void UpdateQuotation(ref Quotation quotation)
        {
            //string url = "http://api.796.com/apiV2/depth/100.html?op=futures";
            string url = "http://api.796.com/apiV2/depth/50.html?op=futures";
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
        ///{
        ///     "result":"success",
        ///     "return":
        ///     {
        ///         "asks":
        ///         [
        ///             ["799.00","0.1"],
        ///             ["799.99","0.78"],
        ///             ["800.00","0.2"],
        ///             ...
        ///             ["890.00","10"],
        ///             ["900.00","10"]
        ///         ],
        ///         "bids":
        ///         [
        ///             ["789.00","0.5"],
        ///             ["787.00","1"],
        ///             ["786.00","0.5"],
        ///             ...
        ///             ["466.00","10"],
        ///             ["451.00","30"]
        ///         ]
        ///     }
        /// }
        /// </remarks>
        private void UpdateQuotation(Quotation quotation, string json)
        {
            JObject jObject = JObject.Parse(json);
            decimal convertRate = quotation.Ticker.ConvertRate;
            string asks = jObject["return"]["asks"].ToString();
            quotation.BuyOrders = GetOrders(asks, convertRate);

            string bids = jObject["return"]["bids"].ToString();
            quotation.SellOrders = GetOrders(bids, convertRate);
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
