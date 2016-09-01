
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Properties;
using Microsoft.Practices.Prism.Commands;

namespace Eureka.CoinTrade.ChartModule.ViewModel
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ChartMainViewModel : BindableBase
    {
        [Import]
        ApiManager apiManager;
        public ChartMainViewModel()
        {
            AllChartPairs = GetAllChartPairs();
            GetExchangeAbbrNamesCommand = new DelegateCommand<string>(GetExchangeAbbrNames);
            GetChartCommand = new DelegateCommand(GetChart);
        }


        Dictionary<string, string> chartExchange = new Dictionary<string, string>()
        {
            {"okcoin","OKCoin"},
            {"btc-e","Btce"},
            {"btcchina","BtcChina"},
            {"bitstamp","Bitstamp"},
            {"mtgox","MtGox"},
            {"fxbtc","Fxbtc"},
            {"cryptsy","Cryptsy"},
            {"bter","Bter"},
            {"btcltc","Btcltc"},
            {"crypto-trade","CryptoTrade"}
        };

        #region 属性
        #region AllSelectedPairs
        private ObservableCollection<string> allChartPairs;
        public ObservableCollection<string> AllChartPairs
        {
            get { return allChartPairs; }
            set { SetProperty(ref allChartPairs, value); }
        }
        #endregion


        #region SelectedPairCode
        private string selectedPairCode;
        public string SelectedPairCode
        {
            get { return selectedPairCode; }
            set { SetProperty(ref selectedPairCode, value); }
        }
        #endregion

        #region PeriodTypes
        public Array PeriodTypes
        {
            get
            {
                //这样测试也没办法得到中文！好象是得不到PeriodTypeConverter，依旧使用EnumConverter获得数据！
                //return PeriodTypeConverter.GetValues(typeof(ViewModel.PeriodType);
                return Enum.GetValues(typeof(ViewModel.PeriodType));
            }
        }
        #endregion


        #region SelectedPeriodType
        private PeriodType selectedPeriodType;
        public PeriodType SelectedPeriodType
        {
            get { return selectedPeriodType; }
            set { SetProperty(ref selectedPeriodType, value); }
        }
        #endregion


        #region Exchanges
        private ObservableCollection<string> exchangeAbbrNames;
        public ObservableCollection<string> ExchangeAbbrNames
        {
            get { return exchangeAbbrNames; }
            set { SetProperty(ref exchangeAbbrNames, value); }
        }
        #endregion


        #region SelectExchange
        private string selectedExchangeAbbrName;
        public string SelectedExchangeAbbrName
        {
            get { return selectedExchangeAbbrName; }
            set { SetProperty(ref selectedExchangeAbbrName, value); }
        }
        #endregion
        #endregion

        public DelegateCommand<string> GetExchangeAbbrNamesCommand { get; set; }
        public DelegateCommand GetChartCommand { get; set; }

        private void GetChart()
        {
            if (string.IsNullOrEmpty(SelectedPairCode))
            {
                MessageBox.Show(Resources.PleaseSelectTradePair);
                return;
            }

            if (string.IsNullOrEmpty(SelectedExchangeAbbrName))
            {
                MessageBox.Show(Resources.PleaseSelectExchange);
                return;
            }

            string url = "http://www.cryptocoincharts.info/v2/pair/";
            string baseCurrenyCode = CurrencyPair.All[SelectedPairCode].Base.Code.ToLower();
            url += baseCurrenyCode + "/";

            string quote = CurrencyPair.All[SelectedPairCode].Quote.Code.ToLower();
            url += quote + "/";

            string exchange = chartExchange.FirstOrDefault(x => x.Value.Contains(SelectedExchangeAbbrName)).Key;

            url += exchange + "/";

            string period = "10-days";
            switch (SelectedPeriodType)
            {
                case PeriodType.AllTime:
                    period = "alltime";
                    break;
                case PeriodType.YearToDate:
                    period = "ytd";
                    break;
                case PeriodType.Months6:
                    period = "6-months";
                    break;
                case PeriodType.Months3:
                    period = "3-months";
                    break;
                case PeriodType.Month1:
                    period = "1-month";
                    break;
                case PeriodType.Days10:
                    period = "10-days";
                    break;
                case PeriodType.Days5:
                    period = "5-days";
                    break;
                case PeriodType.Days2:
                    period = "2-days";
                    break;
                case PeriodType.Today:
                    period = "today";
                    break;
            }
            string periodUrl = url + period;

            GetPeriodChart(periodUrl);
            //既然深度数据一样，所以就用today 以减少换行之苦
            string depthUrl = url + "today";
            if (selectedExchangeAbbrName == "CryptoTrade" || SelectedPairCode == "LTCBTC")
            {
                GetOneWrapDepthChart(url);
            }
            else
            {
                GetDepthChart(depthUrl);
            }

        }
        private void GetExchangeAbbrNames(string selectPair)
        {
            ExchangeAbbrNames = new ObservableCollection<string>();

            foreach (var item in apiManager.AllExchangeApi)
            {
                if (item.SelectedPairs.Contains(CurrencyPair.All[selectPair]))
                {
                    if (chartExchange.ContainsValue(item.Exchange.AbbrName))
                    {
                        ExchangeAbbrNames.Add(item.Exchange.AbbrName);
                    }
                }
            }
        }

        private ObservableCollection<string> GetAllChartPairs()
        {
            ObservableCollection<string> allPairs = new ObservableCollection<string>();

            foreach (var item in Setting.Singleton.ExchangeSelectedPairs)
            {
                if (chartExchange.ContainsValue(item.Key))
                {
                    foreach (var pair in item.Value)
                    {
                        if (!allPairs.Contains(pair.Code))
                        {
                            allPairs.Add(pair.Code);
                        }
                    }
                }
            }
            return allPairs;
        }

        /// <remarks>
        /// json示例：
        /// [
        ///     {"id":"adt","name":"ADT","website":"","price_btc":"0.00000002","volume_btc":"22.4254333286391"},
        ///     {"id":"alf","name":"AlphaCoin","website":"http:\/\/cur.lv\/5rzs5","price_btc":"0.00000583","volume_btc":"2.72975115341219"},
        ///     {"id":"alp","name":"ALP","website":"","price_btc":"0.00000700","volume_btc":"0.0345706868942931"},
        ///     {"id":"amc","name":"AmericanCoin","website":"","price_btc":"0.00001120","volume_btc":"0.494095631819897"},
        ///     ...
        ///     {"id":"zet","name":"ZetaCoin","website":"http:\/\/cur.lv\/5rzoe","price_btc":"0.00003115","volume_btc":"22.1378650476046"}
        /// ]
        /// </remarks>
        public void GetAllPair()
        {
            string url = "http://www.cryptocoincharts.info/v2/api/listCoins";
            using (WebClient webClient = new WebClient())
            {
                // EventSourceLogger.Logger.BeginDownloadDataAsync(DataTypeDict["Ticker"], Exchange.Name, ticker.CurrencyPair.Code);
                string json = webClient.DownloadString(url);
            }
        }


        public static WebBrowser PeriodBrowser { get; set; }
        public static WebBrowser DepthBrowser { get; set; }


        private void GetPeriodChart(string url)
        {
            string peroidhtml = HtmlString.GetPeriodHtml(url);
            PeriodBrowser.NavigateToString(peroidhtml);
        }

        private void GetDepthChart(string url)
        {
            string depthHtml = HtmlString.GetDepthHtml(url);
            DepthBrowser.NavigateToString(depthHtml);
        }

        private void GetOneWrapDepthChart(string url)
        {
            string depthHtml = HtmlString.GetOneWrapDepthHtml(url);
            DepthBrowser.NavigateToString(depthHtml);
        }
    }
}
