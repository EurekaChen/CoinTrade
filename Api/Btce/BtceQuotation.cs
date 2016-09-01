using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Btce
{
    [Export(typeof(IQuotationApi))]
    public class BtceQuotation : BtceApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;

        public void UpdateQuotation(ref Quotation quotation)
        {
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://btc-e.com/api/2/" + codePair + "/depth";

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
        /// 示例:
        /// {
        ///     "asks":
        ///     [
        ///         [0.01114,304.72271323],
        ///         [0.01118,9.82615],
        ///         [0.0112,8.5631],
        ///         ...
        ///         [0.01288,82.95965672]
        ///      ],
        ///      "bids":
        ///      [
        ///         [0.01113,0.50642534],
        ///         [0.01112,2.95184741],
        ///         ...
        ///         [0.009,1277.83463737]
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
                UpdateQuotation(e.Result, ref quotation);
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Quotation"],Exchange.Name, currencyPair.Code);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["Quotation"],Exchange.Name, quotation.Ticker.CurrencyPair.Code, exception.Message);
                //不再更新报价和历史
                return;
            }

            var ticker = quotation.Ticker;
            var tickerUpdater = apiManager.GetApi<ITickerApi>(Exchange.AbbrName);
            tickerUpdater.UpdateTicker(ref ticker);

            var refHistory = quotation.History;
            var historyUpdater = apiManager.GetApi<IHistoryApi>(Exchange.AbbrName);
            historyUpdater.UpdateHistory(ref refHistory);
        }

        private void UpdateQuotation(string json, ref Quotation quotation)
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
