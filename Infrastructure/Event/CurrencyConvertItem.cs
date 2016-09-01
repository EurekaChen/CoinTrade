
namespace Eureka.CoinTrade.Infrastructure.Event
{
    public class CurrencyConvertItem
    {
        public string FromCode { get; set; }
        public string ToCode { get; set; }
        public decimal Rate { get; set; }

        public bool IsConvert { get { return Rate != 1m; } }
    }
}
