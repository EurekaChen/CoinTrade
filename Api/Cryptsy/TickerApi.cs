using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
using Eureka.CoinTrade.ApiBase;
namespace Eureka.CoinTrade.Api.Cryptsy
{  
    [Export(typeof(ITickerApi))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class  TickerApi : CryptsyApi,ITickerApi
    {
        [Import]
        ApiManager apiManager;
        
        public void UpdateTicker(ref Ticker ticker)
        {          
            Quotation quotation = new Quotation();
            quotation.Ticker = ticker;  
            IQuotationApi quotationUpdater = apiManager.GetApi<IQuotationApi>(Exchange.AbbrName);
            quotationUpdater.UpdateQuotation(ref quotation);           
        }        
    }
}
