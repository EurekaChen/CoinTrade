using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Fxbtc
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : FxbtcApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;

        public void UpdateQuotation(ref Quotation quotation)
        {
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            string codePair = currencyPair.Base.Code.ToLower() + "_" + currencyPair.Quote.Code.ToLower();
            string url = "https://data.fxbtc.com/api?op=query_depth&symbol=" + codePair;

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
        ///     "result":true,
        ///     "symbol":"btc_cny",
        ///     "depth":
        ///     {
        ///         "asks":
        ///         [
        ///             {"count":1,"rate":5000,"vol":1.9645},
        ///             {"count":1,"rate":5060.0001,"vol":0.0904},
        ///             {"count":3,"rate":5100,"vol":0.2194},
        ///             {"count":1,"rate":5138,"vol":0.1024},
        ///             ...
        ///             {"count":1,"rate":6996,"vol":0.2994},
        ///             {"count":1,"rate":6998.999,"vol":0.499}
        ///         ],
        ///         "bids":
        ///         [
        ///             {"count":1,"rate":4930,"vol":1},
        ///             {"count":1,"rate":4889.01,"vol":0.3},
        ///             {"count":1,"rate":4889,"vol":2.5},
        ///             ...
        ///             {"count":1,"rate":3146.26,"vol":4.11},
        ///             {"count":1,"rate":3142.52,"vol":2.16}
        ///         ]
        ///     },
        ///     "params":
        ///     {
        ///         "symbol":"btc_cny","count":100
        ///     }
        /// }
        /// 
        /// </remarks>
        private void UpdateQuotation(Quotation quotation, string json)
        {
            JObject jObject = JObject.Parse(json);
            decimal convertRate = quotation.Ticker.ConvertRate;
            string asks = jObject["depth"]["asks"].ToString();
            quotation.SellOrders = GetOrders(asks, convertRate);

            string bids = jObject["depth"]["bids"].ToString();
            quotation.BuyOrders = GetOrders(bids, convertRate);
        }

        private PriceQuantityCollection GetOrders(string ordersJson, decimal convertRate)
        {
            JArray asksJArray = JArray.Parse(ordersJson);
            PriceQuantityCollection orders = new PriceQuantityCollection();
            foreach (var item in asksJArray)
            {
                var def = new { count = 0, rate = 0m, vol = 0m };
                var result = JsonConvert.DeserializeAnonymousType(item.ToString(), def);
                PriceQuantityItem order = new PriceQuantityItem();
                order.ConvertRate = convertRate;
                order.Price = result.rate;
                order.Quantity = result.vol;
                orders.Add(order);
            }
            return orders;
        }
    }
}
