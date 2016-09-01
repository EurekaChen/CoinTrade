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
using Eureka.CoinTrade.Infrastructure.Event;

namespace Eureka.CoinTrade.Api.MtGox
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : MtGoxApi, IQuotationApi
    {
        [Import]
        ApiManager apiManager;       

        public void UpdateQuotation(ref Quotation quotation)
        {
            string pairCode =quotation.Ticker.CurrencyPair.Code;
            string url = "http://data.mtgox.com/api/2/" +  pairCode + "/money/depth";

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadQuotationCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Quotation"],Exchange.Name, pairCode);
                webClient.DownloadStringAsync(new Uri(url), quotation);
            }
        }

        /// <summary>
        /// 获取市场深度。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// {
        ///     "result":"success",
        ///     "data":
        ///     {
        ///         "now":"1383224838039898",
        ///         "cached":"1383224866772290",
        ///         "asks":
        ///         [
        ///             {"price":208.94703,"amount":7.01607518,"price_int":"20894703","amount_int":"701607518","stamp":"1383224727467052"},
        ///             {"price":208.94727,"amount":0.17055584,"price_int":"20894727","amount_int":"17055584","stamp":"1383223959232186"},
        ///             ...
        ///             {"price":209,"amount":0.14626393,"price_int":"20900000","amount_int":"14626393","stamp":"1383223923641299"},
        ///             {"price":229.81166,"amount":15.089159,"price_int":"22981166","amount_int":"1508915900","stamp":"1383168835197644"}
        ///         ],
        ///         "bids":
        ///         [
        ///             {"price":188.05239,"amount":0.25,"price_int":"18805239","amount_int":"25000000","stamp":"1383224793033001"},
        ///             {"price":188.0681,"amount":0.01030221,"price_int":"18806810","amount_int":"1030221","stamp":"1382834704530916"},
        ///             ...
        ///             {"price":208.06732,"amount":0.02,"price_int":"20806732","amount_int":"2000000","stamp":"1383224402696243"},
        ///             {"price":208.08,"amount":5,"price_int":"20808000","amount_int":"500000000","stamp":"1383224546961865"}
        ///         ],
        ///         "filter_min_price":{"value":"188.05239","value_int":"18805239","display":"$188.05","display_short":"$188.05","currency":"USD"},
        ///         "filter_max_price":{"value":"229.84181","value_int":"22984181","display":"$229.84","display_short":"$229.84","currency":"USD"}
        ///    }
        ///}
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
           
            string asks = jObject["data"]["asks"].ToString(); 
            
            PriceQuantityCollection sellOrders = GetOrders(asks, convertRate);           
            quotation.SellOrders = sellOrders;

            string bids = jObject["data"]["bids"].ToString();
            PriceQuantityCollection buyOrders = GetOrders(bids, convertRate);
            quotation.BuyOrders = buyOrders; 
    
            var ticker = quotation.Ticker;
            var tickerUpdater = apiManager.GetApi<ITickerApi>(Exchange.AbbrName);
            tickerUpdater.UpdateTicker(ref ticker);

            var refHistory = quotation.History;
            var historyUpdater =apiManager.GetApi<IHistoryApi>(Exchange.AbbrName);
            historyUpdater.UpdateHistory(ref refHistory);          
        }

        private PriceQuantityCollection GetOrders(string ordersJson, decimal convertRate)
        {
            JArray asksJArray = JArray.Parse(ordersJson);
            PriceQuantityCollection orders = new PriceQuantityCollection();
            foreach (var item in asksJArray)
            {
                var itemDef = new { price = 0m, amount = 0m };
                var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);
                PriceQuantityItem order = new PriceQuantityItem();
                order.ConvertRate = convertRate;
                order.Price = itemResult.price;
                order.Quantity = itemResult.amount;
                orders.Add(order);
            }
            return orders;
        }
    }
}
