using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Btc38
{
    [Export(typeof(IQuotationApi))]
    public class QuotationApi : Btc38Api, IQuotationApi
    {

        public void UpdateQuotation(ref Quotation quotation)
        {
            CurrencyPair currencyPair = quotation.Ticker.CurrencyPair;
            string url = "http://www.btc38.com/trade/getTradeList.php?coinname=" + currencyPair.Base.Code.ToLower();
            using (WebClient webClient = new WebClient())
            {
                //解决403问题！
                webClient.Credentials = CredentialCache.DefaultCredentials; // 添加授权证书
                webClient.Headers.Add("User-Agent", "Microsoft Internet Explorer");
                webClient.Headers.Add("Host", "www.btc38.com");

                webClient.DownloadStringCompleted += DownloadQuotationCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Quotation"], Exchange.Name, currencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), quotation);
            }
        }

        /// <remarks>
        /// {
        ///     "buyOrder":
        ///     [
        ///         {"price":"3.515000","amount":"0.898688"},
        ///         {"price":"3.514000","amount":"0.900000"},
        ///         ...
        ///         {"price":"1.500000","amount":"116.128298"}
        ///     ], 
        ///     "sellOrder":
        ///     [
        ///         {"price":"4.380000","amount":"7.070000"},
        ///         {"price":"4.390000","amount":"10.970000"},
        ///         ...
        ///         {"price":"4.600000","amount":"20.653658"}
        ///     ],
        ///     "trade":
        ///     [
        ///         {"price":"4.499000","volume":"0.665933","time":"2013-10-16 14:10:30","type":"1"},
        ///         {"price":"3.500000","volume":"4.607775","time":"2013-10-16 14:06:54","type":"2"},
        ///         ...
        ///         {"price":"4.575000","volume":"4.591143","time":"2013-10-16 07:15:30","type":"2"}
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
                UpdateQuotaion(quotation, e.Result);
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["Quotation"], Exchange.Name, currencyPair.Code);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["Quotation"], Exchange.Name, currencyPair.Code, exception.Message);
            }
        }

        private void UpdateQuotaion(Quotation quotation, string jsonText)
        {
            JObject jObject = JObject.Parse(jsonText);

            decimal convertRate = quotation.Ticker.ConvertRate;

            PriceQuantityCollection buyOrders = GetOrders(jObject["buyOrder"].ToString(), convertRate);
            quotation.Ticker.BidPrice = buyOrders.First().OrginalPrice;
            quotation.BuyOrders = buyOrders;

            PriceQuantityCollection sellOrders = GetOrders(jObject["sellOrder"].ToString(), convertRate);
            quotation.Ticker.AskPrice = sellOrders.First().OrginalPrice;
            quotation.SellOrders = sellOrders;

            string historyJson = jObject["trade"].ToString();
            History history = GetHistory(historyJson, quotation.Ticker);

            quotation.History = history;
            quotation.Ticker.LastTradePrice = history.First().PriceQuantity.OrginalPrice;
            quotation.Ticker.HighPrice = history.Max(a => a.PriceQuantity.OrginalPrice);
            quotation.Ticker.LowPrice = history.Min(a => a.PriceQuantity.OrginalPrice);
            quotation.History = history;
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

        // 
        /// <summary>
        /// 从Json中获取历史交易数据.
        /// </summary>
        /// <param name="historyJson"></param>
        /// <returns></returns>
        /// <remarks>
        /// {"price":"4.499000","volume":"0.665933","time":"2013-10-16 14:10:30","type":"1"},
        /// </remarks>
        private History GetHistory(string historyJson, Ticker ticker)
        {
            JArray jArray = JArray.Parse(historyJson);
            History history = new History(ticker);
            foreach (var item in jArray)
            {
                var itemDef = new { time = new DateTime(), price = 0m, volume = 0m, type = 1 };
                var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);
                Deal deal = new Deal();

                DateTime dateTime = DateTime.Parse(item["time"].ToString());
                deal.DealTime = dateTime;

                deal.PriceQuantity = new PriceQuantityItem();
                decimal convertRate = ticker.ConvertRate;
                deal.PriceQuantity.ConvertRate = convertRate;

                deal.PriceQuantity.Price = itemResult.price;
                deal.PriceQuantity.Quantity = itemResult.volume;
                if (itemResult.type == 1) deal.DealType = DealType.Buy;
                if (itemResult.type == 2) deal.DealType = DealType.Sell;
                history.Add(deal);
            }
            return history;
        }
    }
}
