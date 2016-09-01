using System.ComponentModel.Composition;
using Eureka.CoinTrade.AlertModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.AlertModule.View
{
    [Export]
    public class CoupleAlertsPresenter : Presenter<CoupleAlertsView, CoupleAlertsViewModel>
    {

        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("CoupleAlertsRegion", View);

        }
    }
}
