using Eureka.CoinTrade.Infrastructure.Properties;
using Eureka.Localization;
namespace Eureka.CoinTrade.ChartModule.ViewModel
{
    public class PeriodTypeConverter : ResourceEnumConverter
    {
        public PeriodTypeConverter()
            : base(typeof(PeriodType), Resources.ResourceManager)
        {
        }
    }
}
