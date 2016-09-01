using System.IO;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;

namespace Eureka.CoinTrade.Infrastructure
{
    public class Option : BindableBase
    {
        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            Setting.WriteJsonToCompressFile("option.key", json);
        }
        public static Option Load()
        {
            if (File.Exists("option.key"))
            {
                string optionJson = Setting.ReadJsonFromCompressFile("option.key");
                Option option = JsonConvert.DeserializeObject<Option>(optionJson);
                return option;
            }
            else
            {
                Option option = new Option();
                option.IsAutoRate = true;
                option.IsAutoTicker = true;
                option.IsAutoFund = false;
                option.Timeout = 15000;
                option.BitstampClientId = "";
                option.Accent = "Blue";
                option.Theme = "Light";
                option.LocalCurrencyCode = "CNY";

                var json = JsonConvert.SerializeObject(option);
                Setting.WriteJsonToCompressFile("option.key", json);
                return option;
            }
        }


        #region LocalCurrencyInfo
        private string localCurrencyCode;
        public string LocalCurrencyCode
        {
            get { return localCurrencyCode; }
            set { SetProperty(ref localCurrencyCode, value); }
        }
        #endregion


        #region LanguageCode
        private string languageCode;
        public string LanguageCode
        {
            get { return languageCode; }
            set { SetProperty(ref languageCode, value); }
        }
        #endregion



        private bool isAutoRate;
        public bool IsAutoRate
        {
            get
            {
                return isAutoRate;
            }
            set
            {
                SetProperty(ref isAutoRate, value);
            }
        }


        private bool isAutoTicker;
        public bool IsAutoTicker
        {
            get
            {
                return isAutoTicker;
            }
            set
            {
                SetProperty(ref isAutoTicker, value);
            }
        }

        private bool isAutoFund;
        public bool IsAutoFund
        {
            get
            {
                return isAutoFund;
            }
            set
            {
                SetProperty(ref isAutoFund, value);
            }
        }


        #region Timeout
        private int timeout = 15000;
        public int Timeout
        {
            get { return timeout; }
            set
            {
                if (value < 1000)
                {
                    timeout = 1000;
                }
                else if (value > 300000)
                {
                    timeout = 300000;
                }
                else
                {
                    timeout = value;
                }
                OnPropertyChanged("Timeout");
            }
        }
        #endregion


        #region Theme
        private string theme = "Blue";  //缺省蓝色方案
        public string Theme
        {
            get { return theme; }
            set { SetProperty(ref theme, value); }
        }
        #endregion


        #region Accent
        private string accent = "Light"; //缺省亮色主题
        public string Accent
        {
            get { return accent; }
            set { SetProperty(ref accent, value); }
        }
        #endregion


        #region BitstampClientId
        private string bitstampClientId;
        public string BitstampClientId
        {
            get { return bitstampClientId; }
            set { SetProperty(ref bitstampClientId, value); }
        }
        #endregion

    }
}
