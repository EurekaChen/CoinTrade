
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
namespace Eureka.CoinTrade.AuthAccess
{
    class CryptsyParse
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        /// <example>
        /// {
        ///     "success":"1",
        ///     "return":
        ///     {
        ///         "balances_available":
        ///         {"ALF":"0.00000000","AMC":"0.00000000","ADT":"152000000.00000000",.......,"XNC":"0.00000000","YAC":"0.00000000","YBC":"0.00000000","ZET":"0.00000000"},
        ///         "servertimestamp":1382449122,
        ///         "servertimezone":"EST",
        ///         "serverdatetime":"2013-10-22 09:38:42",
        ///         "openordercount":1
        ///      }
        /// }
        /// </example>
        public static Dictionary<Currency, decimal> ParseGetinfo(string json)
        {
            Dictionary<Currency, decimal> currencies = new Dictionary<Currency, decimal>();

            return currencies;

        }

        ///getmarkets
        /// {
        ///     "success":"1",
        ///     "return":
        ///     [
        ///         {"marketid":"57","label":"ALF\/BTC","primary_currency_code":"ALF","primary_currency_name":"AlphaCoin","secondary_currency_code":"BTC","secondary_currency_name":"BitCoin","current_volume":"1784417.96329225","last_trade":"0.00000139","high_trade":"0.00000246","low_trade":"0.00000122","created":"2013-07-04 01:01:09"},
        ///         {"marketid":"43","label":"AMC\/BTC","primary_currency_code":"AMC","primary_currency_name":"AmericanCoin","secondary_currency_code":"BTC","secondary_currency_name":"BitCoin","current_volume":"32401.53135330","last_trade":"0.00000306","high_trade":"0.00000316","low_trade":"0.00000306","created":"2013-06-06 07:16:21"},
        ///         {"marketid":"66","label":"ANC\/BTC","primary_currency_code":"ANC","primary_currency_name":"AnonCoin","secondary_currency_code":"BTC","secondary_currency_name":"BitCoin","current_volume":"8808.27636700","last_trade":"0.00135000","high_trade":"0.00183900","low_trade":"0.00090000","created":"2013-07-21 20:48:02"},
        ///         ...
        ///         {"marketid":"67","label":"XNC\/LTC","primary_currency_code":"XNC","primary_currency_name":"XenCoin","secondary_currency_code":"LTC","secondary_currency_name":"LiteCoin","current_volume":"8203286.32267366","last_trade":"0.00011000","high_trade":"0.00012000","low_trade":"0.00008932","created":"2013-07-21 20:49:04"},
        ///         {"marketid":"103","label":"TIX\/XPM","primary_currency_code":"TIX","primary_currency_name":"Tickets","secondary_currency_code":"XPM","secondary_currency_name":"PrimeCoin","current_volume":"444654075.15056723","last_trade":"0.00000099","high_trade":"0.00000099","low_trade":"0.00000084","created":"2013-09-30 23:13:37"}
        ///     ]
        /// }
        public static CurrencyPairs  GetAll(string marketJson)
        {
            CurrencyPairs pairs = new CurrencyPairs();
            JObject jObject = JObject.Parse(marketJson);
            JArray jarray=JArray.Parse(jObject["return"].ToString());
            var def= new {primary_currency_code="",secondary_currency_code="",};
            foreach(var item in jarray)
            {
                var result=  JsonConvert.DeserializeAnonymousType(item.ToString(), def);
                pairs.Add(CurrencyPair.All[result.primary_currency_code +result.secondary_currency_code]);
              //  CryptoCoin coin = new CryptoCoin() { Name = result.primary_currency_name, Code = result.primary_currency_code };
              //  if(!c.Contains(coin.Code))  c.Add(coin);
            }         
           
            return pairs;
        }
    }
}
