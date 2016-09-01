using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.MarketModule.ViewModel;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;

namespace Eureka.CoinTrade.MarketModule.View
{
    [Export]
    public class MarketMainPresenter : Presenter<MarketMainView, MarketMainViewModel>
    {
        [Import]
        QuotationPresenter quotationPresenter;

        [Import]
        InfoPresenter infoPresenter;

        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("MarketRegion", View);

            foreach (var market in ViewModel.Markets.All)
            {
                //注意:可以通过ServiceLocator得到一个实例！注意Export必须为NoShared!
                MarketTabPresenter tabPresenter = ServiceLocator.Current.GetInstance<MarketTabPresenter>();
                //不通过ServcieLocator要经过复杂初始化，并且没有导入！
                //MarketTabresenter tabPresenter = new MarketTabresenter();            
                //var tabViewModel =new MarketTabViewModel();
                //tabViewModel.MarketTickers = market;
                //tabPresenter.ViewModel = tabViewModel;          
                // tabPresenter.View = tabView;
                //有可能为空，这时不加入该市场。
                if (market.Count > 0)
                {
                    tabPresenter.ViewModel.MarketTickers = market;
                    tabPresenter.ViewModel.UpdateUsdLocalRate();
                    tabPresenter.Initialize();
                    regionManager.AddToRegion("MarketTabRegion", tabPresenter.View);
                }
            }
            quotationPresenter.Initialize();

            infoPresenter.Initialize();

            if (Setting.Singleton.Option.IsAutoTicker)
            {
                ViewModel.UpdateAll();
            }
        }
    }
}
