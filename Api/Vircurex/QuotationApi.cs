using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.ApiBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Eureka.CoinTrade.Infrastructure;
using System.ComponentModel.Composition;
using System.Collections.ObjectModel;

namespace Eureka.CoinTrade.Api.Vircurex
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : VircurexApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;       

        public void UpdateQuotation(ref Quotation quotation)
        {
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            string url = "https://vircurex.com/api/orderbook.json?base=" + currencyPair.Base.Code + "&alt=" + currencyPair.Code;

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
        /// 示例:(同样价格可能出现多条数据)
        /// {
        ///     "asks":
        ///     [
        ///         ["199.888","0.32405322"],
        ///         ["199.8888","1.88"],
        ///         ["200.0","0.01"],
        ///         ...
        ///         ["1000.4039","0.0100545"]
        ///     ],
        ///     "bids":
        ///     [
        ///         ["191.49200999","0.01145931"],
        ///         ["191.48944319","0.2"],
        ///         ["191.48870982","0.07530287"],
        ///         ...
        ///         ["0.8011","0.0078"]
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
            EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Quotation"],Exchange.Name, currencyPair.Code);
            string json = e.Result;
            JObject jObject = JObject.Parse(json);

            decimal convertRate = quotation.Ticker.ConvertRate;
           
            string asks = jObject["asks"].ToString();
           
            if (asks == "{}") return;
            PriceQuantityCollection sellOrders = GetOrders(asks, convertRate);
           
            quotation.SellOrders = sellOrders;

            string bids = jObject["bids"].ToString();
            if (bids == "{}") return;
            PriceQuantityCollection buyOrders = GetOrders(bids, convertRate);
            quotation.BuyOrders = buyOrders; 
    
            var ticker = quotation.Ticker;
            var tickerUpdater =apiManager.GetApi<ITickerApi>(Exchange.AbbrName);
            tickerUpdater.UpdateTicker(ref ticker);

            var refHistory = quotation.History;
            var historyUpdater =apiManager.GetApi<IHistoryApi>(Exchange.AbbrName);
            historyUpdater.UpdateHistory(ref refHistory);          
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
