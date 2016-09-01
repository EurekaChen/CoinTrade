using System.ComponentModel.Composition;
using Eureka.CoinTrade.ConfigModule.View;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.ConfigModule
{
    [ModuleExport(typeof(IModule))]
    public class ConfigModule : IModule
    {
        [Import]
        private IRegionManager regionManager;

        [Import]
        private ConfigMainPresenter configMainPresenter;

        public void Initialize()
        {
            configMainPresenter.Initialize();
            EventSourceLogger.Logger.InitModule("ConfigModule");
        }
    }
}
