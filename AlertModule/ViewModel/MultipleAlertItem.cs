using System.Collections.Generic;
using System.Windows.Media;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure.Properties;

namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    public class MultipleAlertItem : BindableBase
    {
        public string PairCode { get; set; }
        public PriceType PriceType { get; set; }

        public bool IsHighThan { get; set; }

        public bool IsLowThan { get { return !IsHighThan; } }
        public decimal ComparePrice { get; set; }

        public List<AbbrNamePrice> AbbrNamePrices { get; set; }


        #region Price
        private decimal price;
        public decimal Price
        {
            get { return price; }
            set
            {
                SetProperty(ref price, value);
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
