using System.Diagnostics.Tracing;

namespace Eureka.CoinTrade.Infrastructure
{
    public partial class EventSourceLogger
    {
        public static class Tasks
        {
            public const EventTask Application = (EventTask)11;
            public const EventTask Prism = (EventTask)12;
            public const EventTask Module = (EventTask)13;
            public const EventTask Presenter = (EventTask)14;
            public const EventTask Register = (EventTask)15;
            public const EventTask Config = (EventTask)16;

            public const EventTask UpdateRate = (EventTask)21;
            public const EventTask UpdateData = (EventTask)22;


            public const EventTask QueryFund = (EventTask)31;
            public const EventTask SubmitOrder = (EventTask)32;
            public const EventTask CancelOrder = (EventTask)33;
            public const EventTask QueryFee = (EventTask)34;
            public const EventTask QueryOpenOrders = (EventTask)35;
        }
    }
}
