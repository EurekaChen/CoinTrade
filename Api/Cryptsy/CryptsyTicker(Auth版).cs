using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.MarketInfo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
namespace Eureka.CoinTrade.Api.Cryptsy
{  
    [Export(typeof(IUpdateTicker))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class  CryptsyTicker : CryptsyApi,IUpdateTicker
    {
        [Import]
        ApiManager apiManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticker"></param>
        /// <remarks>
        /// json示例：     
        /// {
        ///     "success":"1",
        ///     "return":
        ///     [
        ///         {"marketid":"57","label":"ALF\/BTC","primary_currency_code":"ALF","primary_currency_name":"AlphaCoin","secondary_currency_code":"BTC","secondary_currency_name":"BitCoin","current_volume":"1784417.96329225","last_trade":"0.00000139","high_trade":"0.00000246","low_trade":"0.00000122","created":"2013-07-04 01:01:09"},
        ///           ...
        ///         {"marketid":"67","label":"XNC\/LTC","primary_currency_code":"XNC","primary_currency_name":"XenCoin","secondary_currency_code":"LTC","secondary_currency_name":"LiteCoin","current_volume":"8203286.32267366","last_trade":"0.00011000","high_trade":"0.00012000","low_trade":"0.00008932","created":"2013-07-21 20:49:04"},
        ///         {"marketid":"103","label":"TIX\/XPM","primary_currency_code":"TIX","primary_currency_name":"Tickets","secondary_currency_code":"XPM","secondary_currency_name":"PrimeCoin","current_volume":"444654075.15056723","last_trade":"0.00000099","high_trade":"0.00000099","low_trade":"0.00000084","created":"2013-09-30 23:13:37"}
        ///     ]
        /// }
        /// </remarks>
        public void UpdateTicker(ref Ticker ticker)
        {
            IAuthQuery query =apiManager.GetAuthQuery(ticker.Exchange.AbbrName);
            query.AuthKey = ShareAuthKey;

            string json = query.AuthQuery("getmarkets", new Dictionary<string, string>()); 
         
            int id = PairIdDict[ticker.CurrencyPair.Code];
            JObject jObject = JObject.Parse(json);
            JArray jarray=JArray.Parse(jObject["return"].ToString());
            foreach (var item in jarray)
            {
                if (item["marketid"].ToString() == id.ToString())
                {
                    var def = new { current_volume = 0m, last_trade = 0m, high_trade = 0m, low_trade = 0m, created = new DateTime() };
                    var result = JsonConvert.DeserializeAnonymousType(item.ToString(), def);
                    ticker.HighPrice = result.high_trade;
                    ticker.LowPrice = result.low_trade;
                    ticker.LastTradePrice = result.low_trade;
                    ticker.Volume = result.current_volume;
                }
            } 
        }        
    }
}
