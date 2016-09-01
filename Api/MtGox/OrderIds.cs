using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Eureka.CoinTrade.Api.MtGox
{
    public class OrderIds : Collection<string>
    {
        private static OrderIds singleton;
        public static OrderIds Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new OrderIds();
                }
                return singleton;
            }
        }


    }
}
