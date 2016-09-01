using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Text;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.Api.Btce
{
    [Export(typeof(IAuthApi))]
    public class AuthApi : BtceApi, IAuthApi
    {
        public AuthKey AuthKey
        {
            get
            {
                return Setting.Singleton.GetAuthKeyDict()[Exchange.AbbrName];
            }
        }

        public string AuthQuery(string method)
        {
            return AuthQuery(method, new Dictionary<string, string>());
        }
        public string AuthQuery(string method, Dictionary<string, string> postDict)
        {
            //通过Tick获得的整数对Btce来说可能太大了！          
            string nonce = AuthUtility.GetNonceBySeconds();
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
            HttpWebRequest webRequest = WebRequest.Create(new Uri("https://btc-e.com/tapi")) as HttpWebRequest;
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
