using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Text;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.Api.CryptoTrade
{
    [Export(typeof(IAuthApi))]
    public class AuthApi : CryptoTradeApi, IAuthApi
    {
        public AuthKey AuthKey
        {
            get
            {
                return Setting.Singleton.GetAuthKeyDict()[Exchange.AbbrName];
            }
        }


        /// <summary>
        /// Bter 自动交易API
        /// </summary>
        /// <param name="path"><example>1/private/getfunds</example></param>
        /// <param name="postDict"></param>
        /// <returns></returns>
        public string AuthQuery(string path, Dictionary<string, string> postDict)
        {
            string nonce = AuthUtility.GetNonceByTicks();
            postDict.Add("nonce", nonce);
            string post = AuthUtility.ToPost(postDict, true);

            var sign = AuthUtility.GetSign(post, AuthKey.PrivateKey);

            byte[] data = Encoding.ASCII.GetBytes(post);
            HttpWebRequest webRequest = GetWebRequest(data, path, sign);

            string json = AuthUtility.GetResponseJson(webRequest);
            return json;

        }

        private HttpWebRequest GetWebRequest(byte[] data, string path, string sign)
        {
            HttpWebRequest webRequest = WebRequest.Create(new Uri("https://crypto-trade.com/api/" + path)) as HttpWebRequest;

            webRequest.Method = "POST";
            webRequest.Timeout = Timeout;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = data.Length;

            webRequest.Headers.Add("KEY", AuthKey.PublicKey);
            webRequest.Headers.Add("SIGN", sign);

            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }
            return webRequest;
        }
    }
}
