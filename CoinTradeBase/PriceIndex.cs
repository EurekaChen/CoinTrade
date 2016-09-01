using System;
using System.ComponentModel;

namespace Eureka.CoinTrade.Base
{
    /// <summary>
    /// 价格指数。不一定是货币对的指数，例如股票指数。
    /// </summary>
    public  class PriceIndex : BindableBase
    {
        public PriceIndex(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        private decimal index;
        public decimal Index
        {
            get
            {
                return index;
            }
            set
            {
                SetProperty(ref index, value);
            }
        }

        private DateTime dateTime;
        public DateTime DateTime
        {
            get
            {
                return dateTime;
            }
            set
            {
                SetProperty(ref dateTime, value);
            }
        }
    }
}
