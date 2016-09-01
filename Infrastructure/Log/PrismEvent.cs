using Microsoft.Practices.Prism.Logging;
using System.Diagnostics.Tracing;

namespace Eureka.CoinTrade.Infrastructure
{
    public partial class EventSourceLogger
    {
        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    PrismDebug(message, priority);
                    break;
                case Category.Exception:
                    PrismException(message, priority);
                    break;
                case Category.Warn:
                    PrismWarn(message, priority);
                    break;
                case Category.Info:
                default:
                    PrismInfo(message, priority);
                    break;
            }
        }

        //EventId不能重复！！不然不会被侦听！

        [Event(22001, Level = EventLevel.Error, Message = "{0}  (优先级:{1})", Keywords = Keywords.Prism, Task = Tasks.Prism, Opcode = Opcodes.Exception, Version = 1)]
        public void PrismException(string message, Priority priority)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(22001, message, priority);
            }
        }

        [Event(22002, Level = EventLevel.Critical, Message = "{0}  (优先级{1})", Keywords = Keywords.Prism, Task = Tasks.Prism,  Version = 1)]
        public void PrismDebug(string message, Priority priority)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(22002, message, priority);
            }
        }



        [Event(22003, Level = EventLevel.Warning, Message = "{0}  (优先级:{1})", Keywords = Keywords.Prism, Task = Tasks.Prism,  Version = 1)]
        public void PrismWarn(string message, Priority priority)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(22003, message, priority);
            }
        }

        [Event(22004, Level = EventLevel.Informational, Message = "{0}  (优先级:{1})", Keywords = Keywords.Prism, Task = Tasks.Prism, Version = 1)]
        public void PrismInfo(string message, Priority priority)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(22004, message, priority);
            }
        }
    }
}
