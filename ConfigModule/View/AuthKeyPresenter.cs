using System.ComponentModel.Composition;
using Eureka.CoinTrade.ConfigModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.ConfigModule.View
{
    [Export]
    public class AuthKeyPresenter : Presenter<AuthKeyView, AuthKeyViewModel>
    {
        [Import]
        ApiManager apiManager;
        public override void Initialize()
        {
            base.Initialize();

            regionManager.AddToRegion("AuthKeyRegion", View);
        }


    }
}
