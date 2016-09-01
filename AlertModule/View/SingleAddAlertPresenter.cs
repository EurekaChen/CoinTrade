using System.ComponentModel.Composition;
using Eureka.CoinTrade.AlertModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.AlertModule.View
{
    [Export]
    public class SingleAddAlertPresenter : Presenter<SingleAddAlertView, SingleAddAlertViewModel>
    {

        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("SingleAddAlertRegion", View);

        }
    }
}
