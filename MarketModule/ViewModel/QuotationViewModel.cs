
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.ApiBase;
using System.ComponentModel;
using System.Windows.Input;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Prism.Commands;
using Eureka.CoinTrade.Infrastructure;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
namespace Eureka.CoinTrade.MarketModule.ViewModel
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class QuotationViewModel : BindableBase
    { 
        private Quotation quotation;
        public Quotation Quotation
        {
            get
            {
                return quotation;
            }
            set
            {                
                SetProperty(ref quotation,value);
            }
        }   
    }
}
