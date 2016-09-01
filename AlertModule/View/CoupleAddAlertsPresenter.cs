using System.ComponentModel.Composition;
using Eureka.CoinTrade.AlertModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.AlertModule.View
{
    [Export]
    public class CoupleAddAlertPresenter : Presenter<CoupleAddAlertView, CoupleAddAlertViewModel>
    {

        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("CoupleAddAlertRegion", View);
        }
    }
}
