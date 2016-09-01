using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Properties;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;

namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    [Export]
    public class MultipleAddAlertViewModel : BindableBase
    {
        [Import]
        ApiManager apiManager;

        [Import]
        IEventAggregator eventAggregator;
        public MultipleAddAlertViewModel()
        {
            AllSelectedPairs = GetAllSelectedPairs();

            SelectPairCommand = new DelegateCommand<string>(SelectPair);
            AddAlertCommand = new DelegateCommand(AddAlert);
            TestSoundCommand = new DelegateCommand<string>(TestSound);
        }

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
        private ObservableCollection<AbbrNameSelector> exchangeSelectors;
        public ObservableCollection<AbbrNameSelector> ExchangeSelectors
        {
            get { return exchangeSelectors; }
            set { SetProperty(ref exchangeSelectors, value); }
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


        #region PriceType
        private PriceType priceType;
        public PriceType PriceType
        {
            get { return priceType; }
            set { SetProperty(ref priceType, value); }
        }
        #endregion

        #region IsHighThan
        private bool isHighThan;
        public bool IsHighThan
        {
            get { return isHighThan; }
            set { SetProperty(ref isHighThan, value); }
        }
        #endregion

        #region ComparePrice
        private decimal comparePrice;
        public decimal ComparePrice
        {
            get { return comparePrice; }
            set { SetProperty(ref comparePrice, value); }
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


        private void SelectPair(string pair)
        {
            ExchangeSelectors = new ObservableCollection<AbbrNameSelector>();

            foreach (var item in apiManager.AllExchangeApi)
            {
                if (item.SelectedPairs.Contains(CurrencyPair.All[pair]))
                {
                    ExchangeSelectors.Add(new AbbrNameSelector() { AbbrName = item.Exchange.AbbrName, IsSelected = true });
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
                MultipleAlertItem alertItem = new MultipleAlertItem();
                if (string.IsNullOrEmpty(selectedPair))
                {
                    throw new Exception(Resources.PleaseSelectTradePair);
                }
                alertItem.PairCode = SelectedPair;


                alertItem.PriceType = PriceType;

                alertItem.AbbrNamePrices = new List<AbbrNamePrice>();
                foreach (var item in ExchangeSelectors)
                {
                    if (item.IsSelected)
                    {
                        alertItem.AbbrNamePrices.Add(new AbbrNamePrice() { AbbrName = item.AbbrName });
                    }
                }
                if (alertItem.AbbrNamePrices.Count == 0)
                {
                    throw new Exception(Resources.AtLeastOneExchange);
                }

                alertItem.IsHighThan = IsHighThan;

                alertItem.ComparePrice = ComparePrice;

                if (AlertColorItem == null)
                {
                    throw new Exception(Resources.PleaseSelectShowColor);
                }
                SolidColorBrush brush = AlertColorItem.Background as SolidColorBrush;
                alertItem.AlertColor = brush.Color;

                alertItem.IsPlaySound = IsPlaySound;
                alertItem.SoundFile = SelectedSoundFile;

                alertItem.IsOpen = true;

                eventAggregator.GetEvent<AddMultipleAlertItemEvent>().Publish(alertItem);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
