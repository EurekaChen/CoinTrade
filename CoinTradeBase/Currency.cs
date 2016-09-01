
namespace Eureka.CoinTrade.Base
{
    public class Currency
    {
        protected Currency() { }
        public string Name { get; set; }   //如果set为protected则直接Json解析是不能设置该内容。

        /// <summary>
        /// 法币为ISO三个字符的大写字母
        /// 数字币为其三字符大写名称
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 币种中文名称。
        /// </summary>
        public string ChineseName { get; set; }


        private static Currencies all;
        public static Currencies All
        {
            get
            {
                if (all == null)
                {
                    all = new Currencies();
                    foreach (var cryptoCoin in CryptoCoin.All)
                    {
                        all.Add(cryptoCoin);
                    }
                    foreach (var legalCurrency in LegalCurrency.All)
                    {
                        all.Add(legalCurrency);
                    }
                }
                return all;
            }
        }
    }
}
