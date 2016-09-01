using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    public class AuthKeyViewModel : BindableBase
    {
        private ApiManager apiManager;

        [ImportingConstructor]
        public AuthKeyViewModel(ApiManager apiManager)
        {
            this.apiManager = apiManager;
            AuthKeys = Setting.Singleton.GetAuthKeyDict();
            ConfirmCommand = new DelegateCommand(Confirm);
        }
        public Dictionary<string, AuthKey> AuthKeys { get; private set; }
        public DelegateCommand ConfirmCommand { get; set; }

        private void Confirm()
        {
            var json = JsonConvert.SerializeObject(AuthKeys);
            Setting.WriteJsonToCompressFile("Api.key", json);
            string yourKeySaved = Resources.YourKeySaved;
            EventSourceLogger.Logger.UpdateSetting(yourKeySaved);
        }
    }
}
