using System.Collections.Generic;
using Eureka.CoinTrade.Base;

namespace Eureka.CoinTrade.ApiBase
{
    public class ExchangeFund
    {
        public ExchangeFund(Exchange exchange)
        {
            this.Exchange = exchange;
        }
        public Exchange Exchange { get; private set; }
        public Dictionary<Currency, decimal> Available { get; set; }

        /// <summary>
        /// 冻结资金。如果为null说明API暂不支持 
        /// </summary>
        public Dictionary<Currency, decimal> Frozen { get; set; }

        public bool CanNotGetFrozen
        {
            get
            {
                if (Frozen == null) return true;
                return false;
            }
        }


        private Dictionary<Currency, decimal> total;
        public Dictionary<Currency, decimal> Total
        {
            get
            {
                total = new Dictionary<Currency, decimal>();

                foreach (var availableKv in Available)
                {
                    if (Frozen != null && Frozen.ContainsKey(availableKv.Key))
                    {
                        total.Add(availableKv.Key, availableKv.Value + Frozen[availableKv.Key]);
                    }
                    else
                    {
                        total.Add(availableKv.Key, availableKv.Value);
                    }
                }

                if (Frozen != null)
                {
                    //加上只有冻结的部分。
                    foreach (var frozenKv in Frozen)
                    {
                        if (!total.ContainsKey(frozenKv.Key))
                        {
                            total.Add(frozenKv.Key, frozenKv.Value);
                        }
                    }
                }

                return total;
            }

        }
    }
}
