using System.ComponentModel.Composition;
using Eureka.CoinTrade.FundModule.View;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Eureka.CoinTrade.FundModule
{
    [ModuleExport(typeof(FundModule))]
    public class FundModule:IModule
    {
        [Import]
        private FundMainPresenter fundMainPresenter;

        public void Initialize()
        {            
            fundMainPresenter.Initialize();
            EventSourceLogger.Logger.InitModule("FundModule");
        }

    }
}
