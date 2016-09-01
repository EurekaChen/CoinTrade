using System.Windows.Media;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure.Properties;

namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    public class CoupleAlertItem : BindableBase
    {
        public string PairCode { get; set; }
        public string ConvertQuoteCode { get; set; }
        public string Exchange1AbbrName { get; set; }
        public string Exchange2AbbrName { get; set; }
        public PriceType PriceType1 { get; set; }
        public PriceType PriceType2 { get; set; }


        #region Price1
        private decimal price1;
        public decimal Price1
        {
            get { return price1; }
            set
            {
                SetProperty(ref price1, value);
                OnPropertyChanged("CurrentDifference");
            }
        }
        #endregion


        #region Price2
        private decimal price2;
        public decimal Price2
        {
            get { return price2; }
            set
            {
                SetProperty(ref price2, value);
                OnPropertyChanged("CurrentDifferecne");
            }
        }
        #endregion


        #region BackgroundColor
        private Color backgroundColor;
        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { SetProperty(ref backgroundColor, value); }
        }
        #endregion


        public decimal CurrentDifference { get { return Price1 - Price2; } }
        public decimal PriceDifference { get; set; }
        public Color AlertColor { get; set; }

        public bool IsPlaySound { get; set; }

        public string SoundFile { get; set; }


        #region State
        private MonitorState state;
        public MonitorState State
        {
            get { return state; }
            set { SetProperty(ref state, value); }
        }
        #endregion

        #region IsOpen
        private bool isOpen;
        public bool IsOpen
        {
            get
            {
                return isOpen;
            }
            set
            {
                SetProperty(ref isOpen, value);
                OnPropertyChanged("ToggleText");
            }
        }
        #endregion

        public string ToggleText
        {
            get
            {
                if (IsOpen)
                {
                    return Resources.PauseAlert;
                }
                else
                {
                    return Resources.ResumeAlert;
                }
            }
        }
    }
}
