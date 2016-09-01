using System.ComponentModel.Composition;
using System.Windows.Controls;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.FundModule.View
{
   
    [Export]
    public partial class FundMainView : UserControl,IView
    {
        public FundMainView()
        {
            InitializeComponent();
        }
    }
}
