using System.ComponentModel.Composition;
using Eureka.CoinTrade.AlertModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.AlertModule.View
{
    [Export]
    public class MultipleAddAlertPresenter : Presenter<MultipleAddAlertView, MultipleAddAlertViewModel>
    {

        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("MultipleAddAlertRegion", View);
        }
    }
}
