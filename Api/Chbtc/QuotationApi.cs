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

namespace Eureka.CoinTrade.Api.Chbtc
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : ChbtcApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;

        public void UpdateQuotation(ref Quotation quotation)
        {
            string url = "http://api.chbtc.com/data/depth";
            if (quotation.Ticker.CurrencyPair.Base.Code == "LTC")
            {
                url = " http://api.chbtc.com/data/ltc/depth";
                //返回最近历史,可以跟：?since=5000 .
            }

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadQuotationCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Quotation"], Exchange.Name, quotation.Ticker.CurrencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), quotation);
            }
        }

        /// <summary>
        /// 获取市场深度。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// 示例:
        /// {
        ///     "asks":
        ///     [
        ///         [1.0e+14,2.026],
        ///         [10000000000000,0.02],
        ///         [1000000000000,0.001],
        ///         [999999999999.99,0.001],
        ///         ...
        ///         [1318.5,0.672],
        ///         [1318.4,7.666]
        ///     ],
        ///     "bids":
        ///     [
        ///         [1317.03,0.1],
        ///         [1317.02,0.841],
        ///         ...
        ///         [0.02,7363.5],
        ///         [0.01,167197.741]
        ///     ]
        ///  }
        /// </remarks>
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
            PriceQuantityCollection buyOrders = GetOrders(bids, convertRate);
            quotation.BuyOrders = buyOrders;
        }
        private PriceQuantityCollection GetOrders(string ordersJson, decimal convertRate)
        {
            JArray ordersJArray = JArray.Parse(ordersJson);
            PriceQuantityCollection orders = new PriceQuantityCollection();
            foreach (var item in ordersJArray)
            {
                //当心科学计数！
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
