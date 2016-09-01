using System.ComponentModel.Composition;
using Eureka.CoinTrade.ChartModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;


namespace Eureka.CoinTrade.ChartModule.View
{
    [Export]
    public class ChartMainPresenter : Presenter<ChartMainView, ChartMainViewModel>
    {
        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("ChartRegion", View);
        }
    }
}
