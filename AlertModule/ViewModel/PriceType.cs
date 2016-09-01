
using System.ComponentModel;
namespace Eureka.CoinTrade.AlertModule.ViewModel
{
    //在CombBox中转换不起作用！！
    [TypeConverter(typeof(PriceTypeConverter))]
    public enum PriceType
    {
        Last,
        Sell,
        Buy,
        High,
        Low
    }
}
