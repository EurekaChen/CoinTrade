using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.StatusModule.ViewModel;
using Microsoft.Practices.Prism.Regions;
using Eureka.CoinTrade.Infrastructure.Event;
using Eureka.SystemE.Base;

namespace Eureka.CoinTrade.StatusModule.View
{
    [Export]
    public class StatusMainPresenter:Presenter<StatusMainView,StatusMainViewModel>
    {
        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("StatusRegion", View);
           
        }
    }
}
