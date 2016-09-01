using System.ComponentModel.Composition;
using Eureka.CoinTrade.AlertModule.ViewModel;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace Eureka.CoinTrade.AlertModule.View
{
    [Export]
    public class AlertMainPresenter : Presenter<AlertMainView, AlertMainViewModel>
    {
        [Import]
        private SinglePresenter singlePresenter;

        [Import]
        private CouplePresenter couplePresenter;

        [Import]
        private MultiplePresenter multiplePresenter;
        public override void Initialize()
        {
            base.Initialize();
            regionManager.AddToRegion("AlertRegion", View);

            singlePresenter.Initialize();
            couplePresenter.Initialize();
            multiplePresenter.Initialize();
        }
    }
}
