using System.Collections.ObjectModel;
using Eureka.CoinTrade.Base;

namespace Eureka.CoinTrade.ApiBase
{
    public class History : ObservableCollection<Deal>
    {
        public Ticker Ticker { get; private set; }
        public History(Ticker ticker)
        {
            this.Ticker = ticker;
        }
    }
}
