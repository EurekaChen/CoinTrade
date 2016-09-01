using System.ComponentModel.Composition;
using Eureka.CoinTrade.AlertModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.AlertModule.View
{
    [Export]
    public class SinglePresenter : Presenter<SingleView, SingleViewModel>
    {
        [Import]
        SingleAddAlertPresenter singleAddAlertPresenter;

        [Import]
        SingleAlertsPresenter singleAlertsPresenter;
        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("SingleRegion", View);

            singleAddAlertPresenter.Initialize();
            singleAlertsPresenter.Initialize();

        }
    }
}
