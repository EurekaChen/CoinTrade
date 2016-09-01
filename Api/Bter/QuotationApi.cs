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

namespace Eureka.CoinTrade.Api.Bter
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : BterApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;

        public void UpdateQuotation(ref Quotation quotation)
        {
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://bter.com/api/1/depth/" + codePair;

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
        ///{
        ///     "result":"true",
        ///     "asks":
        ///     [
        ///         [10000000,0.0187],[1000000,0.01],[1099,0.1],[900,0.0137]...
        ///     ],
        ///     "bids":
        ///     [   
        ///         [815,5.78926378],[813,2.02678548],[810.01,0.0002018]...
        ///     ]
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
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["Quotation"],Exchange.Name, quotation.Ticker.CurrencyPair.Code, exception.Message);
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
