using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Eureka.CoinTrade.Infrastructure
{
    public static class AuthUtility
    {
        /// <summary>
        /// 返回一个按Ticks不断增长的整数字符串。
        /// </summary>
        /// <returns></returns>
        public static string GetNonceByTicks()
        {
            DateTime begin = new DateTime(2013, 10, 28);
            long nonce = DateTime.Now.Ticks - begin.Ticks;
            return nonce.ToString();
        }

        /// <summary>
        /// 返回一个按秒不断增长的整数字符串。
        /// </summary>
        /// <returns></returns>
        public static string GetNonceBySeconds()
        {
            DateTime begin = new DateTime(2013, 10, 28);
            long nonce = (int)(DateTime.Now - begin).TotalSeconds;
            return nonce.ToString();
        }

        public static string GetResponseJson(HttpWebRequest webRequest)
        {
            using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
            {
                Stream resultStream = webResponse.GetResponseStream();
                string jsonData = new StreamReader(resultStream).ReadToEnd();
                return jsonData;
            }
        }

        /// <summary>
        /// 将参数字典转换为Post文本。
        /// </summary>
        /// <param name="postDict"></param>
        /// <returns></returns>
        public static string ToPost(Dictionary<string, string> postDict, bool isUrlEncode)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in postDict)
            {
                string itemValue = isUrlEncode ? HttpUtility.UrlEncode(item.Value) : item.Value;
                sb.AppendFormat("{0}={1}", item.Key, itemValue);
                sb.Append("&");
            }
            if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
            return sb.ToString(); ;
        }


        /// <summary>
        /// 对数据进行加密签名。
        /// </summary>
        /// <param name="data">要签名的数据格，已经由文本转换为字节</param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        /// <remarks>
        /// 以下API共用该签名函数:
        /// Bter
        /// Btc-e
        /// Cryptsy 
        /// </remarks>
        public static string GetSign(string post, string privateKey)
        {
            byte[] data = Encoding.ASCII.GetBytes(post);
            byte[] key = Encoding.ASCII.GetBytes(privateKey);

            HMACSHA512 hasher = new HMACSHA512(key);
            byte[] hash = hasher.ComputeHash(data);

            string hex = BytesToHex(hash);
            string sign = hex.ToLower();
            return sign;
        }

        private static string BytesToHex(byte[] bytes)
        {
            string hexText = BitConverter.ToString(bytes);
            string hex = hexText.Replace("-", "");
            return hex;
        }
    }
}
