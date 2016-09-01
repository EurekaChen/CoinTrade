using System;

namespace Eureka.CoinTrade.Base
{
    public class Deal
    {
        public int Id { get; set; }
        public DateTime DealTime { get; set; }
        public DealType DealType { get; set; }
        public PriceQuantityItem PriceQuantity { get; set; }       
    }
}
