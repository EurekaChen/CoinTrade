using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Properties;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    [Export]
    public class SingleAddAlertViewModel : BindableBase
    {
        [Import]
        ApiManager apiManager;

        [Import]
        IEventAggregator eventAggregator;
        public SingleAddAlertViewModel()
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
        private ObservableCollection<Exchange> exchanges;
        public ObservableCollection<Exchange> Exchanges
        {
            get { return exchanges; }
            set { SetProperty(ref exchanges, value); }
        }
        #endregion


        #region SelectExchange
        private Exchange selectedExchange;
        public Exchange SelectedExchange
        {
            get { return selectedExchange; }
            set { SetProperty(ref selectedExchange, value); }
        }
        #endregion


        #region PriceTypes
        public Array PriceTypes
        {
            get
            {
                //这样测试也没办法得到中文！好象是得不到PriceTypeConverter，依旧使用EnumConverter获得数据！
                //return PriceTypeConverter.GetValues(typeof(ViewModel.PriceType);
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
            Exchanges = new ObservableCollection<Exchange>();

            foreach (var item in apiManager.AllExchangeApi)
            {
                if (item.SelectedPairs.Contains(CurrencyPair.All[pair]))
                {
                    Exchanges.Add(item.Exchange);
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
                SingleAlertItem alertItem = new SingleAlertItem();
                if (string.IsNullOrEmpty(selectedPair))
                {
                    throw new Exception(Resources.PleaseSelectTradePair);
                }
                alertItem.PairCode = SelectedPair;
                if (SelectedExchange == null)
                {
                    throw new Exception(Resources.PleaseSelectExchange);
                }
                alertItem.ExchangeAbbrName = SelectedExchange.AbbrName;
                //if (string.IsNullOrEmpty(PriceType))
                //{
                //    throw new Exception("请选择第一个交易所的价格类型");
                //}
                alertItem.PriceType = PriceType;
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

                eventAggregator.GetEvent<AddSingleAlertItemEvent>().Publish(alertItem);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
