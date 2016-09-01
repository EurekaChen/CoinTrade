using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Properties;
using Microsoft.Practices.Prism.Commands;

namespace Eureka.CoinTrade.TradeModule.ViewModel
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class TradeTabViewModel : BindableBase
    {

        private ApiManager apiManager;
        public ISubmitOrderApi TradeApi { get; private set; }


        [ImportingConstructor]
        public TradeTabViewModel(ApiManager apiManager)
        {
            this.apiManager = apiManager;
        }

        public Exchange Exchange { get; private set; }
        public void Init(string exchangeAbbrName)
        {

            this.Exchange = apiManager.GetAllExchange()[exchangeAbbrName];
            ExchangeApi api = apiManager.GetApi<ExchangeApi>(exchangeAbbrName);
            Tickers = api.SelectedTickers;

            TradeApi = apiManager.GetApi<ISubmitOrderApi>(exchangeAbbrName);

            OpenTradeCommand = new DelegateCommand<ListBox>(OpenTrade);
            RefreshCommand = new DelegateCommand<Ticker>(Refresh);
            BestPriceCommand = new DelegateCommand<string>(BestPrice);
            SubmitOrderCommand = new DelegateCommand<string>(SubmitOrder);
            CurrentQuotation = new Quotation();

            //传到ListBox的Tag,再通过Tag取到按钮处！
            CancelOrderCommand = new DelegateCommand<int?>(CancelOrder);

        }

        public DelegateCommand<int?> CancelOrderCommand { get; set; }
        private void CancelOrder(int? orderId)
        {

            try
            {
                ICancelOrderApi cancelOrder = apiManager.GetApi<ICancelOrderApi>(Exchange.AbbrName);
                OrderResult result = cancelOrder.CancelOrder(orderId ?? 0);
                if (OrderResult.IsSuccess == true)
                {
                    EventSourceLogger.Logger.CancelOrderSuccess(Exchange.Name, orderId.ToString());
                    UpdateOpenOrders(CurrentTicker);
                }
                else
                {
                    EventSourceLogger.Logger.CancelOrderFail(Exchange.Name, orderId.ToString(), result.Info);
                }
            }
            catch (Exception ex)
            {
                EventSourceLogger.Logger.CancelOrderException(Exchange.Name, orderId.ToString(), ex.Message);
            }

        }

        #region 属性
        public TickersByCurrencyPair Tickers { get; set; }

        private Ticker currentTicker;
        public Ticker CurrentTicker
        {
            get
            {
                return currentTicker;
            }
            set
            {
                SetProperty(ref currentTicker, value);
            }
        }

        private bool isTradeVisible;
        public bool IsTradeVisible
        {
            get
            {
                return isTradeVisible;
            }
            set
            {
                SetProperty(ref isTradeVisible, value);
            }
        }

        private Quotation currentQuotation;
        public Quotation CurrentQuotation
        {
            get
            {
                return currentQuotation;
            }
            set
            {
                SetProperty(ref currentQuotation, value);
            }
        }

        private bool isShowOpenOrdersInfo;
        public bool IsShowOpenOrdersInfo
        {
            get
            {

                return isShowOpenOrdersInfo;
            }
            set
            {
                SetProperty(ref isShowOpenOrdersInfo, value);
            }
        }


        #region OpenOrdersInfo
        private string openOrdersInfo;
        public string OpenOrdersInfo
        {
            get { return openOrdersInfo; }
            set { SetProperty(ref openOrdersInfo, value); }
        }
        #endregion



        private ObservableCollection<Order> openOrders;
        public ObservableCollection<Order> OpenOrders
        {
            get
            {
                return openOrders;
            }
            set
            {
                SetProperty(ref openOrders, value);
            }
        }

        private decimal buyAmount;
        public decimal BuyAmount
        {
            get
            {
                return buyAmount;
            }
            set
            {
                SetProperty(ref buyAmount, value);
                OnPropertyChanged("TotalCost");
                OnPropertyChanged("BuyFee");
                OnPropertyChanged("NetCost");
            }
        }

        private decimal buyPrice;
        public decimal BuyPrice
        {
            get
            {
                return buyPrice;
            }
            set
            {
                SetProperty(ref  buyPrice, value);
                OnPropertyChanged("TotalCost");
                OnPropertyChanged("BuyFee");
                OnPropertyChanged("NetCost");
            }
        }

        public decimal TotalCost
        {
            get
            {
                return buyPrice * buyAmount;
            }
        }

        public decimal BuyFeePercentage
        {
            get
            {
                try
                {
                    return TradeApi.GetBuyFeePercentage(CurrentTicker.CurrencyPair);
                }
                catch
                {
                    return 0.0m;
                }
            }

        }

        public decimal BuyFee
        {
            get
            {
                return BuyFeePercentage * 0.01m * TotalCost;
            }
        }

        public decimal NetCost
        {
            get
            {
                return TotalCost + BuyFee;
            }
        }

        private decimal sellAmount;
        public decimal SellAmount
        {
            get
            {
                return sellAmount;
            }
            set
            {
                SetProperty(ref sellAmount, value);
                OnPropertyChanged("TotalGain");
                OnPropertyChanged("SellFee");
                OnPropertyChanged("NetGain");
            }
        }

        private decimal sellPrice;
        public decimal SellPrice
        {
            get
            {
                return sellPrice;
            }
            set
            {
                SetProperty(ref sellPrice, value);
                OnPropertyChanged("TotalGain");
                OnPropertyChanged("SellFee");
                OnPropertyChanged("NetGain");
            }
        }

        public decimal TotalGain
        {
            get
            {
                return sellPrice * sellAmount;
            }
        }


        public decimal SellFeePercentage
        {
            get
            {
                try
                {
                    return TradeApi.GetSellFeePercentage(CurrentTicker.CurrencyPair);
                }
                catch
                {
                    return 0m;
                }

            }
        }
        public decimal SellFee
        {
            get
            {

                return SellFeePercentage * 0.01m * TotalGain;

            }
        }

        public decimal NetGain
        {
            get
            {
                return TotalGain - SellFee;
            }
        }

        private OrderResult orderResult;
        public OrderResult OrderResult
        {
            get
            {
                return orderResult;
            }
            set
            {
                SetProperty(ref  orderResult, value);
            }
        }
        #endregion


        #region 命令
        public DelegateCommand<ListBox> OpenTradeCommand { get; set; }
        private void OpenTrade(ListBox listBox)
        {
            Ticker ticker = listBox.SelectedItem as Ticker;
            Refresh(ticker);

        }

        public DelegateCommand<Ticker> RefreshCommand { get; set; }

        private void Refresh(Ticker ticker)
        {
            IQuotationApi quotationUpdater = apiManager.GetApi<IQuotationApi>(Exchange.AbbrName);
            CurrentTicker = ticker;
            IsTradeVisible = true;
            CurrentQuotation.Ticker = ticker;
            CurrentQuotation.History = new History(ticker);

            var refQuotation = CurrentQuotation;
            quotationUpdater.UpdateQuotation(ref refQuotation);

            BuyAmount = 0;
            SellAmount = 0;
            BuyPrice = 0;
            SellPrice = 0;

            OnPropertyChanged("SellFeePercentage");
            OnPropertyChanged("BuyFeePercentage");

            UpdateOpenOrders(ticker);
        }


        private void UpdateOpenOrders(Ticker ticker)
        {
            IOpenOrdersApi openOrdersApi = apiManager.GetApi<IOpenOrdersApi>(Exchange.AbbrName);
            if (openOrdersApi == null)
            {
                IsShowOpenOrdersInfo = true;
                OpenOrdersInfo = Resources.NoteNotSupportOpenOrders;
            }
            else
            {
                //调用QueryOpenOrders的所有异常未经Catch,统一在这里catch。
                try
                {
                    OpenOrders = openOrdersApi.QueryOpenOrders(ticker.CurrencyPair);
                    IsShowOpenOrdersInfo = false;
                }
                catch (Exception exception)
                {

                    EventSourceLogger.Logger.QueryDataException(Resources.OpenOrders, Exchange.Name, exception.Message);
                }

            }
        }
        public DelegateCommand<string> SubmitOrderCommand { get; set; }
        private void SubmitOrder(string orderType)
        {
            ISubmitOrderApi submitOrderApi = apiManager.GetApi<ISubmitOrderApi>(Exchange.AbbrName);
            ExchangeApi exchangeApi = submitOrderApi as ExchangeApi;

            Dictionary<string, string> args = new Dictionary<string, string>();

            Order order = new Order();

            if (orderType == "buy")
            {
                order.PriceQuantity = new PriceQuantityItem() { Price = BuyPrice, Quantity = BuyAmount };
                order.OrderType = OrderType.Buy;
            }
            else if (orderType == "sell")
            {
                order.PriceQuantity = new PriceQuantityItem() { Price = SellPrice, Quantity = SellAmount };
                order.OrderType = OrderType.Sell;
            }
            try
            {
                OrderResult = submitOrderApi.SubmitOrder(CurrentTicker.CurrencyPair, order);
                if (orderResult.IsSuccess)
                {
                    EventSourceLogger.Logger.SubmitOrderSuccess(exchangeApi.Exchange.Name, orderResult.OrderId.ToString(), orderResult.Info);
                }
                else
                {
                    EventSourceLogger.Logger.SubmitOrderFail(exchangeApi.Exchange.Name, orderResult.OrderId.ToString(), orderResult.Info);
                }
            }
            catch (Exception exception)
            {
                EventSourceLogger.Logger.SubmitOrderException(Exchange.Name, exception.Message);
            }

            UpdateOpenOrders(CurrentTicker);
        }

        public DelegateCommand<string> BestPriceCommand { get; set; }
        private void BestPrice(string type)
        {
            if (type == "buy")
            {
                if (CurrentQuotation.SellOrders != null)
                {
                    BuyPrice = CurrentQuotation.SellOrders.First().Price;
                }
            }
            if (type == "sell")
            {
                if (CurrentQuotation.BuyOrders != null)
                {
                    SellPrice = CurrentQuotation.BuyOrders.First().Price;
                }
            }
        }

        #endregion

    }
}
