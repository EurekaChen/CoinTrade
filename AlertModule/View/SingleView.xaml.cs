using System.ComponentModel.Composition;
using System.Windows.Controls;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.AlertModule.View
{
    [Export]
    public partial class SingleView : UserControl, IView
    {
        public SingleView()
        {
            InitializeComponent();
        }
       
    }
}
