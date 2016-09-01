using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.TradeModule.ViewModel;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;

namespace Eureka.CoinTrade.TradeModule.View
{
    [Export]
    public class TradeMainPresenter : Presenter<TradeMainView, TradeMainViewModel>
    {
        [Import]
        ApiManager apiManager;

        [Import]
        TradeTabPresenter cryptsyTradePresenter;
        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("TradeRegion", View);
            foreach (var trade in apiManager.TradableApis)
            {
                ExchangeApi tradeApi = trade as ExchangeApi;
                string exchangeAbbrName = tradeApi.Exchange.AbbrName;
                if (Setting.Singleton.GetSelectedPairs(exchangeAbbrName).Count == 0)
                {
                    EventSourceLogger.Logger.Prompt("你没有选择任何属于" + exchangeAbbrName + "的交易对，至少选择一对交易对才能进行实时交易!");
                    continue;
                }

                Dictionary<string, AuthKey> authKeyDict = Setting.Singleton.GetAuthKeyDict();
                if (!authKeyDict[exchangeAbbrName].IsExist)
                {
                    EventSourceLogger.Logger.Prompt("你没有配置于" + exchangeAbbrName + "交易所的交易API密钥，无法进行实时交易!");
                    continue;
                }
                //注意:可以通过ServiceLocator得到一个实例！注意Export必须为NoShared!   不通过ServcieLocator要经过复杂初始化，并且没有导入！
                TradeTabPresenter tradeTabPresenter = ServiceLocator.Current.GetInstance<TradeTabPresenter>();
                tradeTabPresenter.Initialize(exchangeAbbrName);
            }
        }
    }
}
