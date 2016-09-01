using System.ComponentModel.Composition;
using Eureka.CoinTrade.AlertModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.AlertModule.View
{
    [Export]
    public class MultiplePresenter : Presenter<MultipleView, MultipleViewModel>
    {
        [Import]
        MultipleAddAlertPresenter multipleAddAlertPresenter;

        [Import]
        MultipleAlertsPresenter multipleAlertsPresenter;

        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("MultipleRegion", View);

            multipleAddAlertPresenter.Initialize();
            multipleAlertsPresenter.Initialize();

        }
    }
}
