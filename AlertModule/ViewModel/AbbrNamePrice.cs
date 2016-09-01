using Eureka.CoinTrade.Base;

namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    public class AbbrNamePrice : BindableBase
    {
        public string AbbrName { get; set; }

        #region Price
        private decimal price;
        public decimal Price
        {
            get { return price; }
            set { SetProperty(ref price, value); }
        }
        #endregion
    }
}
