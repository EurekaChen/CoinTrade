using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
