using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.Api.BtcChina
{
	[Export(typeof(IAuthApi))]
	public class AuthApi : BtcChinaApi, IAuthApi
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

		///参见文档：http://btcchina.org/api-trade-documentation-zh
		///1. 使用如下强制的参数创建签名字符串。将键值对使用“&”符号按照下面列表的顺序连接在一起。请注意连接顺序很重要。所有的键名必须添加，但是键值可以为空 (例如 params)。
		///tonce (以毫秒为单位的时间戳，请确保您的系统时间准确)
		///accesskey (访问密匙，您可以在您的账户管理页面申请)
		///method (HTTP 请求方法，目前仅支持“post”)
		///id (JSON-RPC 请求 id)
		///method method (JSON-RPC 方法名称)
		///params (JSON-RPC 方法参数)

		public string AuthQuery(string method, Dictionary<string, string> postDict)
		{
			string tonce = GetTonce();
			Dictionary<string, string> parameters = new Dictionary<string, string>() { 
				{ "tonce", tonce },
				{ "accesskey",AuthKey.PublicKey },
				{ "requestmethod", "post" },
				{ "id", "1" },
				{ "method", method }				
			};

			string paramText = "";
			foreach (var item in postDict)
			{
				paramText += item.Value;
				paramText += ",";
			}
			paramText = paramText.Trim(',');
			parameters.Add("params", paramText);

			// bug:如果用了UrlEncode，将params中的“，”翻译掉了！会导致授权不成功！           
			string post = AuthUtility.ToPost(parameters, false);
			string sign = GetSign(post, AuthKey.PrivateKey);
			string base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes(AuthKey.PublicKey + ':' + sign));
			string postData = "{\"method\": \"" + method + "\", \"params\": [" + paramText + "], \"id\": 1}";

			HttpWebRequest webRequest = GetWebRequest(base64String, tonce, postData);
			string json = AuthUtility.GetResponseJson(webRequest);
			return json;
		}

		private static string GetTonce()
		{
			TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);
			long milliSeconds = Convert.ToInt64(timeSpan.TotalMilliseconds * 1000);
			string tonce = Convert.ToString(milliSeconds);
			return tonce;
		}

		public static string GetSign(string post, string privateKey)
		{
			byte[] data = Encoding.ASCII.GetBytes(post);
			byte[] key = Encoding.ASCII.GetBytes(privateKey);
			HMACSHA1 hasher = new HMACSHA1(key);

			MemoryStream stream = new MemoryStream(data);
			byte[] hash = hasher.ComputeHash(stream);

			StringBuilder hashStringBuilder = new StringBuilder();
			foreach (byte byteData in hash)
			{
				hashStringBuilder.Append(byteData.ToString("x2"));
			}
			// string hex = BitConverter.ToString(hash).Replace("-", "");     //将字节转为16进制字符串。 可能只是实现方式不一样。结果差不多。     
			// return hex.ToUpper();
			return hashStringBuilder.ToString();
		}

		private HttpWebRequest GetWebRequest(string base64, string tonce, string postData)
		{
			string url = "https://api.btcchina.com/api_trade_v1.php";

			//会抛出异常，到最终获取数据的地方处理
			HttpWebRequest webRequest = WebRequest.Create(new Uri(url)) as HttpWebRequest;
			byte[] data = Encoding.ASCII.GetBytes(postData);

			webRequest.Method = "POST";
			webRequest.Timeout = Timeout;
			webRequest.ContentType = "application/json-rpc";
			webRequest.ContentLength = data.Length;
			webRequest.Headers["Authorization"] = "Basic " + base64;
			webRequest.Headers["Json-Rpc-Tonce"] = tonce;

			using (Stream requestStream = webRequest.GetRequestStream())
			{
				requestStream.Write(data, 0, data.Length);
			}
			return webRequest;
		}


	}
}
