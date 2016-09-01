using Eureka.CoinTrade.Base;

namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    public class AbbrNameSelector : BindableBase
    {
        public string AbbrName { get; set; }

        #region Price
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }
        #endregion
    }
}
