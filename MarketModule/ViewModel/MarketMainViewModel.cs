
using System.ComponentModel.Composition;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Commands;
namespace Eureka.CoinTrade.MarketModule.ViewModel
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MarketMainViewModel : BindableBase
    {
        [Import]
        ApiManager apiManager;

        [Import]
        private Markets markets;
        public Markets Markets
        {
            get
            {
                return markets;
            }
            set
            {
                SetProperty(ref markets, value);
            }
        }

        private MarketMainViewModel()
        {
            UpdateAllCommand = new DelegateCommand(UpdateAll);
            //构造时没有导入，移至Presenter中！
            //if (Setting.Singleton.Option.IsAutoTicker)
            //{
            //    UpdateAll();
            //}
        }

        public void UpdateAll()
        {
            markets.UpdateAll();
        }

        public DelegateCommand UpdateAllCommand { get; set; }



    }
}
