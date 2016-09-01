using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.MarketModule.ViewModel;

namespace Eureka.CoinTrade.MarketModule.View
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MarketTabPresenter : Presenter<MarketTabView, MarketTabViewModel>
    {
        public override void Initialize()
        {
            base.Initialize();
        }
    }
}
