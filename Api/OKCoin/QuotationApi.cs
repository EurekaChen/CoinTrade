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

namespace Eureka.CoinTrade.Api.OKCoin
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : OKCoinApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;

        public void UpdateQuotation(ref Quotation quotation)
        {
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://www.okcoin.com/api/depth.do?symbol=" + codePair;

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadQuotationCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Quotation"], Exchange.Name, currencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), quotation);
            }
        }

        /// <summary>
        /// 获取市场深度。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// https://btcltc.com/home/api/index/type/depth/t/ltc_cny
        /// {
        ///     "asks":
        ///     [
        ///         [174.95,200],
        ///         [174.94,2.385],
        ///         [174.93,7],
        ///         [174.9,691.648],
        ///         ...
        ///         [172.88,1545.3],
        ///         [172.6,1]
        ///      ],
        ///      "bids":
        ///      [
        ///         [172.6,405],
        ///         [172.51,172.468],
        ///         [172.5,1841.62],
        ///         ...
        ///         [169.3,6.492],
        ///         [169.1,5]
        ///     ]
        /// }
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
            string bids = jObject["bids"].ToString();
            quotation.BuyOrders = GetOrders(bids, convertRate);

            string asks = jObject["asks"].ToString();
            PriceQuantityCollection sellOrders = GetOrders(asks, convertRate);
            var reverse = sellOrders.Reverse();
            var reverseOrders = new PriceQuantityCollection();
            foreach (PriceQuantityItem order in reverse)
            {
                reverseOrders.Add(order);
            }
            quotation.SellOrders = reverseOrders;
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
