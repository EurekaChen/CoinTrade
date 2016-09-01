using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.Api.MtGox
{
    [Export(typeof(IAuthApi))]
    public class AuthApi : MtGoxApi, IAuthApi
    {
        public AuthKey AuthKey
        {
            get
            {
                return Setting.Singleton.GetAuthKeyDict()[Exchange.AbbrName];
            }
        }
        public string AuthQuery(string path)
        {
            return AuthQuery(path, new Dictionary<string, string>());
        }

        public string AuthQuery(string path, Dictionary<string, string> postDict)
        {
            string nonce = AuthUtility.GetNonceByTicks();
            postDict.Add("nonce", nonce);

            string post = AuthUtility.ToPost(postDict, true);
            var signText = path + Convert.ToChar(0) + post;
            string sign = GetSign(signText, AuthKey.PrivateKey);

            HttpWebRequest webRequest = GetWebRequest(post, path, sign);
            string json = AuthUtility.GetResponseJson(webRequest);
            return json;
        }

        public static string GetSign(string signMessage, string privateKey)
        {
            //注：MtGox使用了UTF8！
            var signBytes = Encoding.UTF8.GetBytes(signMessage);
            byte[] key = Convert.FromBase64String(privateKey);
            HMACSHA512 hasher = new HMACSHA512(key);
            byte[] hash = hasher.ComputeHash(signBytes);
            string sign = Convert.ToBase64String(hash);
            return sign;
        }

        private HttpWebRequest GetWebRequest(string post, string path, string sign)
        {
            byte[] data = Encoding.ASCII.GetBytes(post);
            HttpWebRequest webRequest = WebRequest.Create(new Uri("https://data.mtgox.com/api/2/" + path)) as HttpWebRequest;           
            webRequest.Method = "POST";
            webRequest.Timeout = Timeout;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = data.Length;

            webRequest.Headers.Add("Rest-Key", AuthKey.PublicKey);
            webRequest.Headers.Add("Rest-Sign", sign);

            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }
            return webRequest;

        }
    }
}
