using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eureka.CoinTrade.ApiBase;
using Microsoft.Practices.Prism.MefExtensions;

namespace Eureka.CoinTrade.Infrastructure
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ApiManager
    {
        [Import(typeof(CompositionContainer))]
        CompositionContainer Container;


        [ImportMany(typeof(ExchangeApi))]
        public Collection<ExchangeApi> AllExchangeApi;       

        [ImportMany(typeof(IAuthApi))]
        public Collection<IAuthApi> AuthApis;

        [ImportMany(typeof(IFundApi))]
        public Collection<IFundApi> FundApis;

        [ImportMany(typeof(ISubmitOrderApi))]
        public Collection<ISubmitOrderApi> TradableApis;
       

        private Exchanges allExchange;
        public Exchanges GetAllExchange()
        {
            if (allExchange == null)
            {
                allExchange = new Exchanges();
                foreach (ExchangeApi api in AllExchangeApi)
                {
                    allExchange.Add(api.Exchange);
                }
            }
            return allExchange;
        }      


        public T GetApi<T>(string exchangeAbbrName)
        {
            IEnumerable<T> apis = Container.GetExportedValues<T>();
            foreach (T api in apis)
            {
                ExchangeApi exchangeApi = api as ExchangeApi;
                if (exchangeApi.Exchange.AbbrName == exchangeAbbrName)
                {
                    return api;
                }
            }
            return default(T);

        }
    }
}
