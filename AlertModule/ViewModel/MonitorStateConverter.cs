using Eureka.CoinTrade.Infrastructure.Properties;
using Eureka.Localization;
namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    public class MonitorStateConverter : ResourceEnumConverter
    {
        public MonitorStateConverter()
            : base(typeof(MonitorState), Resources.ResourceManager)
        {
        }
    }
}
