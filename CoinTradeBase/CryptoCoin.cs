
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Eureka.CoinTrade.Base
{
    public class CryptoCoin : Currency
    {
        private CryptoCoin() { }

        public static new Currencies All
        {
            get
            {
                if (all == null)
                {
                    all = new Currencies(); ;
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    Stream stream = assembly.GetManifestResourceStream("Eureka.CoinTrade.Base.CryptoCoinJson.txt");
                    using (TextReader textReader = new StreamReader(stream))
                    {
                        string json = textReader.ReadToEnd();
                        JArray jArray = JArray.Parse(json);
                        foreach (var item in jArray)
                        {
                            CryptoCoin cryptCoin = JsonConvert.DeserializeObject<CryptoCoin>(item.ToString());
                            all.Add(cryptCoin);
                        }
                    }
                }
                return all;
            }
        }

        private static Currencies all;
    }
}
