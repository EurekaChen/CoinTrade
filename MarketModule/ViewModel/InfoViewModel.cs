
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Commands;
namespace Eureka.CoinTrade.MarketModule.ViewModel
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class InfoViewModel : BindableBase
    {
        private InfoViewModel()
        {
            LocalCode = Setting.Singleton.Option.LocalCurrencyCode;
            if (LocalCode != "USD")
            {
                if (LocalCode == "RUR") { LocalCode = "RUB"; }

                IsShowUsdLocal = true;
                string usdLocalPair = "USD" + LocalCode;
                YahooUsdLocalRate = new ExchangeRate(CurrencyPair.All[usdLocalPair], 0.0m);
                OerUsdLocalRate = new ExchangeRate(CurrencyPair.All[usdLocalPair], 0.0m);
            }

            BpiBtcUsdRate = new ExchangeRate(CurrencyPair.All["BTCUSD"], 0.0m);
            string btcLocalPair = "BTC" + LocalCode;
            BpiBtcLocalRate = new ExchangeRate(CurrencyPair.All[btcLocalPair], 0.0m);

            UpdateCommand = new DelegateCommand(Update);
        }


        #region LocalCode
        private string localCode;
        public string LocalCode
        {
            get { return localCode; }
            set { SetProperty(ref localCode, value); }
        }
        #endregion


        [Import("Coindesk", typeof(IExchangeRateApi))]
        private IExchangeRateApi CoindeskApi;
        public ExchangeRate BpiBtcUsdRate { get; set; }
        public ExchangeRate BpiBtcLocalRate { get; set; }
        [Import("Yahoo", typeof(IExchangeRateApi))]

        private IExchangeRateApi YahooApi;
        public ExchangeRate YahooUsdLocalRate { get; set; }

        [Import("OpenExchangeRate", typeof(IExchangeRateApi))]
        private IExchangeRateApi OerApi;
        public ExchangeRate OerUsdLocalRate { get; set; }

        public DelegateCommand UpdateCommand { get; set; }


        #region IsShowUsdLocal
        private bool isShowUsdLocal = false;
        public bool IsShowUsdLocal
        {
            get { return isShowUsdLocal; }
            set { SetProperty(ref isShowUsdLocal, value); }
        }
        #endregion


        public void Update()
        {
            CoindeskApi.Update(BpiBtcUsdRate);
            CoindeskApi.Update(BpiBtcLocalRate);
            if (IsShowUsdLocal)
            {
                YahooApi.Update(YahooUsdLocalRate);
                OerApi.Update(OerUsdLocalRate);
            }
        }

    }
}
