using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.Api.Vircurex
{
    [Export(typeof(IAuthApi))]
    public class AuthApi : VircurexApi, IAuthApi
    {

        public AuthKey AuthKey
        {
            get
            {
                return Setting.Singleton.GetAuthKeyDict()[Exchange.AbbrName];
            }
        }

        string securityWord = "yourword";
        string userName = "yourUsernName";
        public string AuthQuery(string method, Dictionary<string, string> postDict)
        {
            string time = DateTime.UtcNow.ToString("s");
            // string id = AuthUtility.GetNonceBySeconds().ToString();
            string id = GetVircurexSign(time + new Random().NextDouble().ToString());
            //用分号分开的字符串
            string tokenMsg = securityWord + ";" + userName + ";" + time + ";" + id + ";" + "get_balances";

            var sign = AuthUtility.GetVircurexSign(tokenMsg);
            string url = "https://vircurex.com/api/get_balances.json?account=cyj100&id=" + id + "&token=" + sign + "&timestamp=" + time;

            using (WebClient webClient = new WebClient())
            {
                string result = webClient.DownloadString(url);
                return result;
            }
        }

        //总是遇到签名不正确错误：{"status":8003,"statustxt":"Authentication failed"}

        /// <summary>
        /// 相当失望，试半天没试出正确的算法！还是我哪里搞错了！
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string GetVircurexSign(string msg)
        {
            byte[] msgBytes = Encoding.ASCII.GetBytes(msg);
            //HMACSHA256 hasher = new HMACSHA256(Encoding.ASCII.GetBytes("987654321"));
            HMACSHA256 hasher = new HMACSHA256();
            byte[] hash = hasher.ComputeHash(msgBytes);
            // string sign = Convert.ToBase64String(hash);

            string sign = BitConverter.ToString(hash).Replace("-", "").ToLower();
            // string sign = BitConverter.ToString(hash).Replace("-", "").ToUpper();
            return sign;
        }
    }
}
