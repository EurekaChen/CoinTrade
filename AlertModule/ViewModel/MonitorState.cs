
using System.ComponentModel;
namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    [TypeConverter(typeof(MonitorStateConverter))]
    public enum MonitorState
    {
        Stop,
        Start,
        Pause,
        DataError,
        NotTriggered,
        Triggered
    }
}
