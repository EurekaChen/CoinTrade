using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Event;
using Eureka.CoinTrade.Infrastructure.Properties;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;

namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    [Export]
    public class CoupleAddAlertViewModel : BindableBase
    {
        [Import]
        ApiManager apiManager;

        IEventAggregator eventAggregator;

        [ImportingConstructor]
        public CoupleAddAlertViewModel(IEventAggregator eventAggregator)
        {
            AllSelectedPairs = GetAllSelectedPairs();

            SelectPairCommand = new DelegateCommand<string>(SelectPair);
            AddAlertCommand = new DelegateCommand(AddAlert);
            TestSoundCommand = new DelegateCommand<string>(TestSound);
            this.eventAggregator = eventAggregator;
            eventAggregator.GetEvent<ConvertCurrencyEvent>().Subscribe(DisplayMerge);
        }

        private void DisplayMerge(CurrencyConvertItem item)
        {
            if (Merges == null)
            {
                Merges = new ObservableCollection<CurrencyConvertItem>();
                Merges.Add(item);
                return;
            }

            var existItems = from i in Merges where i.FromCode == item.FromCode && i.ToCode == item.ToCode select i;
            if (existItems.Count() == 0)
            {
                merges.Add(item);
                return;
            }

            var theItem = existItems.FirstOrDefault();
            Merges.Remove(theItem);

            if (item.Rate != 1m)
            {
                merges.Add(item);
            }
        }

        #region Merges
        private ObservableCollection<CurrencyConvertItem> merges;
        public ObservableCollection<CurrencyConvertItem> Merges
        {
            get { return merges; }
            set { SetProperty(ref merges, value); }
        }
        #endregion

        private ObservableCollection<string> GetAllSelectedPairs()
        {
            ObservableCollection<string> allPairs = new ObservableCollection<string>();

            foreach (var item in Setting.Singleton.ExchangeSelectedPairs)
            {
                foreach (var pair in item.Value)
                {
                    if (!allPairs.Contains(pair.Code))
                    {
                        allPairs.Add(pair.Code);
                    }
                }
            }
            return allPairs;
        }

        #region 属性

        #region AllSelectedPairs
        private ObservableCollection<string> allSelectedPairs;
        public ObservableCollection<string> AllSelectedPairs
        {
            get { return allSelectedPairs; }
            set { SetProperty(ref allSelectedPairs, value); }
        }
        #endregion


        #region SelectedPair
        private string selectedPair;
        public string SelectedPair
        {
            get { return selectedPair; }
            set { SetProperty(ref selectedPair, value); }
        }
        #endregion


        #region ExchangeSelectors
        private ObservableCollection<Exchange> exchanges;
        public ObservableCollection<Exchange> Exchanges
        {
            get { return exchanges; }
            set { SetProperty(ref exchanges, value); }
        }
        #endregion


        #region SelectExchange1
        private Exchange selectedExchange1;
        public Exchange SelectedExchange1
        {
            get { return selectedExchange1; }
            set { SetProperty(ref selectedExchange1, value); }
        }
        #endregion


        #region SelectExchange2
        private Exchange selectedExchange2;
        public Exchange SelectedExchange2
        {
            get { return selectedExchange2; }
            set { SetProperty(ref selectedExchange2, value); }
        }
        #endregion


        #region PriceTypes
        public Array PriceTypes
        {
            get
            {
                return Enum.GetValues(typeof(ViewModel.PriceType));
            }
        }
        #endregion


        #region PriceType1
        private PriceType priceType1;
        public PriceType PriceType1
        {
            get { return priceType1; }
            set { SetProperty(ref priceType1, value); }
        }
        #endregion


        #region PriceType2
        private PriceType priceType2;
        public PriceType PriceType2
        {
            get { return priceType2; }
            set { SetProperty(ref priceType2, value); }
        }
        #endregion


        #region PriceDiffrence
        private decimal priceDifference;
        public decimal PriceDifference
        {
            get { return priceDifference; }
            set { SetProperty(ref priceDifference, value); }
        }
        #endregion


        #region AlertColorItem
        private ComboBoxItem alertColorItem;
        public ComboBoxItem AlertColorItem
        {
            get { return alertColorItem; }
            set { SetProperty(ref alertColorItem, value); }
        }
        #endregion

        public ObservableCollection<string> SoundFiles
        {
            get
            {
                var alarmFilesWithPath = Directory.GetFiles("Media");
                ObservableCollection<string> alarmFiles = new ObservableCollection<string>();
                foreach (var item in alarmFilesWithPath)
                {
                    string fileName = item.Replace("Media\\", "");
                    alarmFiles.Add(fileName);
                }
                return alarmFiles;
            }
        }


        #region IsPlaySound
        private bool isPlaySound;
        public bool IsPlaySound
        {
            get { return isPlaySound; }
            set { SetProperty(ref isPlaySound, value); }
        }
        #endregion


        #region SelectedAlarmFile
        private string selectedSoundFile;
        public string SelectedSoundFile
        {
            get { return selectedSoundFile; }
            set { SetProperty(ref selectedSoundFile, value); }
        }
        #endregion
        #endregion

        public DelegateCommand<string> SelectPairCommand { get; set; }
        public DelegateCommand AddAlertCommand { get; set; }
        public DelegateCommand<string> TestSoundCommand { get; set; }


        private void SelectPair(string pairCode)
        {
            Exchanges = new ObservableCollection<Exchange>();

            foreach (var item in apiManager.AllExchangeApi)
            {
                if (item.SelectedPairs.Contains(CurrencyPair.All[pairCode]))
                {
                    Exchanges.Add(item.Exchange);
                }
            }
            //转换：
            if (merges == null) return;

            CurrencyPair pair = CurrencyPair.All[pairCode];
            foreach (var item in Merges)
            {
                if (item.ToCode == pair.Quote.Code)
                {
                    //币不能一样。
                    if (pair.Base.Code == item.FromCode) continue;

                    string mergePairCode = pair.Base.Code + item.FromCode;

                    foreach (var api in apiManager.AllExchangeApi)
                    {
                        if (api.SelectedPairs.Contains(CurrencyPair.All[mergePairCode]))
                        {
                            Exchanges.Add(api.Exchange);
                        }
                    }
                }
            }
        }

        private void TestSound(string soundFile)
        {
            MediaPlayer player = new MediaPlayer();
            Uri uri = new Uri("Media\\" + soundFile, UriKind.Relative);

            player.Open(uri);

            player.Play();
        }

        private void AddAlert()
        {
            try
            {
                CoupleAlertItem alertItem = new CoupleAlertItem();
                if (string.IsNullOrEmpty(selectedPair))
                {
                    throw new Exception(Resources.PleaseSelectTradePair);
                }
                alertItem.PairCode = SelectedPair;
                if (SelectedExchange1 == null)
                {
                    throw new Exception(Resources.PleaseSelectFirstExchange);
                }
                alertItem.Exchange1AbbrName = SelectedExchange1.AbbrName;

                alertItem.PriceType1 = PriceType1;
                if (SelectedExchange2 == null)
                {
                    throw new Exception(Resources.PleaseSelectSecondExchange);
                }
                alertItem.Exchange2AbbrName = SelectedExchange2.AbbrName;
                alertItem.PriceType2 = PriceType2;

                if (AlertColorItem == null)
                {
                    throw new Exception(Resources.PleaseSelectShowColor);
                }
                SolidColorBrush brush = AlertColorItem.Background as SolidColorBrush;
                alertItem.AlertColor = brush.Color;

                alertItem.IsPlaySound = IsPlaySound;
                alertItem.SoundFile = SelectedSoundFile;
                alertItem.PriceDifference = PriceDifference;
                alertItem.IsOpen = true;

                var pair = CurrencyPair.All[alertItem.PairCode];
                var quoteCode = pair.Quote.Code;
                var existMergeItems = from i in Merges where i.ToCode == quoteCode select i;
                if (existMergeItems.Count() > 0)
                {
                    alertItem.ConvertQuoteCode = existMergeItems.FirstOrDefault().FromCode;
                }
                eventAggregator.GetEvent<AddCoupleAlertItemEvent>().Publish(alertItem);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
