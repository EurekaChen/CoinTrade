using System.ComponentModel.Composition;
using Eureka.CoinTrade.AlertModule.View;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Eureka.CoinTrade.AlertModule
{
    [ModuleExport(typeof(AlertModule))]
    public class AlertModule : IModule
    {
        [Import]
        private AlertMainPresenter marketMainPresenter;

        public void Initialize()
        {
            marketMainPresenter.Initialize();
            EventSourceLogger.Logger.InitModule("AlertModule");
        }
    }
}
