using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Properties;
using Microsoft.Practices.Prism.Commands;
using Newtonsoft.Json;

namespace Eureka.CoinTrade.ConfigModule.ViewModel
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ExchangePairSelectorViewModel : BindableBase
    {
        [ImportingConstructor]
        public ExchangePairSelectorViewModel(ApiManager apiManager, RegisterInfo registerInfo)
        {
            this.apiManager = apiManager;
            this.registerInfo = registerInfo;
            ExportCommand = new DelegateCommand(ExportAllSupportPair);
            AllPairSelectorDict = GetAllPairSelectorDict();
            ConfirmSelectedPairCommand = new DelegateCommand(ConfirmSelectedPair);
        }

        private ApiManager apiManager;
        private RegisterInfo registerInfo;


        public Dictionary<Exchange, Collection<CurrencyPairSelector>> AllPairSelectorDict { get; set; }
        public DelegateCommand ConfirmSelectedPairCommand { get; set; }
        public DelegateCommand ExportCommand { get; set; }


        /// <summary>
        /// 选择交易对。
        /// 交易对有激活状态和选择状态。
        /// </summary>
        /// <returns></returns>
        private Dictionary<Exchange, Collection<CurrencyPairSelector>> GetAllPairSelectorDict()
        {
            Exchanges all = apiManager.GetAllExchange();
            var allSupportPairDict = new Dictionary<Exchange, Collection<CurrencyPairSelector>>();

            Dictionary<string, Collection<string>> enabledPairsDict = GetEnabledPairsDict();
            Dictionary<string, Collection<string>> selectedPairsDict = GetSelectedPairsDict();

            foreach (Exchange exchange in all)
            {
                Collection<CurrencyPairSelector> pairs = new Collection<CurrencyPairSelector>();
                foreach (CurrencyPair currencyPair in apiManager.GetApi<ExchangeApi>(exchange.AbbrName).SupportPairs)
                {
                    CurrencyPairSelector currencyPairSelector = new CurrencyPairSelector();
                    currencyPairSelector.CurrencyPair = currencyPair;

                    if (enabledPairsDict.ContainsKey(exchange.AbbrName))
                    {
                        Collection<string> enabledPairs = enabledPairsDict[exchange.AbbrName];
                        Collection<string> selectedPairs;

                        if (selectedPairsDict.ContainsKey(exchange.AbbrName))
                        {
                            selectedPairs = selectedPairsDict[exchange.AbbrName];
                        }
                        //有可能为null，为null时生成空集合。
                        else
                        {
                            selectedPairs = new Collection<string>();
                        }

                        if (enabledPairs.Contains(currencyPair.Code))
                        {
                            if (selectedPairs.Contains(currencyPair.Code))
                            {
                                currencyPairSelector.IsSelected = true;
                            }
                            else
                            {
                                currencyPairSelector.IsSelected = false;
                            }
                            currencyPairSelector.IsEnabled = true;
                        }
                        else
                        {
                            currencyPairSelector.IsSelected = false;
                            currencyPairSelector.IsEnabled = false;
                        }
                    }
                    //可能为null。为null时没激活并且没有选。
                    else
                    {
                        currencyPairSelector.IsSelected = false;
                        currencyPairSelector.IsEnabled = false;
                    }
                    pairs.Add(currencyPairSelector);
                }
                allSupportPairDict.Add(exchange, pairs);
            }
            return allSupportPairDict;
        }

        private static Dictionary<string, Collection<string>> GetSelectedPairsDict()
        {
            //注：如果没有SelectedPairs.key，则在bootstraper中会自动产生一个，同EnablePairs对应的SelectedPairs.key.    
            //由于SelectedPairs.key在API初始化时就要用到，所以放到了Bootstraper中！
            string selectedPairsJson = Setting.ReadJsonFromCompressFile("SelectedPairs.key");
            Dictionary<string, Collection<string>> selectedPairsDict = JsonConvert.DeserializeObject<Dictionary<string, Collection<string>>>(selectedPairsJson);
            return selectedPairsDict;
        }

        private Dictionary<string, Collection<string>> GetEnabledPairsDict()
        {
            Dictionary<string, Collection<string>> enabledPairsDict;
            if (registerInfo.EnabledPairs != null)
            {
                enabledPairsDict = registerInfo.EnabledPairs;
            }
            else
            {
                if (!File.Exists("AllSupportPair.key"))
                {
                    //输出AllSupportPair.key进行缓存，以及AllSupporPiar.txt供Register使用。
                    //删除AllSupportPair.key可以重新生成一个。
                    //AllSupportPair.key的用途在于SelectedPair.key没有时拷贝它，以及这里EnabledPairs里获取它。
                    ExportAllSupportPair();
                }
                string allSupportPairs = Setting.ReadJsonFromCompressFile("AllSupportPair.key");
                enabledPairsDict = JsonConvert.DeserializeObject<Dictionary<string, Collection<string>>>(allSupportPairs);
            }
            return enabledPairsDict;
        }

        //如果没有AllSupportPair.key则生成一个。
        private void ExportAllSupportPair()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.None;
                writer.WriteStartObject();
                foreach (var item in apiManager.AllExchangeApi)
                {
                    writer.WritePropertyName(item.Exchange.AbbrName);
                    writer.WriteStartArray();
                    foreach (var pair in item.SupportPairs)
                    {
                        writer.WriteValue(pair.Code);
                    }
                    writer.WriteEndArray();
                }
                writer.WriteEndObject();
            }
            var text = sb.ToString();

            Setting.WriteJsonToCompressFile("AllSupportPair.key", text);
            File.WriteAllText("AllSupportPair.txt", text);
        }

        /// <summary>
        /// 确认选择关注的交易对。
        /// </summary>
        private void ConfirmSelectedPair()
        {
            Dictionary<string, Collection<string>> selectedPairs = new Dictionary<string, Collection<string>>();
            foreach (var item in AllPairSelectorDict)
            {
                Collection<string> pairs = new Collection<string>();
                foreach (var pair in item.Value)
                {
                    if (pair.IsSelected)
                    {
                        pairs.Add(pair.CurrencyPair.Code);
                    }
                }
                selectedPairs.Add(item.Key.AbbrName, pairs);
            }
            var json = JsonConvert.SerializeObject(selectedPairs);
            Setting.WriteJsonToCompressFile("selectedPairs.key", json);
            EventSourceLogger.Logger.UpdateSetting(Resources.PairSelected);
        }

    }
}
