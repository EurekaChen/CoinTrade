
using Eureka.CoinTrade.Base;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Microsoft.Practices.Prism.ViewModel;
using System.ComponentModel;

namespace Eureka.CoinTrade.Infrastructure
{
    /// <summary>
    /// 指一个报价币的所有交易对及其在所有不同交易所的报价。
    /// </summary>    
    public class MarketTickers : KeyedCollection<string, TickersByExchange>, INotifyPropertyChanged
    {
        private decimal convertRate;
        public decimal ConvertRate
        {
            get
            {
                return convertRate;
            }
            set
            {
                convertRate = value;
                RaisePropertyChanged("ConvertRate");
            }
        }
        public Currency Quote
        {
            get
            {
                if (this.Count > 0)
                {
                    return this.First().CurrencyPair.Quote;
                }
                else
                {
                    return null;
                }
            }
        }
        protected override string GetKeyForItem(TickersByExchange item)
        {
            return item.CurrencyPair.Code;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
