
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Event;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;

namespace Eureka.CoinTrade.MarketModule.ViewModel
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MarketTabViewModel : BindableBase
    {

        //private static LegalCurrency localCurrency = LegalCurrency.All["CNY"] as LegalCurrency;

        [Import]
        ApiManager apiManager;

        [Import]
        IEventAggregator eventAggregator;

        [Import]
        QuotationViewModel quotaionViewModel;

        public MarketTabViewModel()
        {
            LocalCurrencyCode = Setting.Singleton.Option.LocalCurrencyCode;
            //localCurrency = LegalCurrency.All[LocalCurrencyCode] as LegalCurrency;
            UpdateCommand = new DelegateCommand(Update);
            ConvertCurrencyCommand = new DelegateCommand(ConvertCurrency);
            GetRateListCommand = new DelegateCommand(GetRateList);
            RefreshTickerCommand = new DelegateCommand<Ticker>(RefreshTicker);

        }

        public void UpdateUsdLocalRate()
        {
            if (MarketTickers.Quote.Code == "USD")
            {
                string pairCode = MarketTickers.Quote.Code + LocalCurrencyCode;
                yahooUsdLocalRate = new ExchangeRate(CurrencyPair.All[pairCode], Rate);
                YahooApi.Update(yahooUsdLocalRate);
            }
        }

        [Import("Yahoo", typeof(IExchangeRateApi))]
        private IExchangeRateApi YahooApi;

        private ExchangeRate yahooUsdLocalRate;

        //[Import("OpenExchangeRate", typeof(IExchangeRateApi))]
        //private IExchangeRateApi OerApi;


        #region LocalCurrencyCode
        private string localCurrencyCode;
        public string LocalCurrencyCode
        {
            get { return localCurrencyCode; }
            set { SetProperty(ref localCurrencyCode, value); }
        }
        #endregion



        #region 属性
        private MarketTickers marketTickers;
        public MarketTickers MarketTickers
        {
            get
            {
                return marketTickers;
            }
            set
            {
                marketTickers = value;
                if (marketTickers.Quote.Code == LocalCurrencyCode)
                {
                    IsShowConvert = false;
                }
                else
                {
                    IsShowConvert = true;
                }
                SetProperty(ref marketTickers, value);
            }
        }


        private bool isShowConvert;
        public bool IsShowConvert
        {
            get
            {
                return isShowConvert;
            }
            set
            {
                SetProperty(ref isShowConvert, value);
            }
        }

        private decimal rate;
        public decimal Rate
        {
            get
            {
                return rate;
            }

            set
            {
                SetProperty(ref rate, value);
            }
        }

        private bool needConvert = false;

        public bool NeedConvert
        {
            get
            {
                return needConvert;
            }
            set
            {
                SetProperty(ref needConvert, value);
            }
        }

        private bool isShowRateList;
        public bool IsShowRateList
        {
            get
            {
                return isShowRateList;
            }
            set
            {
                SetProperty(ref isShowRateList, value);

            }
        }

        private Dictionary<string, decimal> rateList;
        public Dictionary<string, decimal> RateList
        {
            get
            {
                return rateList;
            }
            set
            {
                SetProperty(ref rateList, value);
            }
        }

        private KeyValuePair<string, decimal> selectedRate;
        public KeyValuePair<string, decimal> SelectedRate
        {
            get
            {
                return selectedRate;
            }
            set
            {
                Rate = value.Value;
                SetProperty(ref selectedRate, value);
                OnPropertyChanged("Rate");
            }
        }

        #endregion

        #region  命令

        public DelegateCommand<Ticker> RefreshTickerCommand { get; set; }

        private void RefreshTicker(Ticker ticker)
        {
            quotaionViewModel.Quotation = new Quotation();
            quotaionViewModel.Quotation.Ticker = ticker;
            quotaionViewModel.Quotation.History = new History(ticker);
            var refQuotation = quotaionViewModel.Quotation;
            IQuotationApi quotationUpdater = apiManager.GetApi<IQuotationApi>(ticker.Exchange.AbbrName);
            quotationUpdater.UpdateQuotation(ref refQuotation);
        }

        public DelegateCommand ConvertCurrencyCommand { get; set; }
        private void ConvertCurrency()
        {
            foreach (TickersByExchange tickers in marketTickers)
            {
                foreach (Ticker ticker in tickers)
                {
                    if (NeedConvert)
                    {
                        ticker.ConvertRate = Rate;
                    }
                    else
                    {
                        ticker.ConvertRate = 1;
                    }
                    var tickerUpdater = apiManager.GetApi<ITickerApi>(ticker.Exchange.AbbrName);
                    var refTicker = ticker;
                    tickerUpdater.UpdateTicker(ref refTicker);
                }
                //将消息发出去！当前供价格提醒合并美元和人民币用！
                var item = new CurrencyConvertItem();
                item.FromCode = marketTickers.Quote.Code;
                item.ToCode = LocalCurrencyCode;
                if (NeedConvert)
                {
                    item.Rate = Rate;
                }
                else
                {
                    item.Rate = 1m;
                }
                eventAggregator.GetEvent<ConvertCurrencyEvent>().Publish(item);
            }
        }

        public DelegateCommand UpdateCommand { get; set; }
        private void Update()
        {
            foreach (var exchangeTickers in MarketTickers)
            {
                UpdateExchangeTickers(exchangeTickers);
            }
        }

        private void UpdateExchangeTickers(TickersByExchange exchangeTickers)
        {
            foreach (var ticker in exchangeTickers)
            {
                ITickerApi tickerUpdater = apiManager.GetApi<ITickerApi>(ticker.Exchange.AbbrName);
                var refTicker = ticker;
                tickerUpdater.UpdateTicker(ref refTicker);
            }
        }

        public DelegateCommand GetRateListCommand { get; set; }
        private void GetRateList()
        {

            string pairCode = marketTickers.Quote.Code + LocalCurrencyCode;
            //如果是法币：
            if (marketTickers.Quote.Code == "USD")
            {
                if (LocalCurrencyCode != "USD")
                {
                    YahooApi.Update(yahooUsdLocalRate);
                    Rate = yahooUsdLocalRate.Rate;
                }
                IsShowRateList = false;
            }
            else
            {
                var tempRateList = new Dictionary<string, decimal>();
                var allApi = apiManager.AllExchangeApi;

                foreach (var api in allApi)
                {
                    if (api.SelectedTickers.Contains(pairCode))
                    {
                        tempRateList.Add(api.Exchange.Name, api.SelectedTickers[pairCode].LastTradePrice);
                    }
                }
                RateList = tempRateList;
                if (RateList.Count > 0)
                {
                    SelectedRate = RateList.First();
                }
            }
        }
        #endregion

    }
}
