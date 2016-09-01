using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.Api.Btc38
{

    [Export(typeof(IHistoryApi))]

    public class HistoryApi : Btc38Api, IHistoryApi
    {
        [Import]
        ApiManager apiManager;
        //BTC38现只提供一个API，通过Qutaion进行更新
        public void UpdateHistory(ref History history)
        {
            Quotation quotation = new Quotation();
            quotation.History = history;
            quotation.Ticker = history.Ticker;
            IQuotationApi quotationUpdater = apiManager.GetApi<IQuotationApi>(Exchange.AbbrName);
            quotationUpdater.UpdateQuotation(ref quotation);
        }
    }
}
