using System.ComponentModel.Composition;
using Eureka.CoinTrade.AlertModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.AlertModule.View
{
    [Export]
    public class CouplePresenter : Presenter<CoupleView, CoupleViewModel>
    {
        [Import]
        private CoupleAddAlertPresenter coupleAddAlertPresenter;

        [Import]
        private CoupleAlertsPresenter coupleAlertsPresenter;
        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("CoupleRegion", View);

            coupleAddAlertPresenter.Initialize();
            coupleAlertsPresenter.Initialize();

        }
    }
}
