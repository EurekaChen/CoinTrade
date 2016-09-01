using System.Diagnostics.Tracing;


namespace Eureka.CoinTrade.Infrastructure
{
    public partial class EventSourceLogger
    {
        public static class Opcodes
        {
            //public const EventOpcode Info = EventOpcode.Info;
            
            public const EventOpcode BeginDownloadAsync = (EventOpcode)101;
            public const EventOpcode BeginAuthQuery = (EventOpcode)102;

            //240缺省的Recieve. 所以我从301开始：

            public const EventOpcode DownloadAsyncSuccess = (EventOpcode)301;
            public const EventOpcode AuthQuerySuccess = (EventOpcode)302;
            public const EventOpcode Exception = (EventOpcode)303;
            public const EventOpcode AsyncError = (EventOpcode)304;
        }
    }
}
