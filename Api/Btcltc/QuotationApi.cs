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

namespace Eureka.CoinTrade.Api.Btcltc
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : BtcltcApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;

        public void UpdateQuotation(ref Quotation quotation)
        {
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "http://www.btcltc.com/index.php?g=home&m=api&a=index&type=depth&t=" + codePair;

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
        ///         ["0.00000026","20000000.00000000"],
        ///         ["0.00000025","30000000.00000000"],
        ///         ...
        ///         ["0.00000024","10000000.00000000"]              
        ///     ],
        ///     "bids":
        ///     [
        ///         ["0.00001500","10000000.00000000"],
        ///         ["0.00000500","5789342.17000000"],
        ///         ...
        ///         ["0.00000300","3940907.62934130"]
        ///     ]
        /// }
        /// </remarks>
        private void UpdateQuotation(Quotation quotation, string json)
        {
            JObject jObject = JObject.Parse(json);
            decimal convertRate = quotation.Ticker.ConvertRate;
            string asks = jObject["asks"].ToString();
            quotation.BuyOrders = GetOrders(asks, convertRate);

            string bids = jObject["bids"].ToString();
            PriceQuantityCollection sellOrders = GetOrders(bids, convertRate);
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
