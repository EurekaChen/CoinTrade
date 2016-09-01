
using Eureka.CoinTrade.Base;
namespace Eureka.CoinTrade.ApiBase
{
    public class Ticker : BindableBase
    {
        public Exchange Exchange { get; set; }
        public CurrencyPair CurrencyPair { get; set; }


        #region ConvertTo
        private Currency mergeTo;
        public Currency MergeTo
        {
            get { return mergeTo; }
            set { SetProperty(ref mergeTo, value); }
        }
        #endregion



        private decimal lastTradePrice;
        public decimal LastTradePrice
        {
            set
            {
                SetProperty(ref lastTradePrice, value);
            }
            get
            {
                return lastTradePrice * ConvertRate;
            }
        }


        private decimal lowPrice;
        public decimal LowPrice
        {
            set
            {
                SetProperty(ref lowPrice, value);
            }
            get
            {
                return lowPrice * ConvertRate;
            }
        }

        private decimal highPrice;
        public decimal HighPrice
        {
            set
            {
                SetProperty(ref highPrice, value);
            }
            get
            {
                return highPrice * ConvertRate;
            }
        }

        private decimal askPrice;
        public decimal AskPrice
        {
            set
            {
                SetProperty(ref askPrice, value);
            }
            get
            {
                return askPrice * ConvertRate;
            }
        }


        private decimal bidPrice;
        public decimal BidPrice
        {
            set
            {
                SetProperty(ref bidPrice, value);
            }
            get
            {
                return bidPrice * ConvertRate;
            }
        }


        private decimal volume;
        public decimal Volume
        {
            set
            {
                SetProperty(ref volume, value);
            }
            get
            {
                return volume;
            }
        }


        private decimal convertRate = 1;
        public decimal ConvertRate
        {
            get
            {
                return convertRate;
            }
            set
            {
                SetProperty(ref convertRate, value);
                OnPropertyChanged("LastTradePrice");
                OnPropertyChanged("HighPrice");
                OnPropertyChanged("LowPrice");
                OnPropertyChanged("AskPrice");
                OnPropertyChanged("BidPrice");
            }
        }
    }
}
