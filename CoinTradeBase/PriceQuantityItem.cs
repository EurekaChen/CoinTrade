
using System;
namespace Eureka.CoinTrade.Base
{
    public class PriceQuantityItem
    {
        private decimal convertRate = 1;
        public decimal ConvertRate
        {
            get { return convertRate; }
            set { convertRate = value; }
        }

        private decimal price;
        public decimal OrginalPrice { get { return price; } }
        public decimal Price
        {
            get
            {
                return price * ConvertRate;
            }
            set
            {
                price = value;
            }
        }
        public decimal Quantity { get; set; }
        public decimal Total { get { return Price * Quantity; } }
    }
}
