using System;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json.Linq;

namespace Eureka.CoinTrade.Api.MtGox
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : MtGoxApi, ITickerApi
    {
        public void UpdateTicker(ref Ticker ticker)
        {
            string url = "https://data.mtgox.com/api/2/" + ticker.CurrencyPair.Code + "/money/ticker";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += DownloadTickerCompleted;
                EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Ticker"],Exchange.Name, ticker.CurrencyPair.Code);
                webClient.DownloadStringAsync(new Uri(url), ticker);
            }
        }


        /// <summary>
        /// 更新报价数据。
        /// </summary>
        /// <param name="currencyPair"></param>
        /// <example>
        /// https://data.mtgox.com/api/2/BTCUSD/money/ticker
        /// {
        ///     "result":"success",
        ///     "data":
        ///     {
        ///         "high":{"value":"187.40000","value_int":"18740000","display":"$187.40","display_short":"$187.40","currency":"USD"},
        ///         "low": {"value":"177.25000","value_int":"17725000","display":"$177.25","display_short":"$177.25","currency":"USD"},
        ///         "avg":{"value":"182.79308","value_int":"18279308","display":"$182.79","display_short":"$182.79","currency":"USD"},
        ///         "vwap":{"value":"182.98154","value_int":"18298154","display":"$182.98","display_short":"$182.98","currency":"USD"},
        ///         "vol":{"value":"11211.97164376","value_int":"1121197164376","display":"11,211.97\u00a0BTC","display_short":"11,211.97\u00a0BTC","currency":"BTC"},
        ///         "last_local":{"value":"186.05142","value_int":"18605142","display":"$186.05","display_short":"$186.05","currency":"USD"},
        ///         "last_orig":{"value":"186.05142","value_int":"18605142","display":"$186.05","display_short":"$186.05","currency":"USD"},
        ///         "last_all":{"value":"186.05142","value_int":"18605142","display":"$186.05","display_short":"$186.05","currency":"USD"},
        ///         "last":{"value":"186.05142","value_int":"18605142","display":"$186.05","display_short":"$186.05","currency":"USD"},
        ///         "buy":{"value":"186.05142","value_int":"18605142","display":"$186.05","display_short":"$186.05","currency":"USD"},
        ///         "sell":{"value":"187.02538","value_int":"18702538","display":"$187.03","display_short":"$187.03","currency":"USD"},
        ///         "item":"BTC",
        ///         "now":"1382315652651232"
        ///     }
        /// }
        /// 说明：详见非官方文档：https://bitbucket.org/nitrous/mtgox-api/overview#markdown-header-contents
        /// last_local:所选择货币的最新价。
        /// last_orig:所有货币的最新价
        /// last_all最新价转换成辅币
        /// last 等同last_local
        /// </remarks>
        private void DownloadTickerCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Ticker ticker = e.UserState as Ticker;
            if (e.Error != null)
            {
                EventSourceLogger.Logger.DownloadDataAsyncError(DataTypeDict["Ticker"],Exchange.Name, ticker.CurrencyPair.Code, e.Error.Message);
                return;
            }
            try
            {
                string jsonText = e.Result;
                JObject jObject = JObject.Parse(jsonText);
                ticker.LastTradePrice = Convert.ToDecimal(jObject["data"]["last"]["value"].ToString());
                ticker.HighPrice = Convert.ToDecimal(jObject["data"]["high"]["value"].ToString());
                ticker.LowPrice = Convert.ToDecimal(jObject["data"]["low"]["value"].ToString());
                ticker.AskPrice = Convert.ToDecimal(jObject["data"]["sell"]["value"].ToString());
                ticker.BidPrice = Convert.ToDecimal(jObject["data"]["buy"]["value"].ToString());
                ticker.Volume = Convert.ToDecimal(jObject["data"]["vol"]["value"].ToString());
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.UpdateDataException(DataTypeDict["Ticker"],Exchange.Name, ticker.CurrencyPair.Code, exception.Message);
                return;
            }
        }
    }
}
