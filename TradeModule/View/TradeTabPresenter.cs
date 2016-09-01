using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.TradeModule.ViewModel;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.TradeModule.View
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class TradeTabPresenter : Presenter<TradeTabView, TradeTabViewModel>
    {
        public void Initialize(string exchangeAbbrName)
        {
            base.Initialize();
            ViewModel.Init(exchangeAbbrName);

            regionManager.AddToRegion("TradeTabRegion", View);
        }
    }
}
