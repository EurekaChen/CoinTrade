using System.ComponentModel.Composition;
using Eureka.CoinTrade.Infrastructure;
using MahApps.Metro.Controls;

namespace Eureka.CoinTrade.TradeModule.View
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TradeTabView : MetroTabItem, IView
    {
        public TradeTabView()
        {
            InitializeComponent();

        }
    }
}
