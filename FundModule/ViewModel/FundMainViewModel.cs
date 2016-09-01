using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Properties;
using Microsoft.Practices.Prism.Commands;

namespace Eureka.CoinTrade.FundModule.ViewModel
{
    [Export]
    public class FundMainViewModel : BindableBase
    {
        //2013-11-02 关于绑定TabControl和TabItem的问题！！
        //一开始发现怎么也绑定不了. 在ItemTemplate上进行绑定时只能绑定到头部，即Header部分。
        //后来找到一个绑定示例进行研究：http://msdn.microsoft.com/zh-cn/library/ms771719(v=vs.90).aspx
        //的经过长时间的研究后！才发现既然是Itemplate专门绑定到Header，而内容（Content）则绑定到ContentTemplate上！这跟ListBox有些不一样！
        //绑定后又发现点击内容没有变化！原来在绑定在不能再在ItemTemplate中使用TabItem项！直接绑定即可，也就是Xaml中根本不需要出现TabItem标签！
        //如果使用了TabItem，则点击时已经不是本TabControl了，所以会打不开另一Item的内容。导至看上去内容没变。
        //我是在旁边点击时发现有变化才知道的。


        private ApiManager apiManager;

        [ImportingConstructor]
        public FundMainViewModel(ApiManager apiManager)
        {
            this.apiManager = apiManager;
            RefreshAllCommand = new DelegateCommand(RefreshAll);
            if (Setting.Singleton.Option.IsAutoFund)
            {
                RefreshAll();
            }
        }
        public ObservableCollection<ExchangeFund> funds;
        public ObservableCollection<ExchangeFund> Funds
        {
            get { return funds; }
            set
            {
                SetProperty(ref  funds, value);
            }
        }

        public DelegateCommand RefreshAllCommand { get; set; }

        private void RefreshAll()
        {
            ObservableCollection<ExchangeFund> funds = new ObservableCollection<ExchangeFund>();
            foreach (IFundApi fundApi in apiManager.FundApis)
            {
                ExchangeApi api = fundApi as ExchangeApi;
                string exchangeAbbrName = api.Exchange.AbbrName;
                string functionInfo = Resources.QueryFund;
                if (Setting.Singleton.GetSelectedPairs(exchangeAbbrName).Count == 0)
                {
                    EventSourceLogger.Logger.NoPairSelected(exchangeAbbrName, functionInfo);
                    continue;
                }

                Dictionary<string, AuthKey> authKeyDict = Setting.Singleton.GetAuthKeyDict();
                if (!authKeyDict[exchangeAbbrName].IsExist)
                {
                    EventSourceLogger.Logger.ApiKeyNotExist(exchangeAbbrName, functionInfo);
                    continue;
                }

                try
                {
                    ExchangeFund exchangeFund = fundApi.QueryFund();
                    funds.Add(exchangeFund);
                    ExchangeApi exchangeApi = fundApi as ExchangeApi;
                    EventSourceLogger.Logger.QueryDataSuccess(functionInfo, exchangeApi.Exchange.Name);
                }
                catch (Exception exception)
                {
                    EventSourceLogger.Logger.QueryDataException(functionInfo, exchangeAbbrName, exception.Message);
                }
            }
            Funds = funds;
        }
    }
}
