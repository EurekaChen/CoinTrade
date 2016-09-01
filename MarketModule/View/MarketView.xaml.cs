using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Eureka.CoinTrade.MarketModule.View
{
    [Export]
    public partial class MarketView : ItemsControl
    {
        public MarketView()
        {
            InitializeComponent();
        }
    }
}
