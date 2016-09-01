using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;
namespace Eureka.CoinTrade.Base
{
    public class LegalCurrency : Currency
    {
        private static Currencies all;
        public new static Currencies All
        {
            get
            {
                if (all == null)
                {
                    all = new Currencies();
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    Stream stream = assembly.GetManifestResourceStream("Eureka.CoinTrade.Base.LegalCurrencyJson.txt");
                    using (TextReader textReader = new StreamReader(stream))
                    {
                        string json = textReader.ReadToEnd();
                        JArray jArray = JArray.Parse(json);
                        foreach (var item in jArray)
                        {
                            LegalCurrency legalCurrency = JsonConvert.DeserializeObject<LegalCurrency>(item.ToString());
                            all.Add(legalCurrency);
                        }
                    }
                }
                return all;
            }
        }
        private LegalCurrency() { }
    }
}
