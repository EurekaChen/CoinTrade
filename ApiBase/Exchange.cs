

namespace Eureka.CoinTrade.ApiBase
{
    public class Exchange
    {
        public string Name { get; set; }
        public string AbbrName { get; set; }
        public string Url { get; set; }

        private string referralUrl;
        public string ReferralUrl
        {
            get
            {
                if (string.IsNullOrEmpty(referralUrl))
                {
                    return Url;
                }
                else
                {
                    return referralUrl;
                }
            }
            set
            {
                referralUrl = value;
            }
        }
        public string ChineseName { get; set; }

    }
}
