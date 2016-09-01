using Eureka.CoinTrade.Infrastructure.Properties;
using Eureka.Localization;
namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    public class PriceTypeConverter : ResourceEnumConverter
    {
        public PriceTypeConverter()
            : base(typeof(PriceType), Resources.ResourceManager)
        {
        }
    }
}
