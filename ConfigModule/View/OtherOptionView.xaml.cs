using System.ComponentModel.Composition;
using System.Windows.Controls;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.ConfigModule.View
{

    [Export]
    public partial class OtherOptionView : UserControl, IView
    {
        public OtherOptionView()
        {
            InitializeComponent();
        }
    }
}
