using System.ComponentModel.Composition;
using Eureka.CoinTrade.ConfigModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.ConfigModule.View
{
    [Export]
    public class ExchangePairSelectorPresenter : Presenter<ExchangePairSelectorView, ExchangePairSelectorViewModel>
    {
        public override void Initialize()
        {
            base.Initialize();

            regionManager.AddToRegion("ExchangePairSelectorRegion", View);
        }

    }
}
