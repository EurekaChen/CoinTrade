using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Newtonsoft.Json;

namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    [Export]
    public class CoupleAlertsViewModel : BindableBase
    {
        [Import]
        ApiManager apiManager;

        [ImportingConstructor]
        public CoupleAlertsViewModel(IEventAggregator eventAggregator)
        {
            if (File.Exists("CoupleAlerts.Key"))
            {
                string json = Setting.ReadJsonFromCompressFile("CoupleAlerts.Key");
                Alerts = JsonConvert.DeserializeObject<ObservableCollection<CoupleAlertItem>>(json);
                foreach (var item in Alerts)
                {
                    item.State = MonitorState.Stop;
                    item.BackgroundColor = Colors.LightGray;
                }
            }
            else
            {
                Alerts = new ObservableCollection<CoupleAlertItem>();
            }

            eventAggregator.GetEvent<AddCoupleAlertItemEvent>().Subscribe(AddItem);

            WatchCommand = new DelegateCommand<bool?>(Watch);
            DeleteItemCommand = new DelegateCommand<CoupleAlertItem>(DeleteItem);
            SaveCommand = new DelegateCommand(Save);
            checkTimer = new DispatcherTimer();
            checkTimer.Interval = TimeSpan.FromSeconds(UpdateInterval);
            checkTimer.Tick += TimerOnTick;
        }

        private void AddItem(CoupleAlertItem item)
        {
            Alerts.Add(item);
        }


        #region Alerts
        private ObservableCollection<CoupleAlertItem> alerts;
        public ObservableCollection<CoupleAlertItem> Alerts
        {
            get { return alerts; }
            set { SetProperty(ref alerts, value); }
        }
        #endregion


        public DelegateCommand<bool?> WatchCommand { get; set; }

        public DelegateCommand<CoupleAlertItem> DeleteItemCommand { get; set; }

        public DelegateCommand SaveCommand { get; set; }


        private void Save()
        {
            string json = JsonConvert.SerializeObject(Alerts);
            Setting.WriteJsonToCompressFile("CoupleAlerts.Key", json);
        }

        private void DeleteItem(CoupleAlertItem alertItem)
        {
            Alerts.Remove(alertItem);
            Save();
        }


        #region UpdateInterval
        private int updateInterval = 2;
        public int UpdateInterval
        {
            get { return updateInterval; }
            set { SetProperty(ref updateInterval, value); }
        }
        #endregion


        private void Watch(bool? isWatch)
        {
            bool toggled = isWatch ?? false;
            if (toggled)
            {
                foreach (var item in Alerts)
                {
                    item.BackgroundColor = Colors.Transparent;
                    if (item.IsOpen)
                    {
                        item.State = MonitorState.Start;
                    }
                    else
                    {
                        item.State = MonitorState.Pause;
                    }
                }

                checkTimer.Interval = TimeSpan.FromSeconds(UpdateInterval);
                checkTimer.Start();
            }
            else
            {
                checkTimer.Stop();
                foreach (var item in Alerts)
                {
                    item.BackgroundColor = Colors.Transparent;
                    item.State = MonitorState.Stop;
                }
            }
        }

        private DispatcherTimer checkTimer;

        private void TimerOnTick(object sender, EventArgs e)
        {
            foreach (var item in Alerts)
            {
                Ticker ticker1 = GetTicker(item, item.Exchange1AbbrName);
                ITickerApi tickerApi1 = apiManager.GetApi<ITickerApi>(item.Exchange1AbbrName);
                tickerApi1.UpdateTicker(ref ticker1);
                item.Price1 = Utility.GetPrice(item.PriceType1, ticker1);

                Ticker ticker2 = GetTicker(item, item.Exchange2AbbrName);
                ITickerApi tickerApi2 = apiManager.GetApi<ITickerApi>(item.Exchange2AbbrName);
                tickerApi2.UpdateTicker(ref ticker2);
                item.Price2 = Utility.GetPrice(item.PriceType2, ticker2);

                if (!item.IsOpen)
                {
                    item.State = MonitorState.Pause;
                    item.BackgroundColor = Colors.Transparent;
                    continue;
                }

                if (item.Price1 == 0m || item.Price2 == 0m)
                {
                    item.State = MonitorState.DataError;
                    item.BackgroundColor = Colors.Gray;
                    continue;
                }

                if (item.CurrentDifference > item.PriceDifference)
                {
                    item.State = MonitorState.Triggered;
                    item.BackgroundColor = item.AlertColor;
                    if (item.IsPlaySound)
                    {
                        MediaPlayer player = new MediaPlayer();
                        Uri uri = new Uri("Media\\" + item.SoundFile, UriKind.Relative);
                        player.Open(uri);
                        player.Play();
                    }
                }
                else
                {
                    item.State = MonitorState.NotTriggered;
                    item.BackgroundColor = Colors.Transparent;
                }
            }
        }

        private Ticker GetTicker(CoupleAlertItem item, string exchangeAbbrName)
        {
            ExchangeApi exchangeApi = apiManager.GetApi<ExchangeApi>(exchangeAbbrName);
            string pairCode = item.PairCode;
            if (!exchangeApi.SelectedTickers.Contains(item.PairCode))
            {
                pairCode = CurrencyPair.All[item.PairCode].Base.Code + item.ConvertQuoteCode;
            }
            return exchangeApi.SelectedTickers[pairCode];
        }

    }
}
