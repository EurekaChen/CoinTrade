using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Text;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.Api.Cryptsy
{
    [Export(typeof(IAuthApi))]
    public class AuthApi : CryptsyApi, IAuthApi
    {

        public AuthKey AuthKey
        {
            get
            {
                var authKeys = Setting.Singleton.GetAuthKeyDict();
                var authKey = authKeys[Exchange.AbbrName];
                return authKey;
            }
        }

        public string AuthQuery(string method, Dictionary<string, string> postDict)
        {
            string nonce = AuthUtility.GetNonceByTicks();
            postDict.Add("nonce", nonce);
            postDict.Add("method", method);
            string post = AuthUtility.ToPost(postDict, true);
            var sign = AuthUtility.GetSign(post, AuthKey.PrivateKey);

            byte[] data = Encoding.ASCII.GetBytes(post);
            HttpWebRequest webRequest = GetWebRequest(data, sign);

            string json = AuthUtility.GetResponseJson(webRequest);
            return json;
        }

        private HttpWebRequest GetWebRequest(byte[] data, string sign)
        {
            HttpWebRequest webRequest = WebRequest.Create(new Uri("https://www.cryptsy.com/api ")) as HttpWebRequest;

            webRequest.Method = "POST";
            webRequest.Timeout = Timeout;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = data.Length;

            webRequest.Headers.Add("Key", AuthKey.PublicKey);
            webRequest.Headers.Add("Sign", sign);

            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            return webRequest;
        }
    }
}
