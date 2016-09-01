
namespace Eureka.CoinTrade.Base
{
    public class ExchangeRate : BindableBase
    {
        public ExchangeRate(CurrencyPair pair, decimal rate)
        {
            Pair = pair;
            Rate = rate;
        }
        public CurrencyPair Pair { get; private set; }

        private decimal rate; 
        public decimal Rate 
        {
            get
            {
                return rate;
            }
            set
            {                
                SetProperty(ref rate,value);
            }
        } 

    }
}
