
using Eureka.CoinTrade.Base;
namespace Eureka.CoinTrade.ApiBase
{
    public class Quotation : BindableBase
    {
        private Ticker ticker;
        public Ticker Ticker
        {
            get { return ticker; }
            set { SetProperty(ref ticker, value); }
        }


        #region History
        private History history;
        public History History
        {
            get { return history; }
            set { SetProperty(ref history, value); }
        }
        #endregion


        private PriceQuantityCollection buyOrders;
        public PriceQuantityCollection BuyOrders
        {
            get { return buyOrders; }
            set { SetProperty(ref buyOrders, value); }
        }

        private PriceQuantityCollection sellOrders;
        public PriceQuantityCollection SellOrders
        {
            get { return sellOrders; }
            set { SetProperty(ref sellOrders, value); }
        }


    }
}
