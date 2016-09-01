using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.MarketModule.ViewModel;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.MarketModule.View
{
    [Export]
    public class InfoPresenter : Presenter<InfoView, InfoViewModel>
    {
        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("InfoRegion", View);

            //由于在ViewModel中使用需要在构造函数中导入过多汇率插件，所以改在这里。
            if (Setting.Singleton.Option.IsAutoRate)
            {
                ViewModel.Update();
            }
        }
    }
}
