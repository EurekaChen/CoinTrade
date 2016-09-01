using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.MarketModule.ViewModel;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.MarketModule.View
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class QuotationPresenter : Presenter<QuotationView, QuotationViewModel>
    {
        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("QuotationRegion", View);
        }
    }
}
