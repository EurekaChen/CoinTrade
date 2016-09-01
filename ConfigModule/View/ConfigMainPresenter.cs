using System.ComponentModel.Composition;
using Eureka.CoinTrade.ConfigModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.ConfigModule.View
{
    [Export]
    public class ConfigMainPresenter : Presenter<ConfigMainView, ConfigMainViewModel>
    {
        [Import]
        private ExchangePairSelectorPresenter exchangePairSelectorPresenter;

        [Import]
        private AuthKeyPresenter authKeyPresenter;

        [Import]
        private OtherOptionPresenter otherOptionPresenter;
        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("ConfigRegion", View);
            exchangePairSelectorPresenter.Initialize();
            authKeyPresenter.Initialize();
            otherOptionPresenter.Initialize();
        }
    }
}
