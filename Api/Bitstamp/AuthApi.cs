using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.Api.Bitstamp
{
    [Export(typeof(IAuthApi))]
    public class AuthApi : BitstampApi, IAuthApi
    {
        public AuthKey AuthKey
        {
            get
            {
                return Setting.Singleton.GetAuthKeyDict()[Exchange.AbbrName];
            }
        }
        public string AuthQuery(string path, Dictionary<string, string> postDict)
        {
            string nonce = AuthUtility.GetNonceByTicks();
            string clientId = Setting.Singleton.Option.BitstampClientId;

            //被签名信息为nonce+clientId+secretKey,签名后需加入到postText中： 
            string signMessage = nonce + clientId + AuthKey.PublicKey;
            string sign = GetSign(signMessage, AuthKey.PrivateKey);

            postDict.Add("nonce", nonce.ToString());
            postDict.Add("key", AuthKey.PublicKey);
            postDict.Add("signature", sign);

            string postText = AuthUtility.ToPost(postDict, true);
            byte[] postBytes = Encoding.ASCII.GetBytes(postText);

            HttpWebRequest webRequest = GetWebRequest(postBytes, path, sign);
            string json = AuthUtility.GetResponseJson(webRequest);
            return json;
        }

        private static string GetSign(string signMessage, string privateKey)
        {
            byte[] data = Encoding.ASCII.GetBytes(signMessage);
            byte[] key = Encoding.ASCII.GetBytes(privateKey);

            HMACSHA256 hasher = new HMACSHA256(key);
            byte[] hash = hasher.ComputeHash(data);

            string hex = BitConverter.ToString(hash).Replace("-", "");     //将字节转为16进制字符串。       
            return hex.ToUpper();
        }

        private HttpWebRequest GetWebRequest(byte[] data, string path, string sign)
        {
            HttpWebRequest webRequest = WebRequest.Create(new Uri("https://www.bitstamp.net/api/" + path + "/")) as HttpWebRequest;
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
