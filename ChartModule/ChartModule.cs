using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.ChartModule.View;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;


namespace Eureka.CoinTrade.ChartModule
{
    [ModuleExport(typeof(ChartModule))]
    public class ChartModule : IModule
    {
        [Import]
        private ChartMainPresenter chartMainPresenter;

        public void Initialize()
        {
            chartMainPresenter.Initialize();
            EventSourceLogger.Logger.InitModule("ChartModule");
        }
    }
}
