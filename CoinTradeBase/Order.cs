using System;

namespace Eureka.CoinTrade.Base
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderTime { get; set; }
        public OrderType OrderType { get; set; }
        public PriceQuantityItem PriceQuantity { get; set; }       
    }
}
