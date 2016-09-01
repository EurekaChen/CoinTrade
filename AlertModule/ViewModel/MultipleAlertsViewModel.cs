using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
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
    public class MultipleAlertsViewModel : BindableBase
    {
        [Import]
        ApiManager apiManager;

        [ImportingConstructor]
        public MultipleAlertsViewModel(IEventAggregator eventAggregator)
        {
            if (File.Exists("MultipleAlerts.Key"))
            {
                string json = Setting.ReadJsonFromCompressFile("MultipleAlerts.Key");
                Alerts = JsonConvert.DeserializeObject<ObservableCollection<MultipleAlertItem>>(json);
                foreach (var item in Alerts)
                {
                    item.State = MonitorState.Stop;
                    item.BackgroundColor = Colors.LightGray;
                }
            }
            else
            {
                Alerts = new ObservableCollection<MultipleAlertItem>();
            }

            eventAggregator.GetEvent<AddMultipleAlertItemEvent>().Subscribe(AddItem);

            WatchCommand = new DelegateCommand<bool?>(Watch);
            DeleteItemCommand = new DelegateCommand<MultipleAlertItem>(DeleteItem);
            SaveCommand = new DelegateCommand(Save);
            checkTimer = new DispatcherTimer();
            checkTimer.Interval = TimeSpan.FromSeconds(UpdateInterval);
            checkTimer.Tick += TimerOnTick;
        }

        private void AddItem(MultipleAlertItem item)
        {
            Alerts.Add(item);
        }


        #region Alerts
        private ObservableCollection<MultipleAlertItem> alerts;
        public ObservableCollection<MultipleAlertItem> Alerts
        {
            get { return alerts; }
            set { SetProperty(ref alerts, value); }
        }
        #endregion


        public DelegateCommand<bool?> WatchCommand { get; set; }

        public DelegateCommand<MultipleAlertItem> DeleteItemCommand { get; set; }

        public DelegateCommand SaveCommand { get; set; }


        private void Save()
        {
            string json = JsonConvert.SerializeObject(Alerts);
            Setting.WriteJsonToCompressFile("MultipleAlerts.Key", json);
        }

        private void DeleteItem(MultipleAlertItem alertItem)
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
            foreach (var alert in Alerts)
            {

                foreach (var item in alert.AbbrNamePrices)
                {
                    ITickerApi tickerApi = apiManager.GetApi<ITickerApi>(item.AbbrName);
                    ExchangeApi exchangeApi = apiManager.GetApi<ExchangeApi>(item.AbbrName);
                    Ticker ticker = exchangeApi.SelectedTickers[alert.PairCode];
                    tickerApi.UpdateTicker(ref ticker);
                    decimal price = Utility.GetPrice(alert.PriceType, ticker);
                    item.Price = price;
                }

                if (!alert.IsOpen)
                {
                    alert.State = MonitorState.Pause;
                    alert.BackgroundColor = Colors.Transparent;
                    continue;
                }


                if (alert.IsHighThan)
                {
                    var prices = from priceItem in alert.AbbrNamePrices where priceItem.Price != 0 select priceItem.Price;
                    alert.Price = prices.Max();
                    if (alert.Price > alert.ComparePrice)
                    {
                        alert.State = MonitorState.Triggered;
                        alert.BackgroundColor = alert.AlertColor;
                        PlaySound(alert);
                    }
                    else
                    {
                        alert.State = MonitorState.NotTriggered;
                        alert.BackgroundColor = Colors.Transparent;
                    }
                }
                else
                {
                    var prices = from priceItem in alert.AbbrNamePrices where priceItem.Price != 0 select priceItem.Price;
                    alert.Price = prices.Min();
                    if (alert.Price < alert.ComparePrice)
                    {
                        alert.State = MonitorState.Triggered;
                        alert.BackgroundColor = alert.AlertColor;
                        PlaySound(alert);
                    }
                    else
                    {
                        alert.State = MonitorState.NotTriggered;
                        alert.BackgroundColor = Colors.Transparent;
                    }
                }
            }
        }

        private static void PlaySound(MultipleAlertItem alert)
        {
            if (alert.IsPlaySound)
            {
                MediaPlayer player = new MediaPlayer();
                Uri uri = new Uri("Media\\" + alert.SoundFile, UriKind.Relative);
                player.Open(uri);
                player.Play();
            }
        }
    }
}
