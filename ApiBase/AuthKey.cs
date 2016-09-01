using Eureka.CoinTrade.Base;
namespace Eureka.CoinTrade.ApiBase
{
    public class AuthKey : BindableBase
    {

        private string publicKey;
        public string PublicKey
        {
            get
            {
                return publicKey;
            }
            set
            {
                SetProperty(ref publicKey, value);
            }
        }

        private string privateKey;
        public string PrivateKey
        {
            get
            {
                return privateKey;
            }
            set
            {
                SetProperty(ref privateKey, value);
            }
        }

        public bool IsExist
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PublicKey) || string.IsNullOrWhiteSpace(privateKey))
                {
                    return false;
                }
                return true;
            }
        }
    }
}
