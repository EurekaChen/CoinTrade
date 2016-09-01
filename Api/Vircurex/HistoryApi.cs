using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.Vircurex
{

    [Export(typeof(IHistoryApi))]
    public class HistoryApi : VircurexApi, IHistoryApi
    {

        public void UpdateHistory(ref History history)
        {
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            string url = "https://vircurex.com/api/trades.json?base=" + currencyPair.Base.Code + "&alt=" + currencyPair.Quote.Code;
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadHistoryCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["History"], Exchange.Name, currencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), history);
            }
        }
        /// <remarks>
        ///[
        ///     {"date":1382677103,"tid":636036,"amount":"0.39978026","price":"185.0"},
        ///     {"date":1382694734,"tid":636466,"amount":"0.1204","price":"181.1029"},
        ///     {"date":1382697661,"tid":636884,"amount":"0.00350999","price":"181.00006154"},
        ///     ...
        ///     {"date":1383268800,"tid":644236,"amount":"0.0313532","price":"199.88806268"},
        ///     {"date":1383268892,"tid":644246,"amount":"0.02710337","price":"199.88806268"}
        ///]
        /// </remarsk>     
        private void DownloadHistoryCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            History history = e.UserState as History;
            CurrencyPair currencyPair = history.Ticker.CurrencyPair;
            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadDataAsyncError(DataTypeDict["History"], Exchange.Name, currencyPair.Code, e.Error.Message);
                return;
            }
            try
            {
                UpdateHistory(ref history, e.Result);
                EventSourceLogger.Logger.UpdateDataSuccess(DataTypeDict["History"], Exchange.Name, currencyPair.Code);
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["History"], Exchange.Name, history.Ticker.CurrencyPair.Code, exception.Message);
            }
        }

        private static void UpdateHistory(ref History history, string json)
        {
            JArray jArray = JArray.Parse(json);

            history.Clear();
            foreach (var item in jArray)
            {
                var itemDef = new { date = 0, tid = 1, amount = 0m, price = 0m };
                var itemResult = JsonConvert.DeserializeAnonymousType(item.ToString(), itemDef);
                Deal deal = new Deal();

                deal.DealTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(itemResult.date);

                deal.PriceQuantity = new PriceQuantityItem();
                decimal convertRate = history.Ticker.ConvertRate;
                deal.PriceQuantity.ConvertRate = convertRate;
                deal.PriceQuantity.Price = itemResult.price;
                deal.PriceQuantity.Quantity = itemResult.amount;
                history.Add(deal);
            }
        }
    }
}
