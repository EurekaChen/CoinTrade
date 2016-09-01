
namespace Eureka.CoinTrade.Base
{
    public class OrderResult
    {
        public bool IsSuccess { get; set; }
        public int OrderId { get; set; }
        public string Info { get; set; }
        public bool IsFail { get { return !IsSuccess; } }
    }
}
