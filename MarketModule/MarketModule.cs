using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.MarketModule.View;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;


namespace Eureka.CoinTrade.MarketModule
{
    [ModuleExport(typeof(MarketModule))]
    public class MarketModule : IModule
    {
        [Import]
        private MarketMainPresenter marketMainPresenter;

        public void Initialize()
        {
            marketMainPresenter.Initialize();
            EventSourceLogger.Logger.InitModule("MarketModule");
        }
    }
}
