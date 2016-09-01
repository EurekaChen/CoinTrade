using System.ComponentModel.Composition;
using Eureka.CoinTrade.AlertModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.AlertModule.View
{
    [Export]
    public class MultipleAlertsPresenter : Presenter<MultipleAlertsView, MultipleAlertsViewModel>
    {

        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("MultipleAlertsRegion", View);

        }
    }
}
