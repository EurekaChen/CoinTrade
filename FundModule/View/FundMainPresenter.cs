using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eureka.CoinTrade.FundModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Newtonsoft.Json;
using Microsoft.Practices.Prism.Regions;
using System.Collections.ObjectModel;
using Eureka.CoinTrade.ApiBase;
using Microsoft.Practices.Prism.Commands;

namespace Eureka.CoinTrade.FundModule.View
{
    [Export]
    public class FundMainPresenter:Presenter<FundMainView,FundMainViewModel>
    {           
        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("FundRegion",View);     
        }       
    }
}
