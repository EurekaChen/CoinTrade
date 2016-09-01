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
    public class SingleAlertsViewModel : BindableBase
    {
        [Import]
        ApiManager apiManager;

        [ImportingConstructor]
        public SingleAlertsViewModel(IEventAggregator eventAggregator)
        {
            if (File.Exists("SingleAlerts.Key"))
            {
                string json = Setting.ReadJsonFromCompressFile("SingleAlerts.Key");
                Alerts = JsonConvert.DeserializeObject<ObservableCollection<SingleAlertItem>>(json);
                foreach (var item in Alerts)
                {
                    item.State = MonitorState.Stop;
                    item.BackgroundColor = Colors.LightGray;
                }
            }
            else
            {
                Alerts = new ObservableCollection<SingleAlertItem>();
            }

            eventAggregator.GetEvent<AddSingleAlertItemEvent>().Subscribe(AddItem);

            WatchCommand = new DelegateCommand<bool?>(Watch);
            DeleteItemCommand = new DelegateCommand<SingleAlertItem>(DeleteItem);
            SaveCommand = new DelegateCommand(Save);
            checkTimer = new DispatcherTimer();
            checkTimer.Interval = TimeSpan.FromSeconds(UpdateInterval);
            checkTimer.Tick += TimerOnTick;
        }

        private void AddItem(SingleAlertItem item)
        {
            Alerts.Add(item);
        }


        #region Alerts
        private ObservableCollection<SingleAlertItem> alerts;
        public ObservableCollection<SingleAlertItem> Alerts
        {
            get { return alerts; }
            set { SetProperty(ref alerts, value); }
        }
        #endregion


        public DelegateCommand<bool?> WatchCommand { get; set; }

        public DelegateCommand<SingleAlertItem> DeleteItemCommand { get; set; }

        public DelegateCommand SaveCommand { get; set; }


        private void Save()
        {
            string json = JsonConvert.SerializeObject(Alerts);
            Setting.WriteJsonToCompressFile("SingleAlerts.Key", json);
        }

        private void DeleteItem(SingleAlertItem alertItem)
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
                ITickerApi tickerApi = apiManager.GetApi<ITickerApi>(item.ExchangeAbbrName);
                ExchangeApi exchangeApi = apiManager.GetApi<ExchangeApi>(item.ExchangeAbbrName);
                Ticker ticker = exchangeApi.SelectedTickers[item.PairCode];
                tickerApi.UpdateTicker(ref ticker);
                item.Price = Utility.GetPrice(item.PriceType, ticker);


                if (!item.IsOpen)
                {
                    item.State = MonitorState.Pause;
                    item.BackgroundColor = Colors.Transparent;
                    continue;
                }

                if (item.Price == 0m)
                {
                    item.State = MonitorState.DataError;
                    item.BackgroundColor = Colors.Gray;
                    continue;
                }

                if (item.IsHighThan)
                {

                    if (item.Price > item.ComparePrice)
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
                else
                {
                    if (item.Price < item.ComparePrice)
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
        }


    }
}
