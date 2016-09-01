using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.TradeModule.View;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;


namespace Eureka.CoinTrade.TradeModule
{
    [ModuleExport(typeof(TradeModule))]
    public class TradeModule : IModule
    {
        [Import]
        private TradeMainPresenter tradeMainPresenter;

        public void Initialize()
        {
            tradeMainPresenter.Initialize();
            EventSourceLogger.Logger.InitModule("TradeModule");
        }
    }
}
