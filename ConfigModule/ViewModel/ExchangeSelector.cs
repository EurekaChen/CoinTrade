using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Microsoft.Practices.Prism.ViewModel;

namespace Eureka.CoinTrade.ConfigModule.ViewModel
{
    public class ExchangeSelector:BindableBase
    {
        private bool isSelected;
   
        public bool IsSelected 
        {
            get
            {
                return isSelected;
            }
            set
            {
                SetProperty(ref isSelected, value);               
            }
        }

        private bool isEnabled;
        public bool IsEnabled 
        {
            get
            {
                return isEnabled;
            }
            set
            {
                SetProperty(ref isEnabled, value);               
            }
        }
        public Exchange Exchange { get; set; }

    }
}
