using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.CryptoTrade
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : CryptoTradeApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;

        public void UpdateQuotation(ref Quotation quotation)
        {
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://crypto-trade.com/api/1/depth/" + codePair;

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadQuotationCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Quotation"], Exchange.Name, currencyPair.Code);
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
        ///         ["873","0.01"],
        ///         ["898","0.61068057"],
        ///         ["899","0.01302876"],
        ///         ...
        ///         ["100000","0.02"],
        ///         ["100001","0.01734345"]
        ///     ],
        ///     "bids":
        ///     [
        ///         ["825.01001","0.06174"],
        ///         ["825.01","0.3"],
        ///         ["815.003","0.302"],
        ///         ...
        ///         ["0.0001","5"],
        ///         ["0.00002","0.01499998"]
        ///     ]
        /// }
        /// </remarks>
        private void UpdateQuotation(Quotation quotation, string json)
        {
            JObject jObject = JObject.Parse(json);
            decimal convertRate = quotation.Ticker.ConvertRate;
            string asks = jObject["asks"].ToString();
            quotation.SellOrders = GetOrders(asks, convertRate);

            string bids = jObject["bids"].ToString();
            quotation.BuyOrders = GetOrders(bids, convertRate);

        }

        //PriceQuantityCollection使用了ObservableCollection，需要对System.Windows的引用。
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
