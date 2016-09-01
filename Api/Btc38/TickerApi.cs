using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.ApiBase;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Eureka.CoinTrade.Infrastructure;
using System.ComponentModel.Composition;

namespace Eureka.CoinTrade.Api.Btc38
{
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerApi : Btc38Api,ITickerApi
    {
        [Import]
        ApiManager apiManager;
        public void UpdateTicker(ref Ticker ticker)
        {
            Quotation quotation = new Quotation();
            quotation.Ticker = ticker;
            IQuotationApi quotationUpdater =apiManager.GetApi<IQuotationApi>(Exchange.AbbrName);
            quotationUpdater.UpdateQuotation(ref quotation);      
        }

    }
}
