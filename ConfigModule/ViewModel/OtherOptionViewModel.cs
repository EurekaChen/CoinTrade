using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.Infrastructure.Properties;
using Microsoft.Practices.Prism.Commands;

namespace Eureka.CoinTrade.ConfigModule.ViewModel
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OtherOptionViewModel : BindableBase
    {

        public OtherOptionViewModel()
        {
            option = Setting.Singleton.Option;
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        private Option option;
        public Option Option
        {
            get
            {
                return option;
            }
            set
            {
                SetProperty(ref option, value);
            }
        }

        public Collection<string> LegalCurrencies
        {
            get
            {
                Collection<string> legalCurrencies = new Collection<string>();
                foreach (var item in LegalCurrency.All)
                {
                    string info = item.Code;
                    legalCurrencies.Add(info);
                }
                return legalCurrencies;
            }
        }


        public DelegateCommand ConfirmCommand { get; set; }

        private void Confirm()
        {
            Option.Save();
            EventSourceLogger.Logger.UpdateSetting(Resources.ConfigOtherOption);
        }

    }
}
