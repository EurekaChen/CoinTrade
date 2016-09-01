using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.StatusModule.View;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;


namespace Eureka.CoinTrade.StatusModule
{
    [ModuleExport(typeof(StatusModule))]
    public class StatusModule : IModule
    {
        [Import]
        private StatusMainPresenter statusMainPresenter;

        public void Initialize()
        {
            statusMainPresenter.Initialize();
            EventSourceLogger.Logger.InitModule("StatusModule");
        }
    }
}
