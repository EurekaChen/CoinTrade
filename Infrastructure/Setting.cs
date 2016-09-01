using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;

namespace Eureka.CoinTrade.Infrastructure
{
    //说明：本来想导出，结果各API的初始化需要注入到构造函数，这样各API的子类也需要注入！因此改成单件模式。
    //[Export]
    //[PartCreationPolicy(CreationPolicy.Shared)]
    public class Setting
    {
        private static Setting sigleton;
        public static Setting Singleton
        {
            get
            {
                if (sigleton == null)
                {
                    sigleton = new Setting();
                }
                return sigleton;
            }
        }
        private Setting() { Option = Option.Load(); }

        public Option Option { get; private set; }

        private static Dictionary<string, AuthKey> authKeyDict;       

        public Dictionary<string, AuthKey> GetAuthKeyDict()
        {
            //缓存
            if (authKeyDict != null) return authKeyDict;
            //获取所有AuthApi
            Dictionary<string, AuthKey> allKeys = new Dictionary<string, AuthKey>();
            ApiManager apiManager = ServiceLocator.Current.GetInstance<ApiManager>();
            foreach (IAuthApi authApi in apiManager.AuthApis)
            {
                ExchangeApi api = authApi as ExchangeApi;
                allKeys.Add(api.Exchange.AbbrName, new AuthKey());
            }
            //已经设置则获取
            if (File.Exists("Api.key"))
            {
                string authKeyJson = Setting.ReadJsonFromCompressFile("Api.key");
                Dictionary<string, AuthKey> authKeys = JsonConvert.DeserializeObject<Dictionary<string, AuthKey>>(authKeyJson);
                foreach (var authKey in authKeys)
                {
                    allKeys[authKey.Key] = authKey.Value;
                }
                return allKeys;
            }
            //未设置则生成空的
            else
            {
                string json = JsonConvert.SerializeObject(allKeys);
                Setting.WriteJsonToCompressFile("Api.key", json);
            }
            authKeyDict = allKeys;
            return authKeyDict;
        }


        private static Dictionary<string, CurrencyPairs> exchangeSelectedPairs;

        public Dictionary<string, CurrencyPairs> ExchangeSelectedPairs
        {
            get
            {
                if (exchangeSelectedPairs == null)
                {
                    exchangeSelectedPairs = new Dictionary<string, CurrencyPairs>();

                    string selectedPairsJson = ReadJsonFromCompressFile("SelectedPairs.key");
                    Dictionary<string, Collection<string>> selectedPairsDict = JsonConvert.DeserializeObject<Dictionary<string, Collection<string>>>(selectedPairsJson);

                    foreach (var item in selectedPairsDict)
                    {
                        CurrencyPairs currencyPairs = new CurrencyPairs();
                        foreach (var pair in item.Value)
                        {
                            currencyPairs.Add(CurrencyPair.All[pair]);
                        }
                        exchangeSelectedPairs.Add(item.Key, currencyPairs);
                    }
                }
                return exchangeSelectedPairs;
            }
        }
        public CurrencyPairs GetSelectedPairs(string exchangeAbbrName)
        {           

            if (ExchangeSelectedPairs.ContainsKey(exchangeAbbrName))
            {
                return ExchangeSelectedPairs[exchangeAbbrName];
            }
            else
            {
                return new CurrencyPairs();
            }
        }

       

        public static string ReadJsonFromCompressFile(string compressFileName)
        {
            string path = System.Environment.CurrentDirectory;
            string fullFilename = path + @"\" + compressFileName;
            FileInfo compressFileInfo = new FileInfo(fullFilename);
            return Decompress(compressFileInfo);
        }

        public static void WriteJsonToCompressFile(string filenNme, string json)
        {
            string path = System.Environment.CurrentDirectory;
            string fullFileName = path + @"\" + filenNme;
            Compress(json, fullFileName);
        }


        public static void Compress(string content, string compressFileName)
        {
            byte[] data = Encoding.UTF8.GetBytes(content);
            using (MemoryStream originalFileStream = new MemoryStream(data))
            {
                using (FileStream compressedFileStream = File.Create(compressFileName))
                {
                    using (DeflateStream compressionStream = new DeflateStream(compressedFileStream, CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        public static string Decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (DeflateStream decompressionStream = new DeflateStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(memoryStream);
                        byte[] bytes = memoryStream.ToArray();
                        string content = Encoding.UTF8.GetString(bytes);
                        return content;
                    }
                }
            }
        }
    }
}
